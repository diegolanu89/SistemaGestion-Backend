namespace bdt_evm_app.DTOs;

public class VacationPeriodEntryDto
{
    public ulong UserId { get; set; }
    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public string? Notes { get; set; }
}