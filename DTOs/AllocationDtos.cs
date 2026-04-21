namespace bdt_evm_app.DTOs;

public class AllocationEntryDto
{
    public ulong? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string MonthKey { get; set; } = string.Empty;
    public string? MonthLabel { get; set; }
    public decimal Hours { get; set; }
}

public class StoreAllocationsDto
{
    public List<AllocationEntryDto> Entries { get; set; } = new();
}

public class CapacityLimitsRequestDto
{
    public List<string> UserNames { get; set; } = new();
    public List<string> MonthKeys { get; set; } = new();
    public ulong? PotencialProjectId { get; set; }
}

public class ValidateCapacityRequestDto
{
    public ulong? PotencialProjectId { get; set; }
    public List<AllocationEntryDto> Entries { get; set; } = new();
}
