using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;
using Microsoft.EntityFrameworkCore;

namespace bdt_evm_app.Services;

// Resultados que la capa de orquestación devuelve a los controllers. Los controllers
// solo los mapean a la respuesta HTTP (preservando el shape del contrato existente).

public record ProjectTimeEntriesSyncResult(
    ulong ProjectId,
    int TotalInDbBefore,
    int TotalInDbAfter,
    int Added,
    int Updated,
    int Skipped,
    int Deleted,
    string Mode,
    string? StartIso,
    string? EndIso,
    string ClockifyProjectId);

public record ProjectSyncStatus(
    ulong ProjectId,
    string? ClockifyProjectId,
    int TimeEntriesInDb,
    int? TimeEntriesInClockify,
    bool NeedsSync,
    int MissingCount,
    string? Error);

public record EntriesEstimate(
    int EstimatedCount,
    int EstimatedTimeSeconds,
    string EstimatedTimeFormatted,
    int UsersCount);

/// <summary>
/// Orquestación de la sincronización de time tracking. Toda la lógica de negocio y de
/// acceso a la base (upsert, dedup, borrado de huérfanos, modos) vive acá; no conoce el
/// formato del proveedor, solo consume <see cref="ITimesheetProvider"/> (DTOs neutros).
/// </summary>
public class TimesheetSyncService
{
    private readonly AppDbContext _db;
    private readonly ITimesheetProvider _provider;
    private readonly ILogger<TimesheetSyncService> _logger;

    public TimesheetSyncService(AppDbContext db, ITimesheetProvider provider, ILogger<TimesheetSyncService> logger)
    {
        _db = db;
        _provider = provider;
        _logger = logger;
    }

    public async Task<int> SyncClientsAsync()
    {
        var clients = await _provider.GetClientsAsync();
        foreach (var c in clients)
        {
            if (string.IsNullOrEmpty(c.ExternalId)) continue;

            var existing = await _db.TimesheetClients.FirstOrDefaultAsync(x => x.ExternalId == c.ExternalId);
            if (existing != null)
            {
                existing.Name = c.Name;
                existing.Status = c.Archived ? "inactivo" : "activo";
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.TimesheetClients.Add(new TimesheetClient
                {
                    ExternalId = c.ExternalId,
                    Name = c.Name,
                    Status = c.Archived ? "inactivo" : "activo",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }
        await _db.SaveChangesAsync();
        return clients.Count;
    }

    public async Task<int> SyncUsersAsync(bool onlyActive)
    {
        var users = await _provider.GetUsersAsync(onlyActive);
        var count = 0;
        foreach (var u in users)
        {
            if (string.IsNullOrEmpty(u.ExternalId)) continue;

            var existing = await _db.TimesheetUsers.FirstOrDefaultAsync(x => x.ClockifyUserId == u.ExternalId);
            if (existing != null)
            {
                existing.Name = u.Name;
                existing.Email = u.Email;
                existing.Active = u.Active;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.TimesheetUsers.Add(new TimesheetUser
                {
                    ClockifyUserId = u.ExternalId,
                    Name = u.Name,
                    Email = u.Email,
                    Active = u.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            count++;
        }
        await _db.SaveChangesAsync();
        return count;
    }

    public async Task<int> SyncProjectsAsync()
    {
        var projects = await _provider.GetProjectsAsync();
        foreach (var p in projects)
        {
            if (string.IsNullOrEmpty(p.ExternalId)) continue;

            ulong? clientId = null;
            if (!string.IsNullOrEmpty(p.ClientExternalId))
            {
                var client = await _db.TimesheetClients.FirstOrDefaultAsync(c => c.ExternalId == p.ClientExternalId);
                if (client != null) clientId = client.Id;
            }

            var status = p.Archived ? "cerrado" : "activo";
            var existing = await _db.TimesheetProjects.FirstOrDefaultAsync(x => x.ClockifyProjectId == p.ExternalId);
            if (existing != null)
            {
                existing.Name = p.Name;
                existing.Code = p.Code;
                existing.Status = status;
                existing.ClientId = clientId;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.TimesheetProjects.Add(new TimesheetProject
                {
                    ClockifyProjectId = p.ExternalId,
                    Name = p.Name,
                    Code = p.Code,
                    Status = status,
                    ClientId = clientId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }
        await _db.SaveChangesAsync();
        return projects.Count;
    }

    public async Task<int> SyncUserTimeEntriesAsync(string userExternalId, string? start, string? end)
    {
        var page = 1;
        var total = 0;
        IReadOnlyList<TimesheetEntryDto> entries;
        do
        {
            entries = await _provider.GetEntriesForUserAsync(userExternalId, start, end, page, 100);
            foreach (var e in entries)
                if (await UpsertEntryAsync(e)) total++;
            page++;
        } while (entries.Count == 100);

        await _db.SaveChangesAsync();
        return total;
    }

    public async Task<int> SyncAllTimeEntriesAsync(string? start, string? end)
    {
        var total = 0;
        await foreach (var e in _provider.GetAllEntriesAsync(start, end))
            if (await UpsertEntryAsync(e)) total++;

        await _db.SaveChangesAsync();
        return total;
    }

    public async Task<int> SyncProjectTimeEntriesByExternalIdAsync(string projectExternalId, string? start, string? end)
    {
        var page = 1;
        var total = 0;
        IReadOnlyList<TimesheetEntryDto> entries;
        do
        {
            entries = await _provider.GetEntriesForProjectAsync(projectExternalId, start, end, page, 100);
            foreach (var e in entries)
                if (await UpsertEntryAsync(e)) total++;
            page++;
        } while (entries.Count == 100);

        await _db.SaveChangesAsync();
        return total;
    }

    /// <summary>Sincronización por proyecto local con modos all / missing / from_date.
    /// Asume <paramref name="project"/> ya validado (existe y tiene ClockifyProjectId).</summary>
    public async Task<ProjectTimeEntriesSyncResult> SyncProjectTimeEntriesAsync(TimesheetProject project, string mode, string? from)
    {
        var totalInDbBefore = await _db.TimesheetTimeEntries.CountAsync(t => t.ProjectId == project.Id);
        var clockifyProjectId = project.ClockifyProjectId.Trim();

        string? startIso = null, endIso = null;

        if (mode == "missing")
        {
            var oldest = await _db.TimesheetTimeEntries
                .Where(t => t.ProjectId == project.Id)
                .OrderBy(t => t.StartTime)
                .FirstOrDefaultAsync();

            var twoYearsAgo = DateTime.UtcNow.AddYears(-2);
            var startDate = oldest != null
                ? new[] { oldest.StartTime.AddMonths(-1), twoYearsAgo }.Min()
                : twoYearsAgo;

            startIso = startDate.ToString("yyyy-MM-ddT00:00:00Z");
            endIso = DateTime.UtcNow.ToString("yyyy-MM-ddT23:59:59Z");
        }
        else if (mode == "from_date" && !string.IsNullOrEmpty(from))
        {
            startIso = $"{from}T00:00:00Z";
        }

        var existingIds = mode == "missing"
            ? await _db.TimesheetTimeEntries
                .Where(t => t.ProjectId == project.Id)
                .Select(t => t.ClockifyTimeEntryId)
                .ToListAsync()
            : new List<string>();

        var added = 0;
        var skipped = 0;
        var updated = 0;
        var page = 1;
        var allExternalIds = new List<string>();
        IReadOnlyList<TimesheetEntryDto> entries;

        do
        {
            entries = await _provider.GetEntriesForProjectAsync(clockifyProjectId, startIso, endIso, page, 100);

            foreach (var e in entries)
            {
                if (!string.IsNullOrEmpty(e.ExternalId))
                    allExternalIds.Add(e.ExternalId);

                if (mode == "missing" && existingIds.Contains(e.ExternalId))
                {
                    skipped++;
                    continue;
                }

                var existsBefore = await _db.TimesheetTimeEntries
                    .AnyAsync(t => t.ClockifyTimeEntryId == e.ExternalId);

                if (await UpsertEntryAsync(e))
                {
                    if (existsBefore) updated++;
                    else added++;
                }
                else skipped++;
            }

            page++;
        } while (entries.Count == 100);

        var deleted = 0;
        if (mode == "all" && allExternalIds.Any())
        {
            deleted = await _db.TimesheetTimeEntries
                .Where(t => t.ProjectId == project.Id &&
                            !allExternalIds.Contains(t.ClockifyTimeEntryId))
                .ExecuteDeleteAsync();
        }

        await _db.SaveChangesAsync();
        var totalInDbAfter = await _db.TimesheetTimeEntries.CountAsync(t => t.ProjectId == project.Id);

        return new ProjectTimeEntriesSyncResult(
            project.Id, totalInDbBefore, totalInDbAfter,
            added, updated, skipped, deleted,
            mode, startIso, endIso, project.ClockifyProjectId);
    }

    /// <summary>Devuelve null si el proyecto no existe.</summary>
    public async Task<ProjectSyncStatus?> GetProjectSyncStatusAsync(ulong id)
    {
        var project = await _db.TimesheetProjects.FindAsync(id);
        if (project == null) return null;

        if (string.IsNullOrEmpty(project.ClockifyProjectId))
            return new ProjectSyncStatus(project.Id, null, 0, null, false, 0,
                "El proyecto no tiene clockify_project_id configurado");

        var timeEntriesInDb = await _db.TimesheetTimeEntries.CountAsync(t => t.ProjectId == project.Id);

        int? timeEntriesInClockify = null;
        try
        {
            var firstPage = await _provider.GetEntriesForProjectAsync(project.ClockifyProjectId.Trim(), null, null, 1, 100);
            timeEntriesInClockify = firstPage.Count == 100 ? 100 : firstPage.Count;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo obtener time entries desde Clockify para proyecto {Id}", id);
        }

        var needsSync = timeEntriesInClockify.HasValue && timeEntriesInClockify > timeEntriesInDb;
        var missingCount = needsSync ? Math.Max(0, timeEntriesInClockify!.Value - timeEntriesInDb) : 0;

        return new ProjectSyncStatus(project.Id, project.ClockifyProjectId, timeEntriesInDb,
            timeEntriesInClockify, needsSync, missingCount, null);
    }

    public async Task<EntriesEstimate> EstimateEntriesAsync(string? start, string? end)
    {
        var users = await _provider.GetUsersAsync();
        var usersCount = users.Count;

        var sampleSize = Math.Min(3, usersCount);
        var sampleCount = 0;
        var sampleUsers = 0;

        foreach (var user in users.Take(sampleSize))
        {
            if (string.IsNullOrEmpty(user.ExternalId)) continue;
            var entries = await _provider.GetEntriesForUserAsync(user.ExternalId, start, end, 1, 100);
            sampleCount += entries.Count;
            sampleUsers++;
        }

        var avgPerUser = sampleUsers > 0 ? (double)sampleCount / sampleUsers : 0;
        var estimatedCount = (int)Math.Ceiling(avgPerUser * usersCount * 1.2);
        var estimatedTimeSeconds = (int)Math.Ceiling((estimatedCount * 0.05) + (usersCount * 0.5));

        return new EntriesEstimate(estimatedCount, estimatedTimeSeconds, FormatEstimatedTime(estimatedTimeSeconds), usersCount);
    }

    // ---- Upsert de un time entry neutro en la base. Mantiene la semántica de skip
    // del antiguo ProcessTimeEntry: descarta (return false) si falta id, proyecto local,
    // o start; cuenta como procesado en caso contrario. ----
    private async Task<bool> UpsertEntryAsync(TimesheetEntryDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.ExternalId)) return false;
            if (string.IsNullOrEmpty(dto.ProjectExternalId)) return false;

            var project = await _db.TimesheetProjects
                .FirstOrDefaultAsync(p => p.ClockifyProjectId == dto.ProjectExternalId);
            if (project == null) return false;

            TimesheetUser? user = null;
            if (!string.IsNullOrEmpty(dto.UserExternalId))
                user = await _db.TimesheetUsers.FirstOrDefaultAsync(u => u.ClockifyUserId == dto.UserExternalId);

            if (dto.Start == null) return false;
            var startTime = dto.Start.Value;
            var endTime = dto.End ?? startTime;

            var description = dto.Description?[..Math.Min(dto.Description.Length, 255)];

            var existing = await _db.TimesheetTimeEntries
                .FirstOrDefaultAsync(t => t.ClockifyTimeEntryId == dto.ExternalId);

            if (existing != null)
            {
                existing.ProjectId = project.Id;
                existing.UserId = user?.Id;
                existing.Description = description;
                existing.StartTime = startTime;
                existing.EndTime = endTime;
                existing.DurationHours = dto.DurationHours;
                existing.Billable = dto.Billable;
                existing.SourceRaw = dto.RawPayload;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.TimesheetTimeEntries.Add(new TimesheetTimeEntry
                {
                    ClockifyTimeEntryId = dto.ExternalId,
                    ProjectId = project.Id,
                    UserId = user?.Id,
                    Description = description,
                    StartTime = startTime,
                    EndTime = endTime,
                    DurationHours = dto.DurationHours,
                    Billable = dto.Billable,
                    SourceRaw = dto.RawPayload,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error procesando time entry");
            return false;
        }
    }

    private static string FormatEstimatedTime(int seconds)
    {
        if (seconds < 60) return $"~{seconds} segundo{(seconds != 1 ? "s" : "")}";
        if (seconds < 3600)
        {
            var minutes = (int)Math.Ceiling((double)seconds / 60);
            return $"~{minutes} minuto{(minutes != 1 ? "s" : "")}";
        }
        var hours = (int)Math.Ceiling((double)seconds / 3600);
        return $"~{hours} hora{(hours != 1 ? "s" : "")}";
    }
}
