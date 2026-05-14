using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
            return UnprocessableEntity(new { message = "Email y contraseña son requeridos" });

        var user = await _db.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            return Unauthorized(new { message = "Credenciales incorrectas" });

        if (!user.Active)
            return StatusCode(403, new { message = "Usuario inactivo" });

        var oldTokens = await _db.PersonalAccessTokens
            .Where(t => t.TokenableId == user.Id && t.Name == "spa")
            .ToListAsync();

        _db.PersonalAccessTokens.RemoveRange(oldTokens);

        var rawToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        var hashedToken = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(rawToken)
            )
        ).ToLower();

        var accessToken = new PersonalAccessToken
        {
            TokenableType = "App\\Models\\User",
            TokenableId = user.Id,
            Name = "spa",
            Token = hashedToken,
            Abilities = "[\"*\"]",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.PersonalAccessTokens.Add(accessToken);

        await _db.SaveChangesAsync();

        var plainTextToken = $"{accessToken.Id}|{rawToken}";

        Response.Cookies.Append(
            "auth_token",
            plainTextToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            }
        );

        return Ok(new LoginResponseDto
        {
            User = new AuthUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                ProfileId = user.ProfileId,
                ProfileName = user.Profile?.Name,
                ProfileCode = user.Profile?.Code
            }
        });
    }

    // POST api/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var plainTextToken = Request.Cookies["auth_token"];

        if (string.IsNullOrEmpty(plainTextToken))
            return Unauthorized(new { message = "Token no provisto" });

        var parts = plainTextToken.Split('|');

        if (parts.Length != 2 || !ulong.TryParse(parts[0], out var tokenId))
            return Unauthorized(new { message = "Token inválido" });

        var token = await _db.PersonalAccessTokens.FindAsync(tokenId);

        if (token != null)
        {
            _db.PersonalAccessTokens.Remove(token);

            await _db.SaveChangesAsync();
        }

        Response.Cookies.Delete("auth_token");

        return Ok(new { message = "Sesión cerrada" });
    }

    // GET api/auth/me
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var plainTextToken = Request.Cookies["auth_token"];

        if (string.IsNullOrEmpty(plainTextToken))
            return Unauthorized(new { message = "Token no provisto" });

        var parts = plainTextToken.Split('|');

        if (parts.Length != 2 || !ulong.TryParse(parts[0], out var tokenId))
            return Unauthorized(new { message = "Token inválido" });

        var rawToken = parts[1];

        var hashedToken = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(rawToken)
            )
        ).ToLower();

        var accessToken = await _db.PersonalAccessTokens
            .FirstOrDefaultAsync(t => t.Id == tokenId && t.Token == hashedToken);

        if (accessToken == null)
            return Unauthorized(new { message = "Token inválido" });

        var user = await _db.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == accessToken.TokenableId);

        if (user == null)
            return Unauthorized(new { message = "Usuario no encontrado" });

        return Ok(new AuthUserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            ProfileId = user.ProfileId,
            ProfileName = user.Profile?.Name,
            ProfileCode = user.Profile?.Code,
            CreatedAt = user.CreatedAt
        });
    }

    // GET api/auth/permissions
    [HttpGet("permissions")]
    public async Task<IActionResult> MyPermissions()
    {
        var plainTextToken = Request.Cookies["auth_token"];

        if (string.IsNullOrEmpty(plainTextToken))
            return Unauthorized(new { message = "Token no provisto" });

        var parts = plainTextToken.Split('|');

        if (parts.Length != 2 || !ulong.TryParse(parts[0], out var tokenId))
            return Unauthorized(new { message = "Token inválido" });

        var rawToken = parts[1];

        var hashedToken = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(rawToken)
            )
        ).ToLower();

        var accessToken = await _db.PersonalAccessTokens
            .FirstOrDefaultAsync(t => t.Id == tokenId && t.Token == hashedToken);

        if (accessToken == null)
            return Unauthorized(new { message = "Token inválido" });

        var user = await _db.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == accessToken.TokenableId);

        if (user == null || user.ProfileId == null)
            return Unauthorized(new { message = "Usuario inválido" });

        var items = await _db.ProfilePermissions
            .Where(pp => pp.ProfileId == user.ProfileId)
            .Include(pp => pp.Permission)
                .ThenInclude(p => p!.Module)
            .Include(pp => pp.Action)
            .OrderBy(pp => pp.Permission!.Code)
            .Select(pp => new ProfilePermissionItemDto
            {
                PermissionId = pp.PermissionId,
                PermissionCode = pp.Permission!.Code,
                ModuleCode = pp.Permission!.Module!.Code,
                Action = new ProfilePermissionActionDto
                {
                    Id = pp.Action!.Id,
                    Code = pp.Action!.Code,
                    Level = pp.Action!.Level
                }
            })
            .ToListAsync();

        return Ok(new ProfilePermissionsResponseDto
        {
            ProfileId = user.ProfileId.Value,
            Permissions = items
        });
    }
}