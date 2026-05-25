namespace bdt_evm_app.DTOs;

public class UserDto
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ulong? ProfileId { get; set; }
    public string? ProfileName { get; set; }
    public string? ProfileCode { get; set; }
    public bool Active { get; set; }
}

public class CreateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordConfirmation { get; set; } = string.Empty;
    public ulong ProfileId { get; set; }
    public bool Active { get; set; } = true;
}

public class UpdateUserDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? PasswordConfirmation { get; set; }
    public ulong? ProfileId { get; set; }
    public bool? Active { get; set; }
}
