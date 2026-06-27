using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;
using bdt_evm_app.Services;

namespace bdt_evm_app.Controllers;

// RF-10: ADMIN_ACCESS se exige por método (mutaciones). El GET de la lista
// usa PROJECTS_ACCESS porque el Dashboard EVM lo lee para "Control de cambios".
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
    [RequirePermission("PROJECTS_ACCESS")]
    public async Task<IActionResult> GetByProject(ulong projectId)
    {
        var project = await _db.TimesheetProjects.FindAsync(projectId);
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
    [RequirePermission("ADMIN_ACCESS")]
    public async Task<IActionResult> Create(ulong projectId, [FromBody] CreateChangeRequestDto dto)
    {
        var project = await _db.TimesheetProjects.FindAsync(projectId);
        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        if (string.IsNullOrWhiteSpace(dto.Code))
            return UnprocessableEntity(new { message = "El código es requerido" });

        if (string.IsNullOrWhiteSpace(dto.Title))
            return UnprocessableEntity(new { message = "El título es requerido" });

        if (dto.RequestedDate is null)
            return UnprocessableEntity(new { message = "La fecha de solicitud es requerida" });

        var validStatuses = new[] { "propuesto", "aprobado", "rechazado", "implementado" };
        if (!validStatuses.Contains(dto.Status))
            return UnprocessableEntity(new { message = "Estado inválido" });

        var codeExists = await _db.ChangeRequests
            .AnyAsync(cr => cr.ProjectId == projectId && cr.Code == dto.Code);
        if (codeExists)
            return UnprocessableEntity(new { message = $"Ya existe un control de cambio con el código '{dto.Code}' en este proyecto" });

        // Solicitante y aprobador se estampan desde el usuario. aprobación es un 2do paso, así
        // que solo se persiste en aprobado/implementado
        var currentUserName = await ResolveCurrentUserNameAsync();
        var isApproved = IsApprovedStatus(dto.Status);

        var cr = new ChangeRequest
        {
            ProjectId = projectId,
            Code = dto.Code,
            Title = dto.Title,
            Description = dto.Description,
            RequestedBy = currentUserName,
            RequestedDate = dto.RequestedDate.Value,
            Status = dto.Status,
            BacHoursIncrement = dto.BacHoursIncrement ?? 0,
            BacCostIncrement = dto.BacCostIncrement ?? 0,
            ApprovedBy = isApproved ? currentUserName : null,
            ApprovedDate = isApproved ? (dto.ApprovedDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date)) : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.ChangeRequests.Add(cr);
        await _db.SaveChangesAsync();

        if (dto.Status == "aprobado")
            await _bacService.RecalculateTotal(project);

        // Auditoría de horas (RF-11): solo si el alta agrega horas.
        if (cr.BacHoursIncrement > 0)
            await LogHoursAdjustmentAsync(project, cr, previousHours: 0m, action: "alta");

        return StatusCode(201, new
        {
            message = "Control de cambio creado",
            change_request = MapToDto(cr)
        });
    }

    // PATCH api/change-requests/{id}
    [HttpPatch("api/change-requests/{id}")]
    [RequirePermission("ADMIN_ACCESS")]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateChangeRequestDto dto)
    {
        var cr = await _db.ChangeRequests.FindAsync(id);
        if (cr == null)
            return NotFound(new { message = "Solicitud de cambio no encontrada" });

        var project = await _db.TimesheetProjects.FindAsync(cr.ProjectId);
        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        var validStatuses = new[] { "propuesto", "aprobado", "rechazado", "implementado" };
        if (dto.Status != null && !validStatuses.Contains(dto.Status))
            return UnprocessableEntity(new { message = "Estado inválido" });

        var previousHours = cr.BacHoursIncrement;

        // Código editable: si cambia, validar que no quede vacío ni duplicado en el proyecto.
        if (dto.Code != null && dto.Code != cr.Code)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return UnprocessableEntity(new { message = "El código es requerido" });

            var codeExists = await _db.ChangeRequests
                .AnyAsync(c => c.ProjectId == cr.ProjectId && c.Code == dto.Code && c.Id != cr.Id);
            if (codeExists)
                return UnprocessableEntity(new { message = $"Ya existe un control de cambio con el código '{dto.Code}' en este proyecto" });

            cr.Code = dto.Code;
        }

        if (dto.Title != null) cr.Title = dto.Title;
        if (dto.Description != null) cr.Description = dto.Description;
        // El solicitante queda fijo desde el alta: no se reasigna al editar.
        if (dto.RequestedDate.HasValue) cr.RequestedDate = dto.RequestedDate.Value;
        if (dto.Status != null) cr.Status = dto.Status;
        if (dto.BacHoursIncrement.HasValue) cr.BacHoursIncrement = dto.BacHoursIncrement.Value;
        if (dto.BacCostIncrement.HasValue) cr.BacCostIncrement = dto.BacCostIncrement.Value;
        if (dto.ApprovedDate.HasValue) cr.ApprovedDate = dto.ApprovedDate;

        // Aprobación: fuera de aprobado/implementado no hay datos de aprobación.
        // Al entrar en esos estados se estampa el aprobador y
        // la fecha (hoy si no se cargó) la PRIMERA vez; no se sobreescriben después.
        if (!IsApprovedStatus(cr.Status))
        {
            cr.ApprovedBy = null;
            cr.ApprovedDate = null;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(cr.ApprovedBy))
                cr.ApprovedBy = await ResolveCurrentUserNameAsync();

            if (cr.ApprovedDate is null)
                cr.ApprovedDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        }

        cr.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _bacService.RecalculateTotal(project);

        // Auditoría de horas (RF-11): solo si la edición cambió las horas.
        if (dto.BacHoursIncrement.HasValue && cr.BacHoursIncrement != previousHours)
            await LogHoursAdjustmentAsync(project, cr, previousHours, action: "modificacion");

        return Ok(new
        {
            message = "Control de cambio actualizado",
            change_request = MapToDto(cr)
        });
    }

    // GET api/projects/{id}/change-log
    [HttpGet("api/projects/{projectId}/change-log")]
    [RequirePermission("PROJECTS_ACCESS")]
    public async Task<IActionResult> GetChangeLog(ulong projectId)
        => await GetByProject(projectId);

    // POST api/projects/{id}/change-log
    [HttpPost("api/projects/{projectId}/change-log")]
    [RequirePermission("ADMIN_ACCESS")]
    public async Task<IActionResult> CreateChangeLog(ulong projectId, [FromBody] CreateChangeRequestDto dto)
        => await Create(projectId, dto);

    // PUT api/projects/{id}/change-log/{changeId}
    [HttpPut("api/projects/{projectId}/change-log/{changeId}")]
    [RequirePermission("ADMIN_ACCESS")]
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
    [RequirePermission("ADMIN_ACCESS")]
    public async Task<IActionResult> DeleteChangeLog(ulong projectId, ulong changeId)
    {
        var cr = await _db.ChangeRequests.FindAsync(changeId);
        if (cr == null)
            return NotFound(new { message = "Solicitud de cambio no encontrada" });

        if (cr.ProjectId != projectId)
            return BadRequest(new { error = "La solicitud de cambio no pertenece a este proyecto" });

        var project = await _db.TimesheetProjects.FindAsync(cr.ProjectId);

        _db.ChangeRequests.Remove(cr);
        await _db.SaveChangesAsync();

        if (project != null)
            await _bacService.RecalculateTotal(project);

        return Ok(new { message = "Control de cambio eliminado" });
    }

    private static readonly JsonSerializerOptions _auditJson = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // Escribe una entrada de auditoría explícita para los ajustes de horas
    // hechos desde "Control de cambios". El interceptor genérico ya audita el
    // diff crudo del ChangeRequest; esta entrada agrega lo que pidió negocio:
    // la SUMATORIA de horas del proyecto, la pantalla de origen y un comentario
    // legible. Todo viaja en new_value (sin cambios de schema).
    private async Task LogHoursAdjustmentAsync(TimesheetProject project, ChangeRequest cr, decimal previousHours, string action)
    {
        // Sumatoria real al momento del log: suma de incrementos de TODOS los
        // controles de cambio del proyecto (independiente de su estado, igual
        // criterio que ProjectBacService.RecalculateTotal).
        var sumatoriaHoras = await _db.ChangeRequests
            .Where(c => c.ProjectId == project.Id && c.BacHoursIncrement > 0)
            .SumAsync(c => c.BacHoursIncrement);

        var (userId, userEmail) = ResolveAuditUser();

        var payload = new
        {
            pantalla = "control_de_cambios",
            comentario = "ajustes de hora desde control de cambios",
            accion = action,
            changeRequestId = cr.Id,
            changeRequestCode = cr.Code,
            horasAnteriores = previousHours,
            horasDeEsteCambio = cr.BacHoursIncrement,
            sumatoriaHorasControlesDeCambio = sumatoriaHoras,
            bacTotalHoras = project.BacTotalHours
        };

        _db.ChangeAuditLogs.Add(new ChangeAuditLog
        {
            Ts = DateTime.UtcNow,
            UserId = userId,
            UserEmail = userEmail,
            Module = "operations",
            Entity = "ChangeRequestHoursAdjustment",
            RecordId = cr.Id.ToString(),
            EventType = AuditEventType.Update,
            NewValue = JsonSerializer.Serialize(payload, _auditJson),
            RequestId = HttpContext.TraceIdentifier,
            Ip = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        await _db.SaveChangesAsync();
    }

    // Resuelve el usuario del request desde HttpContext.Items (lo setea el
    // SanctumAuthMiddleware), igual que el AuditSaveChangesInterceptor.
    private (ulong?, string?) ResolveAuditUser()
    {
        ulong? userId = HttpContext.Items.TryGetValue("UserId", out var raw) && raw is ulong u ? u : null;
        var email = HttpContext.Items.TryGetValue("UserEmail", out var er) ? er as string : null;
        return (userId, email);
    }

    // La aprobación solo aplica en estos estados.
    private static bool IsApprovedStatus(string status) => status is "aprobado" or "implementado";

    // Nombre para mostrar del usuario autenticado (lo setea SanctumAuthMiddleware
    // en HttpContext.Items). Se usa para estampar solicitante/aprobador en vez de
    // aceptar texto libre. Fallback al email si no se puede resolver el nombre.
    private async Task<string?> ResolveCurrentUserNameAsync()
    {
        if (HttpContext.Items.TryGetValue("UserId", out var raw) && raw is ulong uid)
        {
            var user = await _db.Users.FindAsync(uid);
            if (user != null && !string.IsNullOrWhiteSpace(user.Name))
                return user.Name;
        }

        return HttpContext.Items.TryGetValue("UserEmail", out var er) ? er as string : null;
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