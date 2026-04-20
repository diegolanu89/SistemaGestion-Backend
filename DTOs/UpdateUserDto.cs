namespace bdt_evm_app.DTOs;

public class UpdateUserDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? PasswordConfirmation { get; set; }
    public ulong? ProfileId { get; set; }
    public bool? Active { get; set; }
}