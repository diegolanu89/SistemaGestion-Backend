namespace bdt_evm_app.DTOs;

public class CreateUserMonthlyCapacityDto
{
    public List<UserMonthlyCapacityEntryDto> Entries { get; set; } = new();
}