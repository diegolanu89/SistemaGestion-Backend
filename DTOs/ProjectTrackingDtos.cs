namespace bdt_evm_app.DTOs;

// ── Resumen embebido en proyectos e intakes ─────────────────────────────────

public class ProjectTrackingSummaryDto
{
    public ulong Id { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? PlannedEndDate { get; set; }
    public DateOnly? ActualEndDate { get; set; }
    public DateOnly? ImplementationDate { get; set; }
}

// ── Historial de desvíos ────────────────────────────────────────────────────

public class ProjectTrackingUpdateDto
{
    public ulong Id { get; set; }
    public ulong ProjectTrackingId { get; set; }
    public DateOnly? MilestoneDate { get; set; }
    public string? Observations { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateTrackingUpdateDto
{
    public DateOnly MilestoneDate { get; set; }
    public string Observations { get; set; } = string.Empty;
}

public class UpdateTrackingUpdateDto
{
    public DateOnly? MilestoneDate { get; set; }
    public string? Observations { get; set; }
}

// ── Fechas base ─────────────────────────────────────────────────────────────

public class ProjectTrackingDto
{
    public ulong Id { get; set; }
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
