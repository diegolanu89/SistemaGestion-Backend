namespace bdt_evm_app.DTOs;

public class CreateChangeRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RequestedBy { get; set; }
    public DateOnly RequestedDate { get; set; }
    public string Status { get; set; } = "propuesto";
    public decimal? BacHoursIncrement { get; set; }
    public decimal? BacCostIncrement { get; set; }
    public string? ApprovedBy { get; set; }
    public DateOnly? ApprovedDate { get; set; }
}
