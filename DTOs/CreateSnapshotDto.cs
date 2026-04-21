namespace bdt_evm_app.DTOs;

public class CreateSnapshotDto
{
    public List<EtcEntryDto> Entries { get; set; } = new();
}
