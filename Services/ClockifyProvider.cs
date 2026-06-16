using System.Text.Json;
using bdt_evm_app.DTOs;

namespace bdt_evm_app.Services;

/// <summary>
/// Adapter de Clockify hacia <see cref="ITimesheetProvider"/>. Es el ÚNICO punto del
/// backend que conoce el formato del JSON de Clockify: todo el mapping crudo→DTO neutro
/// vive acá. El transporte HTTP (paginado, Reports API, fallback) queda en
/// <see cref="ClockifyService"/>; esta clase solo traduce.
///
/// El mapping es 1:1 y NO filtra: a cada elemento crudo le corresponde un DTO. Esto
/// preserva la semántica de paginación de los callers, que cuentan `Count == pageSize`
/// para decidir si hay más páginas. Los elementos inválidos (sin id, sin start) se
/// descartan más adelante en TimesheetSyncService.UpsertEntryAsync.
/// </summary>
public class ClockifyProvider : ITimesheetProvider
{
    private readonly ClockifyService _clockify;

    public ClockifyProvider(ClockifyService clockify)
    {
        _clockify = clockify;
    }

    public string? DefaultUserExternalId => _clockify.GetUserId();

    public async Task<IReadOnlyList<TimesheetClientDto>> GetClientsAsync()
        => (await _clockify.GetClients()).Select(MapClient).ToList();

    public async Task<IReadOnlyList<TimesheetUserDto>> GetUsersAsync(bool onlyActive = false)
        => (await _clockify.GetUsers(onlyActive)).Select(MapUser).ToList();

    public async Task<IReadOnlyList<TimesheetProjectDto>> GetProjectsAsync()
        => (await _clockify.GetProjects()).Select(MapProject).ToList();

    public async Task<IReadOnlyList<TimesheetEntryDto>> GetEntriesForUserAsync(string userExternalId, string? start, string? end, int page, int pageSize)
        => (await _clockify.GetTimeEntries(userExternalId, start, end, page, pageSize)).Select(MapEntry).ToList();

    public async Task<IReadOnlyList<TimesheetEntryDto>> GetEntriesForProjectAsync(string projectExternalId, string? start, string? end, int page, int pageSize)
        => (await _clockify.GetTimeEntriesByProject(projectExternalId, start, end, page, pageSize)).Select(MapEntry).ToList();

    public async IAsyncEnumerable<TimesheetEntryDto> GetAllEntriesAsync(string? start, string? end)
    {
        await foreach (var e in _clockify.GetAllUsersTimeEntries(start, end))
            yield return MapEntry(e);
    }

    // ---- Mapping crudo (shape Clockify) → DTO neutro ----

    private static TimesheetClientDto MapClient(JsonElement c) => new()
    {
        ExternalId = c.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
        Name = c.TryGetProperty("name", out var n) ? n.GetString() ?? "Cliente sin nombre" : "Cliente sin nombre",
        Archived = c.TryGetProperty("archived", out var a) && a.GetBoolean()
    };

    private static TimesheetUserDto MapUser(JsonElement u)
    {
        var status = u.TryGetProperty("status", out var s) ? s.GetString() : "ACTIVE";
        return new TimesheetUserDto
        {
            ExternalId = u.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
            Name = u.TryGetProperty("name", out var n) ? n.GetString() ?? "Usuario sin nombre" : "Usuario sin nombre",
            Email = u.TryGetProperty("email", out var e) ? e.GetString() : null,
            Active = status == "ACTIVE"
        };
    }

    private static TimesheetProjectDto MapProject(JsonElement p)
    {
        var name = p.TryGetProperty("name", out var n) ? n.GetString() ?? "Proyecto sin nombre" : "Proyecto sin nombre";

        string? code = null;
        if (p.TryGetProperty("code", out var c) && c.ValueKind != JsonValueKind.Null)
            code = c.GetString();
        if (string.IsNullOrEmpty(code))
            code = ExtractCodeFromName(name);

        string? clientExternalId = null;
        if (p.TryGetProperty("clientId", out var cid) && cid.ValueKind != JsonValueKind.Null)
            clientExternalId = cid.GetString();

        return new TimesheetProjectDto
        {
            ExternalId = p.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
            Name = name,
            Code = code,
            Archived = p.TryGetProperty("archived", out var a) && a.GetBoolean(),
            ClientExternalId = clientExternalId
        };
    }

    private static TimesheetEntryDto MapEntry(JsonElement e)
    {
        var durationHours = 0.0m;
        DateTime? start = null, end = null;

        if (e.TryGetProperty("timeInterval", out var ti))
        {
            if (ti.TryGetProperty("duration", out var dur) && dur.ValueKind != JsonValueKind.Null)
                durationHours = ParseIsoDuration(dur.GetString() ?? string.Empty);
            if (ti.TryGetProperty("start", out var s) && s.ValueKind != JsonValueKind.Null)
                start = DateTime.Parse(s.GetString()!).ToUniversalTime();
            if (ti.TryGetProperty("end", out var en) && en.ValueKind != JsonValueKind.Null)
                end = DateTime.Parse(en.GetString()!).ToUniversalTime();
        }

        return new TimesheetEntryDto
        {
            ExternalId = e.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
            ProjectExternalId = e.TryGetProperty("projectId", out var pid) ? pid.GetString() : null,
            UserExternalId = e.TryGetProperty("userId", out var uid) ? uid.GetString() : null,
            Description = e.TryGetProperty("description", out var desc) ? desc.GetString() : null,
            Start = start,
            End = end,
            DurationHours = durationHours,
            // Decisión por-campo: si el proveedor no expone "billable", default = false.
            Billable = e.TryGetProperty("billable", out var bill) && bill.GetBoolean(),
            RawPayload = e.GetRawText()
        };
    }

    private static string ExtractCodeFromName(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        var dashPos = name.IndexOf('-');
        if (dashPos < 0) return string.Empty;
        return name[..dashPos].Trim();
    }

    private static decimal ParseIsoDuration(string iso)
    {
        try
        {
            var ts = System.Xml.XmlConvert.ToTimeSpan(iso);
            return (decimal)ts.TotalHours;
        }
        catch { return 0; }
    }
}
