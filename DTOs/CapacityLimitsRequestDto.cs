namespace bdt_evm_app.DTOs;

public class CapacityLimitsRequestDto
{
    public List<string> UserNames { get; set; } = new();
    public List<string> MonthKeys { get; set; } = new();
    public ulong? PotencialProjectId { get; set; }
}
