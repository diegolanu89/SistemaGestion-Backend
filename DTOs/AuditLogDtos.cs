using System.ComponentModel.DataAnnotations;

namespace bdt_evm_app.DTOs;

// Filtros de la pantalla de consulta (RF-11):
// "rango de fechas, usuario, módulo".
// Agregamos `entity` y `event_type` como filtros adicionales útiles
// para el día a día del soporte.
public class AuditLogQueryDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public ulong? UserId { get; set; }
    public string? Module { get; set; }
    public string? Entity { get; set; }
    public string? EventType { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 200)]
    public int PageSize { get; set; } = 50;
}

public class AuditLogListItemDto
{
    public ulong Id { get; set; }
    public DateTime Ts { get; set; }
    public ulong? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? Module { get; set; }
    public string Entity { get; set; } = string.Empty;
    public string? RecordId { get; set; }
    public string EventType { get; set; } = string.Empty;
}

public class AuditLogDetailDto : AuditLogListItemDto
{
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? RequestId { get; set; }
    public string? Ip { get; set; }
}

public class AuditLogPageDto
{
    public IEnumerable<AuditLogListItemDto> Items { get; set; } = Array.Empty<AuditLogListItemDto>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long Total { get; set; }
}
