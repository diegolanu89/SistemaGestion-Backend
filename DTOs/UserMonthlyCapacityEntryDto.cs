namespace bdt_evm_app.DTOs;

public class UserMonthlyCapacityEntryDto
{
    public string MonthKey { get; set; } = string.Empty;
    public string? MonthLabel { get; set; }
    public decimal Hours { get; set; }
}
