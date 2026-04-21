namespace bdt_evm_app.DTOs;

public class AllocationEntryDto
{
    public ulong? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string MonthKey { get; set; } = string.Empty;
    public string? MonthLabel { get; set; }
    public decimal Hours { get; set; }
}