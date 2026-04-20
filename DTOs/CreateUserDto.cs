namespace bdt_evm_app.DTOs;

public class CreateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordConfirmation { get; set; } = string.Empty;
    public ulong? ProfileId { get; set; }
    public bool Active { get; set; } = true;
}