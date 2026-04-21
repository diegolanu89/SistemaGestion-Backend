namespace bdt_evm_app.DTOs;

public class PotencialProjectDto
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public ulong ClientId { get; set; }
    public string? ClientName { get; set; }
}

public class PotencialProjectRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public ulong PotencialClientId { get; set; }
}
