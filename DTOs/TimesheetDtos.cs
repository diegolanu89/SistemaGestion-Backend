namespace bdt_evm_app.DTOs;

// DTOs neutros para la sincronización de time tracking. No dependen de Clockify:
// cada proveedor (adapter) mapea su payload crudo a estos tipos, y la lógica de
// negocio (TimesheetSyncService) trabaja solo contra ellos.

public record TimesheetClientDto
{
    public string ExternalId { get; init; } = string.Empty;
    public string Name { get; init; } = "Cliente sin nombre";
    public bool Archived { get; init; }
}

public record TimesheetUserDto
{
    public string ExternalId { get; init; } = string.Empty;
    public string Name { get; init; } = "Usuario sin nombre";
    public string? Email { get; init; }
    public bool Active { get; init; } = true;
}

public record TimesheetProjectDto
{
    public string ExternalId { get; init; } = string.Empty;
    public string Name { get; init; } = "Proyecto sin nombre";
    public string? Code { get; init; }
    public bool Archived { get; init; }
    public string? ClientExternalId { get; init; }
}

public record TimesheetEntryDto
{
    public string ExternalId { get; init; } = string.Empty;
    public string? ProjectExternalId { get; init; }
    public string? UserExternalId { get; init; }
    public string? Description { get; init; }
    public DateTime? Start { get; init; }
    public DateTime? End { get; init; }
    public decimal DurationHours { get; init; }
    public bool Billable { get; init; }
    // Payload crudo del proveedor, tal cual llegó. Solo se persiste (source_raw); nunca se re-parsea.
    public string RawPayload { get; init; } = string.Empty;
}
