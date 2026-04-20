namespace bdt_evm_app.DTOs;

public class UpdateChangeRequestDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public decimal? BacHoursIncrement { get; set; }
    public decimal? BacCostIncrement { get; set; }
    public string? ApprovedBy { get; set; }
    public DateOnly? ApprovedDate { get; set; }
}