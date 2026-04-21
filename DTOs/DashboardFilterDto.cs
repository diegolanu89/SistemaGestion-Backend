namespace bdt_evm_app.DTOs;

public class DashboardFilterDto
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LeaderId { get; set; }
    public string? MonthKeys { get; set; }
    public string? ProjectId { get; set; }
}
