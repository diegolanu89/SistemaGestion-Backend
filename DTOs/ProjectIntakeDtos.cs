namespace bdt_evm_app.DTOs;

// Ref DTOs (para dropdowns / options)

public class ProjectIntakeTypeRefDto
{
    public ulong Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string InternalLabel { get; set; } = string.Empty;
    public string SecondaryLabel { get; set; } = string.Empty;
    public string RegistrationLabel { get; set; } = string.Empty;
    public bool RequiresBusinessStatusDate { get; set; }
    public bool RequiresActualEndDate { get; set; }
    public bool RequiresCommercialFields { get; set; }
    public bool IsActive { get; set; }
}

public class ProjectIntakeCategoryRefDto
{
    public ulong Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class ProjectIntakeStatusRefDto
{
    public ulong Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

// Response DTOs

public class ProjectIntakeRecordDto
{
    public ulong Id { get; set; }
    public string? ProjectType { get; set; }
    public string? InternalProjectNumber { get; set; }
    public string? SecondaryProjectNumber { get; set; }
    public DateOnly? RegistrationDate { get; set; }
    public ulong? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ProjectName { get; set; }
    public string? CategoryCode { get; set; }
    public string? ProjectStatusCode { get; set; }
    public DateOnly? BusinessStatusDate { get; set; }
    public DateOnly? EstimatedEndDate { get; set; }
    public DateOnly? ActualEndDate { get; set; }
    public string? CommercialStatus { get; set; }
    public string? LeaderName { get; set; }
    public string? Observations { get; set; }
    public bool RequiresClockifyCreation { get; set; }
    public ulong? ClockifyRecordId { get; set; }
    public bool IsActive { get; set; }
    public ulong? CreatedBy { get; set; }
    public ulong? UpdatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Expandidos para evitar lookups en el frontend
    public ProjectIntakeTypeRefDto? TypeRef { get; set; }
    public ProjectIntakeCategoryRefDto? CategoryRef { get; set; }
    public ProjectIntakeStatusRefDto? StatusRef { get; set; }
    public string? ClockifyProjectName { get; set; }
}

// Request DTOs

public class CreateProjectIntakeDto
{
    public string ProjectType { get; set; } = string.Empty;
    public string? SecondaryProjectNumber { get; set; }
    public DateOnly? RegistrationDate { get; set; }
    public ulong? ClientId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? CategoryCode { get; set; }
    public string? ProjectStatusCode { get; set; }
    public DateOnly? BusinessStatusDate { get; set; }
    public DateOnly? EstimatedEndDate { get; set; }
    public DateOnly? ActualEndDate { get; set; }
    public string? CommercialStatus { get; set; }
    public string? LeaderName { get; set; }
    public string? Observations { get; set; }
    public bool RequiresClockifyCreation { get; set; } = false;
}

public class UpdateProjectIntakeDto
{
    public string? SecondaryProjectNumber { get; set; }
    public DateOnly? RegistrationDate { get; set; }
    public ulong? ClientId { get; set; }
    public string? ProjectName { get; set; }
    public string? CategoryCode { get; set; }
    public string? ProjectStatusCode { get; set; }
    public DateOnly? BusinessStatusDate { get; set; }
    public DateOnly? EstimatedEndDate { get; set; }
    public DateOnly? ActualEndDate { get; set; }
    public string? CommercialStatus { get; set; }
    public string? LeaderName { get; set; }
    public string? Observations { get; set; }
}
