namespace bdt_evm_app.DTOs;

public class CreateDashboardFilterDto
{
    public string Name { get; set; } = string.Empty;
    public string? LeaderId { get; set; }
    public List<string>? MonthKeys { get; set; }
    public string? ProjectId { get; set; }
}