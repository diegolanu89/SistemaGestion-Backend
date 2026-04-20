namespace bdt_evm_app.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public AuthUserDto User { get; set; } = new();
}