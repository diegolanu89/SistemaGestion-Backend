namespace bdt_evm_app.DTOs;

public class AuthUserDto
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ulong? ProfileId { get; set; }
    public string? ProfileName { get; set; }
    public string? ProfileCode { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public AuthUserDto User { get; set; } = new();
}

public class AuthProfileDto
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class AuthUserWithProfileDto
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Active { get; set; }
    public AuthProfileDto Profile { get; set; } = new();
    public IReadOnlyList<string> Permissions { get; set; } = Array.Empty<string>();
}

public class LoginWithProfileResponseDto
{
    public string Token { get; set; } = string.Empty;
    public AuthUserWithProfileDto User { get; set; } = new();
}
