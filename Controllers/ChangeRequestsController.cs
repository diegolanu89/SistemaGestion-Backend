using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;
using bdt_evm_app.Services;

namespace bdt_evm_app.Controllers;

[ApiController]
public class ChangeRequestsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProjectBacService _bacService;
    private readonly ILogger<ChangeRequestsController> _logger;

    public ChangeRequestsController(AppDbContext db, ProjectBacService bacService, ILogger<ChangeRequestsController> logger)
    {
        _db = db;
        _bacService = bacService;
        _logger = logger;
    }

    // GET api/projects/{id}/change-requests
    [HttpGet("api/projects/{projectId}/change-requests")]
    public async Task<IActionResult> GetByProject(ulong projectId)
    {
        var project = await _db.ClockifyProjects.FindAsync(projectId);
        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        await _bacService.RecalculateTotal(project);

        var crs = await _db.ChangeRequests
            .Where(cr => cr.ProjectId == projectId)
            .OrderByDescending(cr => cr.RequestedDate)
            .ToListAsync();

        return Ok(crs.Select(MapToDto));
    }

    // POST api/projects/{id}/change-requests
    [HttpPost("api/projects/{projectId}/change-requests")]
    public async Task<IActionResult> Create(ulong projectId, [FromBody] CreateChangeRequestDto dto)
    {
        var project = await _db.ClockifyProjects.FindAsync(projectId);
        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        if (string.IsNullOrWhiteSpace(dto.Code))
            return UnprocessableEntity(new { message = "El código es requerido" });

        if (string.IsNullOrWhiteSpace(dto.Title))
            return UnprocessableEntity(new { message = "El título es requerido" });

        var validStatuses = new[] { "propuesto", "aprobado", "rechazado", "implementado" };
        if (!validStatuses.Contains(dto.Status))
            return UnprocessableEntity(new { message = "Estado inválido" });

        var codeExists = await _db.ChangeRequests
            .AnyAsync(cr => cr.ProjectId == projectId && cr.Code == dto.Code);
        if (codeExists)
            return UnprocessableEntity(new { message = $"Ya existe un control de cambio con el código '{dto.Code}' en este proyecto" });

        var cr = new ChangeRequest
        {
            ProjectId = projectId,
            Code = dto.Code,
            Title = dto.Title,
            Description = dto.Description,
            RequestedBy = dto.RequestedBy,
            RequestedDate = dto.RequestedDate,
            Status = dto.Status,
            BacHoursIncrement = dto.BacHoursIncrement ?? 0,
            BacCostIncrement = dto.BacCostIncrement ?? 0,
            ApprovedBy = dto.ApprovedBy,
            ApprovedDate = dto.ApprovedDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.ChangeRequests.Add(cr);
        await _db.SaveChangesAsync();

        if (dto.Status == "aprobado")
            await _bacService.RecalculateTotal(project);

        return StatusCode(201, new
        {
            message = "Control de cambio creado",
            change_request = MapToDto(cr)
        });
    }

    // PATCH api/change-requests/{id}
    [HttpPatch("api/change-requests/{id}")]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateChangeRequestDto dto)
    {
        var cr = await _db.ChangeRequests.FindAsync(id);
        if (cr == null)
            return NotFound(new { message = "Solicitud de cambio no encontrada" });

        var project = await _db.ClockifyProjects.FindAsync(cr.ProjectId);
        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        var validStatuses = new[] { "propuesto", "aprobado", "rechazado", "implementado" };
        if (dto.Status != null && !validStatuses.Contains(dto.Status))
            return UnprocessableEntity(new { message = "Estado inválido" });

        if (dto.Title != null) cr.Title = dto.Title;
        if (dto.Description != null) cr.Description = dto.Description;
        if (dto.Status != null) cr.Status = dto.Status;
        if (dto.BacHoursIncrement.HasValue) cr.BacHoursIncrement = dto.BacHoursIncrement.Value;
        if (dto.BacCostIncrement.HasValue) cr.BacCostIncrement = dto.BacCostIncrement.Value;
        if (dto.ApprovedBy != null) cr.ApprovedBy = dto.ApprovedBy;
        if (dto.ApprovedDate.HasValue) cr.ApprovedDate = dto.ApprovedDate;
        cr.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _bacService.RecalculateTotal(project);

        return Ok(new
        {
            message = "Control de cambio actualizado",
            change_request = MapToDto(cr)
        });
    }

    // GET api/projects/{id}/change-log
    [HttpGet("api/projects/{projectId}/change-log")]
    public async Task<IActionResult> GetChangeLog(ulong projectId)
        => await GetByProject(projectId);

    // POST api/projects/{id}/change-log
    [HttpPost("api/projects/{projectId}/change-log")]
    public async Task<IActionResult> CreateChangeLog(ulong projectId, [FromBody] CreateChangeRequestDto dto)
        => await Create(projectId, dto);

    // PUT api/projects/{id}/change-log/{changeId}
    [HttpPut("api/projects/{projectId}/change-log/{changeId}")]
    public async Task<IActionResult> UpdateChangeLog(ulong projectId, ulong changeId, [FromBody] UpdateChangeRequestDto dto)
    {
        var cr = await _db.ChangeRequests.FindAsync(changeId);
        if (cr == null)
            return NotFound(new { message = "Solicitud de cambio no encontrada" });

        if (cr.ProjectId != projectId)
            return BadRequest(new { error = "La solicitud de cambio no pertenece a este proyecto" });

        return await Update(changeId, dto);
    }

    // DELETE api/projects/{id}/change-log/{changeId}
    [HttpDelete("api/projects/{projectId}/change-log/{changeId}")]
    public async Task<IActionResult> DeleteChangeLog(ulong projectId, ulong changeId)
    {
        var cr = await _db.ChangeRequests.FindAsync(changeId);
        if (cr == null)
            return NotFound(new { message = "Solicitud de cambio no encontrada" });

        if (cr.ProjectId != projectId)
            return BadRequest(new { error = "La solicitud de cambio no pertenece a este proyecto" });

        var project = await _db.ClockifyProjects.FindAsync(cr.ProjectId);

        _db.ChangeRequests.Remove(cr);
        await _db.SaveChangesAsync();

        if (project != null)
            await _bacService.RecalculateTotal(project);

        return Ok(new { message = "Control de cambio eliminado" });
    }

    private static ChangeRequestDto MapToDto(ChangeRequest cr) => new()
    {
        Id = cr.Id,
        ProjectId = cr.ProjectId,
        Code = cr.Code,
        Title = cr.Title,
        Description = cr.Description,
        RequestedBy = cr.RequestedBy,
        RequestedDate = cr.RequestedDate,
        Status = cr.Status,
        BacHoursIncrement = cr.BacHoursIncrement,
        BacCostIncrement = cr.BacCostIncrement,
        ApprovedBy = cr.ApprovedBy,
        ApprovedDate = cr.ApprovedDate,
        CreatedAt = cr.CreatedAt,
        UpdatedAt = cr.UpdatedAt
    };
}