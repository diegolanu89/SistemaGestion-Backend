using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;

namespace bdt_evm_app.Controllers;

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

    // GET /api/project-trackings/{trackingId}
    [HttpGet("{trackingId}")]
    [RequirePermission("PROJECTS_ACCESS")]
    public async Task<IActionResult> GetById(ulong trackingId)
    {
        try
        {
            var tracking = await _db.ProjectTrackings
                .Include(t => t.Updates.OrderByDescending(u => u.CreatedAt))
                .FirstOrDefaultAsync(t => t.Id == trackingId);

            if (tracking == null)
                return NotFound(new { success = false, message = "Seguimiento no encontrado" });

            return Ok(new { success = true, data = MapToDto(tracking) });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener tracking {TrackingId}", trackingId);
            return StatusCode(500, new { success = false, message = "Error al obtener el seguimiento", error = e.Message });
        }
    }

    // PUT /api/project-trackings/{trackingId}
    // Actualiza las fechas base (Bloque 1) — creación vía ProjectIntake o sync Clockify
    [HttpPut("{trackingId}")]
    [RequirePermission("PROJECTS_ACCESS")]
    public async Task<IActionResult> Update(ulong trackingId, [FromBody] UpsertProjectTrackingDto dto)
    {
        try
        {
            var tracking = await _db.ProjectTrackings
                .Include(t => t.Updates.OrderByDescending(u => u.CreatedAt))
                .FirstOrDefaultAsync(t => t.Id == trackingId);

            if (tracking == null)
                return NotFound(new { success = false, message = "Seguimiento no encontrado" });

            if (dto.StartDate.HasValue)          tracking.StartDate          = dto.StartDate;
            if (dto.PlannedEndDate.HasValue)     tracking.PlannedEndDate     = dto.PlannedEndDate;
            if (dto.ActualEndDate.HasValue)      tracking.ActualEndDate      = dto.ActualEndDate;
            if (dto.ImplementationDate.HasValue) tracking.ImplementationDate = dto.ImplementationDate;
            tracking.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Fechas base actualizadas exitosamente", data = MapToDto(tracking) });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al actualizar tracking {TrackingId}", trackingId);
            return StatusCode(500, new { success = false, message = "Error al actualizar el seguimiento", error = e.Message });
        }
    }

    // POST /api/project-trackings/{trackingId}/updates
    // Agrega un registro al historial de desvíos (Bloque 2)
    [HttpPost("{trackingId}/updates")]
    [RequirePermission("PROJECTS_ACCESS")]
    public async Task<IActionResult> AddUpdate(ulong trackingId, [FromBody] CreateTrackingUpdateDto dto)
    {
        try
        {
            if (dto.MilestoneDate == default)
                return UnprocessableEntity(new { success = false, message = "milestone_date es obligatorio" });
            if (string.IsNullOrWhiteSpace(dto.Observations))
                return UnprocessableEntity(new { success = false, message = "Las observaciones son obligatorias" });

            var trackingExists = await _db.ProjectTrackings.AnyAsync(t => t.Id == trackingId);
            if (!trackingExists)
                return NotFound(new { success = false, message = "Seguimiento no encontrado" });

            var update = new ProjectTrackingUpdate
            {
                ProjectTrackingId = trackingId,
                MilestoneDate = dto.MilestoneDate,
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
            _logger.LogError(e, "Error al agregar update al tracking {TrackingId}", trackingId);
            return StatusCode(500, new { success = false, message = "Error al agregar al historial", error = e.Message });
        }
    }

    // PUT /api/project-trackings/{trackingId}/updates/{updateId}
    // Edita un registro del historial
    [HttpPut("{trackingId}/updates/{updateId}")]
    [RequirePermission("PROJECTS_ACCESS")]
    public async Task<IActionResult> EditUpdate(ulong trackingId, ulong updateId, [FromBody] UpdateTrackingUpdateDto dto)
    {
        try
        {
            var update = await _db.ProjectTrackingUpdates
                .FirstOrDefaultAsync(u => u.Id == updateId && u.ProjectTrackingId == trackingId);

            if (update == null)
                return NotFound(new { success = false, message = "Registro del historial no encontrado" });

            if (dto.MilestoneDate.HasValue) update.MilestoneDate = dto.MilestoneDate;
            if (dto.Observations != null) update.Observations = dto.Observations;
            update.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Registro actualizado exitosamente", data = MapUpdateToDto(update) });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al editar update {UpdateId} del tracking {TrackingId}", updateId, trackingId);
            return StatusCode(500, new { success = false, message = "Error al editar el registro", error = e.Message });
        }
    }

    // DELETE /api/project-trackings/{trackingId}/updates/{updateId}
    // Eliminación física — el historial es auditoría
    [HttpDelete("{trackingId}/updates/{updateId}")]
    [RequirePermission("PROJECTS_ACCESS")]
    public async Task<IActionResult> DeleteUpdate(ulong trackingId, ulong updateId)
    {
        try
        {
            var update = await _db.ProjectTrackingUpdates
                .FirstOrDefaultAsync(u => u.Id == updateId && u.ProjectTrackingId == trackingId);

            if (update == null)
                return NotFound(new { success = false, message = "Registro del historial no encontrado" });

            _db.ProjectTrackingUpdates.Remove(update);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Registro eliminado del historial" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar update {UpdateId} del tracking {TrackingId}", updateId, trackingId);
            return StatusCode(500, new { success = false, message = "Error al eliminar el registro", error = e.Message });
        }
    }

    private static ProjectTrackingDto MapToDto(ProjectTracking t) => new()
    {
        Id = t.Id,
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
        MilestoneDate = u.MilestoneDate,
        Observations = u.Observations,
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt
    };
}
