using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Services;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/timesheet")]
[RequirePermission("ADMIN_ACCESS")]
public class TimesheetSyncController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITimesheetProvider _provider;
    private readonly TimesheetSyncService _sync;
    private readonly ILogger<TimesheetSyncController> _logger;

    public TimesheetSyncController(
        AppDbContext db,
        ITimesheetProvider provider,
        TimesheetSyncService sync,
        ILogger<TimesheetSyncController> logger)
    {
        _db = db;
        _provider = provider;
        _sync = sync;
        _logger = logger;
    }

    // POST api/timesheet/sync-clients
    [HttpPost("sync-clients")]
    public async Task<IActionResult> SyncClients()
    {
        try
        {
            var count = await _sync.SyncClientsAsync();
            return Ok(new { message = "Clientes sincronizados desde Clockify", count });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al sincronizar clientes");
            return StatusCode(500, new { error = "Error al sincronizar clientes", message = e.Message });
        }
    }

    // GET api/timesheet/sync-clients
    [HttpGet("sync-clients")]
    public async Task<IActionResult> GetClients()
    {
        try
        {
            var clients = await _provider.GetClientsAsync();
            return Ok(new { data = clients, count = clients.Count });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al consultar clientes", message = e.Message });
        }
    }

    // POST api/timesheet/sync-users
    [HttpPost("sync-users")]
    public async Task<IActionResult> SyncUsers([FromQuery] bool only_active = false)
    {
        try
        {
            var count = await _sync.SyncUsersAsync(only_active);
            return Ok(new { message = "Usuarios sincronizados desde Clockify", count, only_active });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al sincronizar usuarios");
            return StatusCode(500, new { error = "Error al sincronizar usuarios", message = e.Message });
        }
    }

    // GET api/timesheet/sync-users
    [HttpGet("sync-users")]
    public async Task<IActionResult> GetUsers([FromQuery] bool only_active = false)
    {
        try
        {
            var users = await _provider.GetUsersAsync(only_active);
            return Ok(new { data = users, count = users.Count, only_active });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al consultar usuarios", message = e.Message });
        }
    }

    // POST api/timesheet/sync-time-entries
    [HttpPost("sync-time-entries")]
    public async Task<IActionResult> SyncTimeEntries([FromQuery] string? from, [FromQuery] string? to)
    {
        try
        {
            var userId = _provider.DefaultUserExternalId;
            if (string.IsNullOrEmpty(userId))
                return StatusCode(500, new { error = "CLOCKIFY_USER_ID no configurado" });

            var start = from != null ? $"{from}T00:00:00Z" : null;
            var end = to != null ? $"{to}T23:59:59Z" : null;

            var total = await _sync.SyncUserTimeEntriesAsync(userId, start, end);
            return Ok(new { message = "Time entries sincronizados desde Clockify", total });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al sincronizar time entries");
            return StatusCode(500, new { error = "Error al sincronizar time entries", message = e.Message });
        }
    }

    // POST api/timesheet/sync-time-entries-all
    [HttpPost("sync-time-entries-all")]
    public async Task<IActionResult> SyncTimeEntriesAll([FromQuery] string? from, [FromQuery] string? to)
    {
        try
        {
            var start = from != null ? $"{from}T00:00:00Z" : null;
            var end = to != null ? $"{to}T23:59:59Z" : null;

            var total = await _sync.SyncAllTimeEntriesAsync(start, end);
            return Ok(new { message = "Time entries sincronizados desde Clockify (todos los usuarios)", total });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al sincronizar todas las time entries");
            return StatusCode(500, new { error = "Error al sincronizar time entries", message = e.Message });
        }
    }

    // POST api/timesheet/sync-time-entries-by-project
    [HttpPost("sync-time-entries-by-project")]
    public async Task<IActionResult> SyncTimeEntriesByProject([FromQuery] string project_id, [FromQuery] string? from, [FromQuery] string? to)
    {
        if (string.IsNullOrEmpty(project_id))
            return UnprocessableEntity(new { error = "project_id es requerido" });

        try
        {
            var start = from != null ? $"{from}T00:00:00Z" : null;
            var end = to != null ? $"{to}T23:59:59Z" : null;

            var total = await _sync.SyncProjectTimeEntriesByExternalIdAsync(project_id, start, end);
            return Ok(new { message = "Time entries sincronizados desde Clockify (por proyecto)", total, project_id });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al sincronizar time entries por proyecto");
            return StatusCode(500, new { error = "Error al sincronizar time entries", message = e.Message });
        }
    }

    // POST api/timesheet/sync-projects
    [HttpPost("sync-projects")]
    public async Task<IActionResult> SyncProjects()
    {
        try
        {
            var count = await _sync.SyncProjectsAsync();
            return Ok(new { message = "Proyectos sincronizados desde Clockify", count });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al sincronizar proyectos");
            return StatusCode(500, new { error = "Error al sincronizar proyectos", message = e.Message });
        }
    }

    // GET api/timesheet/sync-time-entries
    [HttpGet("sync-time-entries")]
    public async Task<IActionResult> GetTimeEntries(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] int page = 1,
        [FromQuery] int page_size = 100)
    {
        try
        {
            var userId = _provider.DefaultUserExternalId;
            if (string.IsNullOrEmpty(userId))
                return StatusCode(500, new { error = "CLOCKIFY_USER_ID no configurado" });

            var start = from != null ? $"{from}T00:00:00Z" : null;
            var end = to != null ? $"{to}T23:59:59Z" : null;

            var entries = await _provider.GetEntriesForUserAsync(userId, start, end, page, page_size);
            return Ok(new { data = entries, page, page_size, count = entries.Count });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al consultar time entries", message = e.Message });
        }
    }

    // GET api/timesheet/sync-time-entries-all
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

            var allEntries = new List<TimesheetEntryDto>();
            var maxEntries = page * page_size;

            await foreach (var entry in _provider.GetAllEntriesAsync(start, end))
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

    // GET api/timesheet/sync-time-entries-by-project
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

            var entries = await _provider.GetEntriesForProjectAsync(project_id, start, end, page, page_size);
            return Ok(new { data = entries, page, page_size, count = entries.Count, project_id });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al consultar time entries", message = e.Message });
        }
    }

    // GET api/timesheet/estimate-time-entries
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

            var est = await _sync.EstimateEntriesAsync(start, end);

            return Ok(new
            {
                estimated_count = est.EstimatedCount,
                estimated_time_seconds = est.EstimatedTimeSeconds,
                estimated_time_formatted = est.EstimatedTimeFormatted,
                users_count = est.UsersCount
            });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al estimar time entries", message = e.Message });
        }
    }

    // GET api/timesheet/projects/{id}/sync-status
    [HttpGet("projects/{id}/sync-status")]
    public async Task<IActionResult> GetSyncStatus(ulong id)
    {
        try
        {
            var status = await _sync.GetProjectSyncStatusAsync(id);
            if (status == null)
                return NotFound(new { message = "Proyecto no encontrado" });

            if (status.Error != null)
                return Ok(new
                {
                    project_id = status.ProjectId,
                    timesheet_project_id = status.TimesheetProjectId,
                    time_entries_in_db = status.TimeEntriesInDb,
                    timesheet_time_entries = status.TimeEntriesInTimesheet,
                    needs_sync = status.NeedsSync,
                    missing_count = status.MissingCount,
                    error = status.Error
                });

            return Ok(new
            {
                project_id = status.ProjectId,
                timesheet_project_id = status.TimesheetProjectId,
                time_entries_in_db = status.TimeEntriesInDb,
                timesheet_time_entries = status.TimeEntriesInTimesheet,
                needs_sync = status.NeedsSync,
                missing_count = status.MissingCount
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener estado de sincronización del proyecto {Id}", id);
            return StatusCode(500, new { error = "Error al obtener estado de sincronización", message = e.Message });
        }
    }

    // POST api/timesheet/projects/{id}/sync-time-entries
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

        var project = await _db.TimesheetProjects.FindAsync(id);
        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        if (string.IsNullOrEmpty(project.TimesheetProjectId?.Trim()))
            return BadRequest(new { error = "El proyecto no tiene timesheet_project_id configurado" });

        try
        {
            var r = await _sync.SyncProjectTimeEntriesAsync(project, mode, from);

            return Ok(new
            {
                message = "Sincronización completada",
                project_id = r.ProjectId,
                total_in_db_before = r.TotalInDbBefore,
                total_in_db_after = r.TotalInDbAfter,
                added = r.Added,
                updated = r.Updated,
                skipped = r.Skipped,
                deleted = r.Deleted,
                params_used = new { mode = r.Mode, startIso = r.StartIso, endIso = r.EndIso, timesheet_project_id = r.TimesheetProjectId }
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al sincronizar time entries del proyecto {Id}", id);
            return StatusCode(500, new { error = "Error al sincronizar time entries", message = e.Message });
        }
    }

    // GET api/timesheet/projects/{id}/hours-summary
    [HttpGet("projects/{id}/hours-summary")]
    public async Task<IActionResult> GetProjectHoursSummary(ulong id)
    {
        try
        {
            var project = await _db.TimesheetProjects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                return NotFound(new { message = "Proyecto no encontrado" });

            var entries = await _db.TimesheetTimeEntries
                .Where(t => t.ProjectId == id)
                .ToListAsync();

            var users = await _db.TimesheetUsers.ToListAsync();

            var months = entries
                .Select(e => e.StartTime.ToString("yyyy-MM"))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var result = entries
                .GroupBy(e => e.UserId)
                .Select(g =>
                {
                    var user = users.FirstOrDefault(u => u.Id == g.Key);
                    var monthData = g
                        .GroupBy(x => x.StartTime.ToString("yyyy-MM"))
                        .ToDictionary(x => x.Key, x => Math.Round(x.Sum(v => v.DurationHours), 2));

                    return new
                    {
                        user_id = g.Key,
                        user_name = user?.Name ?? "Usuario sin identificar",
                        total_hours = Math.Round(g.Sum(x => x.DurationHours), 2),
                        months = monthData
                    };
                })
                .OrderBy(x => x.user_name)
                .ToList();

            return Ok(new
            {
                project_id = project.Id,
                project_name = project.Name,
                total_entries = entries.Count,
                months,
                data = result
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error obteniendo resumen de horas del proyecto {Id}", id);
            return StatusCode(500, new { error = "Error obteniendo horas del proyecto", message = e.Message });
        }
    }
}
