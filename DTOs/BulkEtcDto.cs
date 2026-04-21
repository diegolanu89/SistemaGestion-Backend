namespace bdt_evm_app.DTOs;

public class BulkEtcDto
{
    public ulong ProjectId { get; set; }
    public List<EtcEntryDto> Entries { get; set; } = new();
}
