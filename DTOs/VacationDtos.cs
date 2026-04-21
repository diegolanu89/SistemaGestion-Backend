namespace bdt_evm_app.DTOs;

public class VacationPeriodDto
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public string? UserName { get; set; }
    public string DateFrom { get; set; } = string.Empty;
    public string DateTo { get; set; } = string.Empty;
    public int TotalDays { get; set; }
    public string? Notes { get; set; }
}

public class VacationPeriodEntryDto
{
    public ulong UserId { get; set; }
    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public string? Notes { get; set; }
}

public class CreateVacationPeriodDto
{
    public List<VacationPeriodEntryDto> Entries { get; set; } = new();
}
