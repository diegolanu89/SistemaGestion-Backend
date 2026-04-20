using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using bdt_evm_app.Data;
using bdt_evm_app.Models;
using bdt_evm_app.Services;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/clockify")]
public class ClockifySyncController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ClockifyService _clockify;
    private readonly ILogger<ClockifySyncController> _logger;

    public ClockifySyncController(AppDbContext db, ClockifyService clockify, ILogger<ClockifySyncController> logger)
    {
        _db = db;
        _clockify = clockify;
        _logger = logger;
    }

    // POST api/clockify/sync-clients
    [HttpPost("sync-clients")]
    public async Task<IActionResult> SyncClients()
    {
        try
        {
            var clients = await _clockify.GetClients();
            foreach (var c in clients)
            {
                if (!c.TryGetProperty("id", out var idProp)) continue;
                var externalId = idProp.GetString() ?? string.Empty;
                var name = c.TryGetProperty("name", out var n) ? n.GetString() ?? "Cliente sin nombre" : "Cliente sin nombre";
                var archived = c.TryGetProperty("archived", out var a) && a.GetBoolean();

                var existing = await _db.ClockifyClients.FirstOrDefaultAsync(x => x.ExternalId == externalId);
                if (existing != null)
                {
                    existing.Name = name;
                    existing.Status = archived ? "inactivo" : "activo";
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _db.ClockifyClients.Add(new ClockifyClient
                    {
                        ExternalId = externalId,
                        Name = name,
                        Status = archived ? "inactivo" : "activo",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
            await _db.SaveChangesAsync();
            return Ok(new { message = "Clientes sincronizados desde Clockify", count = clients.Count });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al sincronizar clientes");
            return StatusCode(500, new { error = "Error al sincronizar clientes", message = e.Message });
        }
    }

    // GET api/clockify/sync-clients
    [HttpGet("sync-clients")]
    public async Task<IActionResult> GetClients()
    {
        try
        {
            var clients = await _clockify.GetClients();
            return Ok(new { data = clients, count = clients.Count });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al consultar clientes", message = e.Message });
        }
    }

    // POST api/clockify/sync-users
    [HttpPost("sync-users")]
    public async Task<IActionResult> SyncUsers([FromQuery] bool only_active = false)
    {
        try
        {
            var users = await _clockify.GetUsers(only_active);
            var count = 0;
            foreach (var u in users)
            {
                if (!u.TryGetProperty("id", out var idProp)) continue;
                var clockifyUserId = idProp.GetString() ?? string.Empty;
                var name = u.TryGetProperty("name", out var n) ? n.GetString() ?? "Usuario sin nombre" : "Usuario sin nombre";
                var email = u.TryGetProperty("email", out var e) ? e.GetString() : null;
                var status = u.TryGetProperty("status", out var s) ? s.GetString() : "ACTIVE";
                var active = status == "ACTIVE";

                var existing = await _db.ClockifyUsers.FirstOrDefaultAsync(x => x.ClockifyUserId == clockifyUserId);
                if (existing != null)
                {
                    existing.Name = name;
                    existing.Email = email;
                    existing.Active = active;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _db.ClockifyUsers.Add(new ClockifyUser
                    {
                        ClockifyUserId = clockifyUserId,
                        Name = name,
                        Email = email,
                        Active = active,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                count++;
            }
            await _db.SaveChangesAsync();
            return Ok(new { message = "Usuarios sincronizados desde Clockify", count, only_active });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al sincronizar usuarios");
            return StatusCode(500, new { error = "Error al sincronizar usuarios", message = e.Message });
        }
    }

    // GET api/clockify/sync-users
    [HttpGet("sync-users")]
    public async Task<IActionResult> GetUsers([FromQuery] bool only_active = false)
    {
        try
        {
            var users = await _clockify.GetUsers(only_active);
            return Ok(new { data = users, count = users.Count, only_active });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al consultar usuarios", message = e.Message });
        }
    }

    // POST api/clockify/sync-time-entries
    [HttpPost("sync-time-entries")]
    public async Task<IActionResult> SyncTimeEntries([FromQuery] string? from, [FromQuery] string? to)
    {
        try
        {
            var userId = _clockify.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return StatusCode(500, new { error = "CLOCKIFY_USER_ID no configurado" });

            var start = from != null ? $"{from}T00:00:00Z" : null;
            var end = to != null ? $"{to}T23:59:59Z" : null;

            var page = 1;
            var total = 0;
            List<JsonElement> entries;
            do
            {
                entries = await _clockify.GetTimeEntries(userId, start, end, page, 100);
                foreach (var e in entries)
                    if (await ProcessTimeEntry(e)) total++;
                page++;
            } while (entries.Count == 100);

            await _db.SaveChangesAsync();
            return Ok(new { message = "Time entries sincronizados desde Clockify", total });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al sincronizar time entries");
            return StatusCode(500, new { error = "Error al sincronizar time entries", message = e.Message });
        }
    }

    // POST api/clockify/sync-time-entries-all
    [HttpPost("sync-time-entries-all")]
    public async Task<IActionResult> SyncTimeEntriesAll([FromQuery] string? from, [FromQuery] string? to)
    {
        try
        {
            var start = from != null ? $"{from}T00:00:00Z" : null;
            var end = to != null ? $"{to}T23:59:59Z" : null;
            var total = 0;

            await foreach (var e in _clockify.GetAllUsersTimeEntries(start, end))
                if (await ProcessTimeEntry(e)) total++;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Time entries sincronizados desde Clockify (todos los usuarios)", total });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al sincronizar todas las time entries");
            return StatusCode(500, new { error = "Error al sincronizar time entries", message = e.Message });
        }
    }

    // POST api/clockify/sync-time-entries-by-project
    [HttpPost("sync-time-entries-by-project")]
    public async Task<IActionResult> SyncTimeEntriesByProject([FromQuery] string project_id, [FromQuery] string? from, [FromQuery] string? to)
    {
        if (string.IsNullOrEmpty(project_id))
            return UnprocessableEntity(new { error = "project_id es requerido" });

        try
        {
            var start = from != null ? $"{from}T00:00:00Z" : null;
            var end = to != null ? $"{to}T23:59:59Z" : null;
            var page = 1;
            var total = 0;
            List<JsonElement> entries;
            do
            {
                entries = await _clockify.GetTimeEntriesByProject(project_id, start, end, page, 100);
                foreach (var e in entries)
                    if (await ProcessTimeEntry(e)) total++;
                page++;
            } while (entries.Count == 100);

            await _db.SaveChangesAsync();
            return Ok(new { message = "Time entries sincronizados desde Clockify (por proyecto)", total, project_id });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al sincronizar time entries por proyecto");
            return StatusCode(500, new { error = "Error al sincronizar time entries", message = e.Message });
        }
    }

    private async Task<bool> ProcessTimeEntry(JsonElement e)
    {
        try
        {
            var clockifyId = e.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
            if (string.IsNullOrEmpty(clockifyId)) return false;

            var clockifyProjectId = e.TryGetProperty("projectId", out var pid) ? pid.GetString() : null;
            if (string.IsNullOrEmpty(clockifyProjectId)) return false;

            var project = await _db.ClockifyProjects
                .FirstOrDefaultAsync(p => p.ClockifyProjectId == clockifyProjectId);
            if (project == null) return false;

            var clockifyUserId = e.TryGetProperty("userId", out var uid) ? uid.GetString() : null;
            ClockifyUser? user = null;
            if (!string.IsNullOrEmpty(clockifyUserId))
                user = await _db.ClockifyUsers.FirstOrDefaultAsync(u => u.ClockifyUserId == clockifyUserId);

            var durationHours = 0.0m;
            if (e.TryGetProperty("timeInterval", out var ti) &&
                ti.TryGetProperty("duration", out var dur) &&
                dur.ValueKind != JsonValueKind.Null)
                durationHours = ParseIsoDuration(dur.GetString() ?? string.Empty);

            DateTime? startTime = null, endTime = null;
            if (e.TryGetProperty("timeInterval", out var ti2))
            {
                if (ti2.TryGetProperty("start", out var s) && s.ValueKind != JsonValueKind.Null)
                    startTime = DateTime.Parse(s.GetString()!).ToUniversalTime();
                if (ti2.TryGetProperty("end", out var en) && en.ValueKind != JsonValueKind.Null)
                    endTime = DateTime.Parse(en.GetString()!).ToUniversalTime();
            }

            if (startTime == null) return false;
            endTime ??= startTime;

            var description = e.TryGetProperty("description", out var desc) ? desc.GetString() : null;
            var billable = e.TryGetProperty("billable", out var bill) && bill.GetBoolean();
            var sourceRaw = e.GetRawText();

            var existing = await _db.ClockifyTimeEntries
                .FirstOrDefaultAsync(t => t.ClockifyTimeEntryId == clockifyId);

            if (existing != null)
            {
                existing.ProjectId = project.Id;
                existing.UserId = user?.Id;
                existing.Description = description?[..Math.Min(description.Length, 255)];
                existing.StartTime = startTime.Value;
                existing.EndTime = endTime.Value;
                existing.DurationHours = durationHours;
                existing.Billable = billable;
                existing.SourceRaw = sourceRaw;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.ClockifyTimeEntries.Add(new ClockifyTimeEntry
                {
                    ClockifyTimeEntryId = clockifyId,
                    ProjectId = project.Id,
                    UserId = user?.Id,
                    Description = description?[..Math.Min(description.Length, 255)],
                    StartTime = startTime.Value,
                    EndTime = endTime.Value,
                    DurationHours = durationHours,
                    Billable = billable,
                    SourceRaw = sourceRaw,
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

    // POST api/clockify/sync-projects
    [HttpPost("sync-projects")]
    public async Task<IActionResult> SyncProjects()
    {
        try
        {
            var projects = await _clockify.GetProjects();

            foreach (var p in projects)
            {
                if (!p.TryGetProperty("id", out var idProp)) continue;
                var clockifyProjectId = idProp.GetString() ?? string.Empty;
                var name = p.TryGetProperty("name", out var n) ? n.GetString() ?? "Proyecto sin nombre" : "Proyecto sin nombre";
                var archived = p.TryGetProperty("archived", out var a) && a.GetBoolean();
                var status = archived ? "cerrado" : "activo";

                // Extraer código del nombre
                string? code = null;
                if (p.TryGetProperty("code", out var c) && c.ValueKind != JsonValueKind.Null)
                    code = c.GetString();
                if (string.IsNullOrEmpty(code))
                    code = ExtractCodeFromName(name);

                // Buscar cliente local
                ulong? clientId = null;
                if (p.TryGetProperty("clientId", out var cid) && cid.ValueKind != JsonValueKind.Null)
                {
                    var externalClientId = cid.GetString();
                    var client = await _db.ClockifyClients
                        .FirstOrDefaultAsync(c => c.ExternalId == externalClientId);
                    if (client != null) clientId = client.Id;
                }

                var existing = await _db.ClockifyProjects
                    .FirstOrDefaultAsync(x => x.ClockifyProjectId == clockifyProjectId);

                if (existing != null)
                {
                    existing.Name = name;
                    existing.Code = code;
                    existing.Status = status;
                    existing.ClientId = clientId;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _db.ClockifyProjects.Add(new ClockifyProject
                    {
                        ClockifyProjectId = clockifyProjectId,
                        Name = name,
                        Code = code,
                        Status = status,
                        ClientId = clientId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Proyectos sincronizados desde Clockify", count = projects.Count });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al sincronizar proyectos");
            return StatusCode(500, new { error = "Error al sincronizar proyectos", message = e.Message });
        }
    }

    // GET api/clockify/sync-time-entries
    [HttpGet("sync-time-entries")]
    public async Task<IActionResult> GetTimeEntries(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] int page = 1,
        [FromQuery] int page_size = 100)
    {
        try
        {
            var userId = _clockify.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return StatusCode(500, new { error = "CLOCKIFY_USER_ID no configurado" });

            var start = from != null ? $"{from}T00:00:00Z" : null;
            var end = to != null ? $"{to}T23:59:59Z" : null;

            var entries = await _clockify.GetTimeEntries(userId, start, end, page, page_size);
            return Ok(new { data = entries, page, page_size, count = entries.Count });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al consultar time entries", message = e.Message });
        }
    }

    // GET api/clockify/sync-time-entries-all
    [HttpGet("sync-time-entries-all")]
    public async Task<IActionResult> GetTimeEntriesAll(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] int page = 1,
        [FromQuery] int page_size = 20)
    {
        try
        {
            if (page_size > 50) page_size = 50;

            if (string.IsNullOrEmpty(from) && string.IsNullOrEmpty(to))
            {
                to = DateTime.UtcNow.ToString("yyyy-MM-dd");
                from = DateTime.UtcNow.AddMonths(-3).ToString("yyyy-MM-dd");
            }

            var start = from != null ? $"{from}T00:00:00Z" : null;
            var end = to != null ? $"{to}T23:59:59Z" : null;

            var allEntries = new List<JsonElement>();
            var maxEntries = page * page_size;

            await foreach (var entry in _clockify.GetAllUsersTimeEntries(start, end))
            {
                allEntries.Add(entry);
                if (allEntries.Count >= maxEntries) break;
            }

            var offset = (page - 1) * page_size;
            var paginated = allEntries.Skip(offset).Take(page_size).ToList();

            return Ok(new
            {
                data = paginated,
                page,
                page_size,
                count = paginated.Count,
                total_fetched = allEntries.Count
            });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al consultar time entries", message = e.Message });
        }
    }

    // GET api/clockify/sync-time-entries-by-project
    [HttpGet("sync-time-entries-by-project")]
    public async Task<IActionResult> GetTimeEntriesByProject(
        [FromQuery] string project_id,
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] int page = 1,
        [FromQuery] int page_size = 10)
    {
        if (string.IsNullOrEmpty(project_id))
            return UnprocessableEntity(new { error = "project_id es requerido" });

        try
        {
            if (page_size > 20) page_size = 20;

            if (string.IsNullOrEmpty(from) && string.IsNullOrEmpty(to))
            {
                to = DateTime.UtcNow.ToString("yyyy-MM-dd");
                from = DateTime.UtcNow.AddMonths(-3).ToString("yyyy-MM-dd");
            }

            var start = from != null ? $"{from}T00:00:00Z" : null;
            var end = to != null ? $"{to}T23:59:59Z" : null;

            var entries = await _clockify.GetTimeEntriesByProject(project_id, start, end, page, page_size);
            return Ok(new { data = entries, page, page_size, count = entries.Count, project_id });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al consultar time entries", message = e.Message });
        }
    }

    // GET api/clockify/estimate-time-entries
    [HttpGet("estimate-time-entries")]
    public async Task<IActionResult> EstimateTimeEntries(
        [FromQuery] string? from,
        [FromQuery] string? to)
    {
        try
        {
            if (string.IsNullOrEmpty(from) && string.IsNullOrEmpty(to))
            {
                to = DateTime.UtcNow.ToString("yyyy-MM-dd");
                from = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
            }

            var start = from != null ? $"{from}T00:00:00Z" : null;
            var end = to != null ? $"{to}T23:59:59Z" : null;

            var users = await _clockify.GetUsers();
            var usersCount = users.Count;

            var sampleSize = Math.Min(3, usersCount);
            var sampleCount = 0;
            var sampleUsers = 0;

            foreach (var user in users.Take(sampleSize))
            {
                if (!user.TryGetProperty("id", out var idProp)) continue;
                var userId = idProp.GetString() ?? string.Empty;
                var entries = await _clockify.GetTimeEntries(userId, start, end, 1, 100);
                sampleCount += entries.Count;
                sampleUsers++;
            }

            var avgPerUser = sampleUsers > 0 ? (double)sampleCount / sampleUsers : 0;
            var estimatedCount = (int)Math.Ceiling(avgPerUser * usersCount * 1.2);
            var estimatedTimeSeconds = (int)Math.Ceiling((estimatedCount * 0.05) + (usersCount * 0.5));

            return Ok(new
            {
                estimated_count = estimatedCount,
                estimated_time_seconds = estimatedTimeSeconds,
                estimated_time_formatted = FormatEstimatedTime(estimatedTimeSeconds),
                users_count = usersCount
            });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al estimar time entries", message = e.Message });
        }
    }

    // GET api/clockify/projects/{id}/sync-status
    // TODO: Revisar con el cliente el cambio de ruta original /api/projects/{id}/sync-status
    // a /api/clockify/projects/{id}/sync-status para mantener consistencia con el resto
    // de endpoints de Clockify. Requiere actualizar el frontend React.
    [HttpGet("projects/{id}/sync-status")]
    public async Task<IActionResult> GetSyncStatus(ulong id)
    {
        try
        {
            var project = await _db.ClockifyProjects.FindAsync(id);
            if (project == null)
                return NotFound(new { message = "Proyecto no encontrado" });

            if (string.IsNullOrEmpty(project.ClockifyProjectId))
                return Ok(new
                {
                    project_id = project.Id,
                    clockify_project_id = (string?)null,
                    time_entries_in_db = 0,
                    time_entries_in_clockify = (int?)null,
                    needs_sync = false,
                    missing_count = 0,
                    error = "El proyecto no tiene clockify_project_id configurado"
                });

            var timeEntriesInDb = await _db.ClockifyTimeEntries
                .CountAsync(t => t.ProjectId == project.Id);

            int? timeEntriesInClockify = null;
            try
            {
                var firstPage = await _clockify.GetTimeEntriesByProject(
                    project.ClockifyProjectId.Trim(),
                    null, null, 1, 100
                );
                timeEntriesInClockify = firstPage.Count == 100 ? 100 : firstPage.Count;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener time entries desde Clockify para proyecto {Id}", id);
            }

            var needsSync = timeEntriesInClockify.HasValue && timeEntriesInClockify > timeEntriesInDb;
            var missingCount = needsSync ? Math.Max(0, timeEntriesInClockify!.Value - timeEntriesInDb) : 0;

            return Ok(new
            {
                project_id = project.Id,
                clockify_project_id = project.ClockifyProjectId,
                time_entries_in_db = timeEntriesInDb,
                time_entries_in_clockify = timeEntriesInClockify,
                needs_sync = needsSync,
                missing_count = missingCount
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener estado de sincronización del proyecto {Id}", id);
            return StatusCode(500, new { error = "Error al obtener estado de sincronización", message = e.Message });
        }
    }

    // POST api/clockify/projects/{id}/sync-time-entries
    // TODO: Revisar con el cliente el cambio de ruta original /api/projects/{id}/sync-time-entries
    // a /api/clockify/projects/{id}/sync-time-entries para mantener consistencia con el resto
    // de endpoints de Clockify. Requiere actualizar el frontend React.
    [HttpPost("projects/{id}/sync-time-entries")]
    public async Task<IActionResult> SyncProjectTimeEntries(
        ulong id,
        [FromQuery] string mode = "all",
        [FromQuery] string? from = null)
    {
        if (!new[] { "all", "missing", "from_date" }.Contains(mode))
            return BadRequest(new { error = "Modo inválido. Debe ser: all, missing, o from_date" });

        if (mode == "from_date" && string.IsNullOrEmpty(from))
            return BadRequest(new { error = "El parámetro 'from' es requerido cuando mode='from_date'" });

        var project = await _db.ClockifyProjects.FindAsync(id);
        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        if (string.IsNullOrEmpty(project.ClockifyProjectId?.Trim()))
            return BadRequest(new { error = "El proyecto no tiene clockify_project_id configurado" });

        try
        {
            var totalInDbBefore = await _db.ClockifyTimeEntries
                .CountAsync(t => t.ProjectId == project.Id);

            string? startIso = null, endIso = null;

            if (mode == "missing")
            {
                var oldest = await _db.ClockifyTimeEntries
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
                ? await _db.ClockifyTimeEntries
                    .Where(t => t.ProjectId == project.Id)
                    .Select(t => t.ClockifyTimeEntryId)
                    .ToListAsync()
                : new List<string>();

            var added = 0;
            var skipped = 0;
            var updated = 0;
            var page = 1;
            var allClockifyIds = new List<string>();
            List<System.Text.Json.JsonElement> entries;

            do
            {
                entries = await _clockify.GetTimeEntriesByProject(
                    project.ClockifyProjectId.Trim(),
                    startIso, endIso, page, 100
                );

                foreach (var e in entries)
                {
                    if (!e.TryGetProperty("id", out var eid)) continue;
                    var clockifyId = eid.GetString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(clockifyId))
                        allClockifyIds.Add(clockifyId);

                    if (mode == "missing" && existingIds.Contains(clockifyId))
                    {
                        skipped++;
                        continue;
                    }

                    var existsBefore = await _db.ClockifyTimeEntries
                        .AnyAsync(t => t.ClockifyTimeEntryId == clockifyId);

                    if (await ProcessTimeEntry(e))
                    {
                        if (existsBefore) updated++;
                        else added++;
                    }
                    else skipped++;
                }

                page++;
            } while (entries.Count == 100);

            var deleted = 0;
            if (mode == "all" && allClockifyIds.Any())
            {
                deleted = await _db.ClockifyTimeEntries
                    .Where(t => t.ProjectId == project.Id &&
                                !allClockifyIds.Contains(t.ClockifyTimeEntryId))
                    .ExecuteDeleteAsync();
            }

            await _db.SaveChangesAsync();
            var totalInDbAfter = await _db.ClockifyTimeEntries
                .CountAsync(t => t.ProjectId == project.Id);

            return Ok(new
            {
                message = "Sincronización completada",
                project_id = project.Id,
                total_in_db_before = totalInDbBefore,
                total_in_db_after = totalInDbAfter,
                added,
                updated,
                skipped,
                deleted,
                params_used = new { mode, startIso, endIso, clockify_project_id = project.ClockifyProjectId }
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al sincronizar time entries del proyecto {Id}", id);
            return StatusCode(500, new { error = "Error al sincronizar time entries", message = e.Message });
        }
    }

    private string ExtractCodeFromName(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        var dashPos = name.IndexOf('-');
        if (dashPos < 0) return string.Empty;
        return name[..dashPos].Trim();
    }

    private string FormatEstimatedTime(int seconds)
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

    private decimal ParseIsoDuration(string iso)
    {
        try
        {
            var ts = System.Xml.XmlConvert.ToTimeSpan(iso);
            return (decimal)ts.TotalHours;
        }
        catch { return 0; }
    }
}