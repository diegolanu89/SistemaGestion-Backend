namespace bdt_evm_app.DTOs;

// ── Historial de desvíos ────────────────────────────────────────────────────

public class ProjectTrackingUpdateDto
{
    public ulong Id { get; set; }
    public ulong ProjectTrackingId { get; set; }
    public DateOnly? ChangeEndDate { get; set; }
    public string? Observations { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateTrackingUpdateDto
{
    public DateOnly? ChangeEndDate { get; set; }
    public string Observations { get; set; } = string.Empty;
}

public class UpdateTrackingUpdateDto
{
    public DateOnly? ChangeEndDate { get; set; }
    public string? Observations { get; set; }
}

// ── Fechas base ─────────────────────────────────────────────────────────────

public class ProjectTrackingDto
{
    public ulong Id { get; set; }
    public ulong ProjectId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? PlannedEndDate { get; set; }
    public DateOnly? ActualEndDate { get; set; }
    public DateOnly? ImplementationDate { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ProjectTrackingUpdateDto> Updates { get; set; } = new();
}

public class UpsertProjectTrackingDto
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? PlannedEndDate { get; set; }
    public DateOnly? ActualEndDate { get; set; }
    public DateOnly? ImplementationDate { get; set; }
}
