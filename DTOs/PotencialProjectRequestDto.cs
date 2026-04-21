namespace bdt_evm_app.DTOs;

public class PotencialProjectRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public ulong PotencialClientId { get; set; }
}
