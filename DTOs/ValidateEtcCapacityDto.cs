namespace bdt_evm_app.DTOs;

public class ValidateEtcCapacityDto
{
    public ulong ProjectId { get; set; }
    public List<EtcEntryDto> Entries { get; set; } = new();
}