namespace bdt_evm_app.DTOs;

public class ProjectDto
{
    public ulong Id { get; set; }
    public string ClockifyProjectId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public ulong? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDatePlanned { get; set; }
    public DateOnly? EndDateActual { get; set; }
    public decimal BacBaseHours { get; set; }
    public decimal BacBaseCost { get; set; }
    public decimal BacTotalHours { get; set; }
    public decimal BacTotalCost { get; set; }
    public decimal HourlyRate { get; set; }
    public string EtcCalculationMode { get; set; } = string.Empty;
    public decimal EtcTotalHours { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public object? Filter { get; set; }
}

public class UpdateBacDto
{
    public decimal? BacBaseHours { get; set; }
    public decimal? BacBaseCost { get; set; }
    public string? EtcCalculationMode { get; set; }
}
