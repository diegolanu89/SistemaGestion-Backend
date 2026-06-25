using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;
using bdt_evm_app.Services;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProjectBacService _bacService;
    private readonly EtcService _etcService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(AppDbContext db, ProjectBacService bacService, EtcService etcService, ILogger<ProjectsController> logger)
    {
        _db = db;
        _bacService = bacService;
        _etcService = etcService;
        _logger = logger;
    }

    // GET api/projects
    [HttpGet]
    [RequirePermission("PROJECTS_ACCESS")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int per_page = 15,
        [FromQuery] string only_visible = "true",
        [FromQuery] string? search = null,
        [FromQuery] string? client = null,
        [FromQuery] string? status = null,
        [FromQuery] string? code = null)
    {
        try
        {
            var onlyVisible = only_visible.ToLower() != "false" && only_visible != "0";
            var userId = HttpContext.Items["UserId"] as ulong?;

            var query = _db.TimesheetProjects
                .Include(p => p.Client)
                .Include(p => p.Filter)
                .AsQueryable();

            if (onlyVisible)
            {
                if (userId.HasValue)
                {
                    var projectIds = await _db.AppUserVisibleProjects
                        .Where(v => v.UserId == userId.Value)
                        .Select(v => v.ProjectId)
                        .ToListAsync();

                    if (projectIds.Any())
                        query = query.Where(p => projectIds.Contains(p.Id));
                    else
                        query = query.Where(p => p.Filter != null);
                }
                else
                {
                    query = query.Where(p => p.Filter != null);
                }
            }

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search));

            if (!string.IsNullOrEmpty(client))
                query = query.Where(p => p.Client != null && p.Client.Name.Contains(client));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrEmpty(code))
                query = query.Where(p => p.Code == code);

            query = query
                .OrderBy(p => p.Status == "activo" ? 0 : 1)
                .ThenBy(p => p.Code == null || p.Code == "" ? 1 : 0)
                .ThenBy(p => p.Code)
                .ThenBy(p => p.Name);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * per_page)
                .Take(per_page)
                .ToListAsync();

            foreach (var project in items)
            {
                try { await _bacService.RecalculateTotal(project); }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error recalculando BAC para proyecto {Id}", project.Id);
                }
            }

            var etcTotals = await _etcService.GetTotalHoursByProjectIds(items.Select(p => p.Id));

            var lastPage = (int)Math.Ceiling((double)total / per_page);

            return Ok(new
            {
                data = items.Select(p => MapToDto(p, etcTotals.GetValueOrDefault(p.Id, 0))),
                current_page = page,
                per_page,
                total,
                last_page = lastPage,
                from = (page - 1) * per_page + 1,
                to = Math.Min(page * per_page, total)
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al listar proyectos");
            return StatusCode(500, new { error = "Error al cargar proyectos", message = e.Message });
        }
    }

    // GET api/projects/evm
    [HttpGet("evm")]
    [RequirePermission("PROJECTS_ACCESS")]
    public async Task<IActionResult> EvmIndex()
    {
        return await GetAll(only_visible: "true");
    }

    // GET api/projects/{id}
    [HttpGet("{id}")]
    [RequirePermission("PROJECTS_ACCESS")]
    public async Task<IActionResult> GetById(ulong id)
    {
        var project = await _db.TimesheetProjects
            .Include(p => p.Client)
            .Include(p => p.Filter)
            .Include(p => p.ChangeRequests)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        await _bacService.RecalculateTotal(project);
        await _db.Entry(project).ReloadAsync();

        return Ok(MapToDto(project));
    }

    // PATCH api/projects/{id}/bac
    [HttpPatch("{id}/bac")]
    [RequirePermission("PROJECTS_CREATE")]
    public async Task<IActionResult> UpdateBac(ulong id, [FromBody] UpdateBacDto dto)
    {
        var project = await _db.TimesheetProjects.FindAsync(id);
        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        try
        {
            project = await _bacService.UpdateBaseAndRecalculate(project, dto.BacBaseHours, dto.BacBaseCost);

            if (!string.IsNullOrEmpty(dto.EtcCalculationMode))
            {
                if (dto.EtcCalculationMode != "manual" && dto.EtcCalculationMode != "automatic")
                    return UnprocessableEntity(new { message = "etc_calculation_mode debe ser manual o automatic" });

                project.EtcCalculationMode = dto.EtcCalculationMode;
                await _db.SaveChangesAsync();
            }

            return Ok(new { message = "BAC actualizado correctamente", project = MapToDto(project) });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { error = "Error al actualizar BAC", message = e.Message });
        }
    }

    // POST api/projects/{id}/recalculate-hours
    [HttpPost("{id}/recalculate-hours")]
    [RequirePermission("PROJECTS_CREATE")]
    public async Task<IActionResult> RecalculateHours(ulong id)
    {
        var project = await _db.TimesheetProjects.FindAsync(id);
        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        var timeEntries = await _db.TimesheetTimeEntries
            .Where(t => t.ProjectId == project.Id &&
                        (t.DurationHours == 0))
            .ToListAsync();

        var updated = 0;
        foreach (var entry in timeEntries)
        {
            try
            {
                var diff = entry.EndTime - entry.StartTime;
                var hours = (decimal)diff.TotalHours;
                if (hours > 0)
                {
                    entry.DurationHours = hours;
                    entry.UpdatedAt = DateTime.UtcNow;
                    updated++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error recalculando horas para time entry {Id}", entry.Id);
            }
        }

        await _db.SaveChangesAsync();
        var totalEntries = await _db.TimesheetTimeEntries.CountAsync(t => t.ProjectId == project.Id);

        return Ok(new
        {
            message = "Horas recalculadas exitosamente",
            updated,
            total_entries = totalEntries
        });
    }

    private ProjectDto MapToDto(TimesheetProject p, decimal etcTotalHours = 0) => new()
    {
        Id = p.Id,
        TimesheetProjectId = p.TimesheetProjectId,
        Name = p.Name,
        Code = p.Code,
        ClientId = p.ClientId,
        ClientName = p.Client?.Name,
        Status = p.Status,
        StartDate = p.StartDate,
        EndDatePlanned = p.EndDatePlanned,
        EndDateActual = p.EndDateActual,
        BacBaseHours = p.BacBaseHours,
        BacBaseCost = p.BacBaseCost,
        BacTotalHours = p.BacTotalHours,
        BacTotalCost = p.BacTotalCost,
        HourlyRate = p.HourlyRate,
        EtcCalculationMode = p.EtcCalculationMode,
        EtcTotalHours = etcTotalHours,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        Filter = p.Filter != null ? new { p.Filter.Id, p.Filter.ProjectId } : null
    };
}
