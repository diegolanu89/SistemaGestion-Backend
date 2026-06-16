using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;

namespace bdt_evm_app.Controllers;

// RF-10: PROJECTS_CREATE se exige por método (POST/PUT/DELETE). El GET
// usa PROJECTS_ACCESS porque el Dashboard EVM lo lee para mostrar "Control
// de cambios" y abrir el modal de seguimiento.
[ApiController]
[Route("api/project-trackings")]
public class ProjectTrackingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProjectTrackingsController> _logger;

    public ProjectTrackingsController(AppDbContext db, ILogger<ProjectTrackingsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET /api/project-trackings/{projectId}
    [HttpGet("{projectId}")]
    [RequirePermission("PROJECTS_ACCESS")]
    public async Task<IActionResult> GetByProject(ulong projectId)
    {
        try
        {
            var projectExists = await _db.TimesheetProjects.AnyAsync(p => p.Id == projectId);
            if (!projectExists)
                return NotFound(new { success = false, message = "Proyecto no encontrado" });

            var tracking = await _db.ProjectTrackings
                .Include(t => t.Updates.OrderByDescending(u => u.CreatedAt))
                .FirstOrDefaultAsync(t => t.ProjectId == projectId);

            if (tracking == null)
                return Ok(new { success = true, data = (object?)null });

            return Ok(new { success = true, data = MapToDto(tracking) });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener tracking del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { success = false, message = "Error al obtener el seguimiento", error = e.Message });
        }
    }

    // POST /api/project-trackings/{projectId}
    // Crea el tracking de fechas base (solo si no existe)
    [HttpPost("{projectId}")]
    [RequirePermission("PROJECTS_CREATE")]
    public async Task<IActionResult> Create(ulong projectId, [FromBody] UpsertProjectTrackingDto dto)
    {
        try
        {
            if (dto.StartDate == default)
                return UnprocessableEntity(new { success = false, message = "start_date es obligatorio" });
            if (dto.PlannedEndDate == default)
                return UnprocessableEntity(new { success = false, message = "planned_end_date es obligatorio" });

            var projectExists = await _db.TimesheetProjects.AnyAsync(p => p.Id == projectId);
            if (!projectExists)
                return NotFound(new { success = false, message = "Proyecto no encontrado" });

            var alreadyExists = await _db.ProjectTrackings.AnyAsync(t => t.ProjectId == projectId);
            if (alreadyExists)
                return UnprocessableEntity(new { success = false, message = "El proyecto ya tiene un seguimiento registrado. Usá PUT para actualizar." });

            var tracking = new ProjectTracking
            {
                ProjectId = projectId,
                StartDate = dto.StartDate,
                PlannedEndDate = dto.PlannedEndDate,
                ActualEndDate = dto.ActualEndDate,
                ImplementationDate = dto.ImplementationDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.ProjectTrackings.Add(tracking);
            await _db.SaveChangesAsync();

            return StatusCode(201, new { success = true, message = "Seguimiento creado exitosamente", data = MapToDto(tracking) });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al crear tracking del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { success = false, message = "Error al crear el seguimiento", error = e.Message });
        }
    }

    // PUT /api/project-trackings/{projectId}
    // Actualiza las fechas base (Bloque 1)
    [HttpPut("{projectId}")]
    [RequirePermission("PROJECTS_CREATE")]
    public async Task<IActionResult> Update(ulong projectId, [FromBody] UpsertProjectTrackingDto dto)
    {
        try
        {
            var tracking = await _db.ProjectTrackings
                .Include(t => t.Updates.OrderByDescending(u => u.CreatedAt))
                .FirstOrDefaultAsync(t => t.ProjectId == projectId);

            if (tracking == null)
                return NotFound(new { success = false, message = "El proyecto no tiene seguimiento registrado. Usá POST para crear." });

            if (dto.StartDate == default)
                return UnprocessableEntity(new { success = false, message = "start_date es obligatorio" });
            if (dto.PlannedEndDate == default)
                return UnprocessableEntity(new { success = false, message = "planned_end_date es obligatorio" });

            tracking.StartDate = dto.StartDate;
            tracking.PlannedEndDate = dto.PlannedEndDate;
            tracking.ActualEndDate = dto.ActualEndDate;
            tracking.ImplementationDate = dto.ImplementationDate;
            tracking.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Fechas base actualizadas exitosamente", data = MapToDto(tracking) });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al actualizar tracking del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { success = false, message = "Error al actualizar el seguimiento", error = e.Message });
        }
    }

    // POST /api/project-trackings/{projectId}/updates
    // Agrega un registro al historial de desvíos (Bloque 2)
    [HttpPost("{projectId}/updates")]
    [RequirePermission("PROJECTS_CREATE")]
    public async Task<IActionResult> AddUpdate(ulong projectId, [FromBody] CreateTrackingUpdateDto dto)
    {
        try
        {
            if (dto.ChangeEndDate == default)
                return UnprocessableEntity(new { success = false, message = "change_end_date es obligatorio" });
            if (string.IsNullOrWhiteSpace(dto.Observations))
                return UnprocessableEntity(new { success = false, message = "Las observaciones son obligatorias" });

            var tracking = await _db.ProjectTrackings
                .FirstOrDefaultAsync(t => t.ProjectId == projectId);

            if (tracking == null)
                return NotFound(new { success = false, message = "El proyecto no tiene seguimiento registrado. Creá primero las fechas base." });

            var update = new ProjectTrackingUpdate
            {
                ProjectTrackingId = tracking.Id,
                ChangeEndDate = dto.ChangeEndDate,
                Observations = dto.Observations,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.ProjectTrackingUpdates.Add(update);
            await _db.SaveChangesAsync();

            return StatusCode(201, new { success = true, message = "Registro agregado al historial", data = MapUpdateToDto(update) });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al agregar update al tracking del proyecto {ProjectId}", projectId);
            return StatusCode(500, new { success = false, message = "Error al agregar al historial", error = e.Message });
        }
    }

    // PUT /api/project-trackings/{projectId}/updates/{updateId}
    // Edita un registro del historial
    [HttpPut("{projectId}/updates/{updateId}")]
    [RequirePermission("PROJECTS_CREATE")]
    public async Task<IActionResult> EditUpdate(ulong projectId, ulong updateId, [FromBody] UpdateTrackingUpdateDto dto)
    {
        try
        {
            var tracking = await _db.ProjectTrackings
                .FirstOrDefaultAsync(t => t.ProjectId == projectId);

            if (tracking == null)
                return NotFound(new { success = false, message = "El proyecto no tiene seguimiento registrado" });

            var update = await _db.ProjectTrackingUpdates
                .FirstOrDefaultAsync(u => u.Id == updateId && u.ProjectTrackingId == tracking.Id);

            if (update == null)
                return NotFound(new { success = false, message = "Registro del historial no encontrado" });

            if (dto.ChangeEndDate.HasValue) update.ChangeEndDate = dto.ChangeEndDate;
            if (dto.Observations != null) update.Observations = dto.Observations;
            update.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Registro actualizado exitosamente", data = MapUpdateToDto(update) });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al editar update {UpdateId} del proyecto {ProjectId}", updateId, projectId);
            return StatusCode(500, new { success = false, message = "Error al editar el registro", error = e.Message });
        }
    }

    // DELETE /api/project-trackings/{projectId}/updates/{updateId}
    // Elimina un registro del historial (eliminación física — el historial es auditoría)
    [HttpDelete("{projectId}/updates/{updateId}")]
    [RequirePermission("PROJECTS_CREATE")]
    public async Task<IActionResult> DeleteUpdate(ulong projectId, ulong updateId)
    {
        try
        {
            var tracking = await _db.ProjectTrackings
                .FirstOrDefaultAsync(t => t.ProjectId == projectId);

            if (tracking == null)
                return NotFound(new { success = false, message = "El proyecto no tiene seguimiento registrado" });

            var update = await _db.ProjectTrackingUpdates
                .FirstOrDefaultAsync(u => u.Id == updateId && u.ProjectTrackingId == tracking.Id);

            if (update == null)
                return NotFound(new { success = false, message = "Registro del historial no encontrado" });

            _db.ProjectTrackingUpdates.Remove(update);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Registro eliminado del historial" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar update {UpdateId} del proyecto {ProjectId}", updateId, projectId);
            return StatusCode(500, new { success = false, message = "Error al eliminar el registro", error = e.Message });
        }
    }

    private static ProjectTrackingDto MapToDto(ProjectTracking t) => new()
    {
        Id = t.Id,
        ProjectId = t.ProjectId,
        StartDate = t.StartDate,
        PlannedEndDate = t.PlannedEndDate,
        ActualEndDate = t.ActualEndDate,
        ImplementationDate = t.ImplementationDate,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt,
        Updates = t.Updates.Select(MapUpdateToDto).ToList()
    };

    private static ProjectTrackingUpdateDto MapUpdateToDto(ProjectTrackingUpdate u) => new()
    {
        Id = u.Id,
        ProjectTrackingId = u.ProjectTrackingId,
        ChangeEndDate = u.ChangeEndDate,
        Observations = u.Observations,
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt
    };
}
