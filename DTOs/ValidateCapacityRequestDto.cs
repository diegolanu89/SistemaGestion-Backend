namespace bdt_evm_app.DTOs;

public class ValidateCapacityRequestDto
{
    public ulong? PotencialProjectId { get; set; }
    public List<AllocationEntryDto> Entries { get; set; } = new();
}