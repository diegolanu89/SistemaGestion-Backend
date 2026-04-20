namespace bdt_evm_app.DTOs;

public class AuthUserDto
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ulong? ProfileId { get; set; }
    public string? ProfileName { get; set; }
    public string? ProfileCode { get; set; }
}
