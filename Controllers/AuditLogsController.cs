using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;

namespace bdt_evm_app.Controllers;

// RF-11 — pantalla de consulta del log de auditoría.
//
// Todos los endpoints son READ-ONLY: el ERS prohíbe explícitamente
// "edición ni eliminación manual de los registros de auditoría desde
// la interfaz funcional del sistema". No hay POST/PUT/DELETE.
//
// El listado base ordena por ts DESC y aplica los filtros que pide el ERS
// (rango de fechas, usuario, módulo) más entity y event_type para soporte.
// La paginación es obligatoria — la tabla puede crecer mucho.
[ApiController]
[Route("api/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuditLogsController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/audit-logs?from=&to=&userId=&module=&entity=&eventType=&page=&pageSize=
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AuditLogQueryDto query)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var q = _db.ChangeAuditLogs.AsNoTracking().AsQueryable();

        if (query.From.HasValue) q = q.Where(a => a.Ts >= query.From.Value);
        if (query.To.HasValue)   q = q.Where(a => a.Ts <  query.To.Value);
        if (query.UserId.HasValue) q = q.Where(a => a.UserId == query.UserId);
        if (!string.IsNullOrWhiteSpace(query.Module))    q = q.Where(a => a.Module == query.Module);
        if (!string.IsNullOrWhiteSpace(query.Entity))    q = q.Where(a => a.Entity == query.Entity);
        if (!string.IsNullOrWhiteSpace(query.EventType)) q = q.Where(a => a.EventType == query.EventType);

        var total = await q.LongCountAsync();

        var items = await q
            .OrderByDescending(a => a.Ts)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new AuditLogListItemDto
            {
                Id = a.Id,
                Ts = a.Ts,
                UserId = a.UserId,
                UserEmail = a.UserEmail,
                Module = a.Module,
                Entity = a.Entity,
                RecordId = a.RecordId,
                EventType = a.EventType
            })
            .ToListAsync();

        return Ok(new AuditLogPageDto
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            Total = total
        });
    }

    // GET api/audit-logs/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(ulong id)
    {
        var row = await _db.ChangeAuditLogs.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
        if (row == null) return NotFound(new { message = "Registro de auditoría no encontrado" });

        return Ok(new AuditLogDetailDto
        {
            Id = row.Id,
            Ts = row.Ts,
            UserId = row.UserId,
            UserEmail = row.UserEmail,
            Module = row.Module,
            Entity = row.Entity,
            RecordId = row.RecordId,
            EventType = row.EventType,
            OldValue = row.OldValue,
            NewValue = row.NewValue,
            RequestId = row.RequestId,
            Ip = row.Ip
        });
    }

    // GET api/audit-logs/modules — catálogo para poblar el dropdown del filtro.
    // Devuelve los códigos de módulo realmente presentes en el log; evita
    // ofrecer filtros vacíos.
    [HttpGet("modules")]
    public async Task<IActionResult> GetModules()
    {
        var modules = await _db.ChangeAuditLogs
            .AsNoTracking()
            .Where(a => a.Module != null)
            .Select(a => a.Module!)
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync();
        return Ok(modules);
    }

    // GET api/audit-logs/entities — catálogo de entidades auditadas (para filtro).
    [HttpGet("entities")]
    public async Task<IActionResult> GetEntities()
    {
        var entities = await _db.ChangeAuditLogs
            .AsNoTracking()
            .Select(a => a.Entity)
            .Distinct()
            .OrderBy(e => e)
            .ToListAsync();
        return Ok(entities);
    }

    // GET api/audit-logs/event-types — valores válidos de event_type.
    [HttpGet("event-types")]
    public IActionResult GetEventTypes()
        => Ok(new[] { "create", "update", "delete", "login", "logout" });
}
