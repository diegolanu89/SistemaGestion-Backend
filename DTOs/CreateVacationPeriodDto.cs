namespace bdt_evm_app.DTOs;

public class CreateVacationPeriodDto
{
    public List<VacationPeriodEntryDto> Entries { get; set; } = new();
}