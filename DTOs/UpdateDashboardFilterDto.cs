namespace bdt_evm_app.DTOs;

public class UpdateDashboardFilterDto
{
    public string? Name { get; set; }
    public string? LeaderId { get; set; }
    public List<string>? MonthKeys { get; set; }
    public string? ProjectId { get; set; }
}