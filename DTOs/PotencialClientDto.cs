namespace bdt_evm_app.DTOs;

public class PotencialClientDto
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}