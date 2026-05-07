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

        // Eliminar tokens anteriores con nombre "spa" igual que Laravel
        var oldTokens = await _db.PersonalAccessTokens
            .Where(t => t.TokenableId == user.Id && t.Name == "spa")
            .ToListAsync();
        _db.PersonalAccessTokens.RemoveRange(oldTokens);

        // Generar token nuevo
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

        // El token que ve el cliente es: {id}|{rawToken} igual que Sanctum
        var plainTextToken = $"{accessToken.Id}|{rawToken}";

        return Ok(new LoginResponseDto
        {
            Token = plainTextToken,
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

    // POST api/auth/login-with-profile
    // Variante de login que valida que el usuario tenga un perfil asignado y que ese
    // perfil tenga al menos un permiso en profile_permissions (RBAC normalizado, RF-03).
    // Devuelve el usuario con datos del perfil + permisos como lista de "module:<code>"
    // (mismo shape que devolvía ProfileCatalog.GetPermissions, sin breaking changes para el front).
    [HttpPost("login-with-profile")]
    public async Task<IActionResult> LoginWithProfile([FromBody] LoginRequestDto dto)
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

        if (user.ProfileId == null || user.Profile == null)
            return UnprocessableEntity(new { message = "El usuario no tiene un perfil asignado" });

        var hasAnyPermission = await _db.ProfilePermissions
            .AnyAsync(pp => pp.ProfileId == user.ProfileId);
        if (!hasAnyPermission)
            return StatusCode(403, new { message = "El perfil del usuario no está autorizado para acceder al sistema" });

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

        var moduleCodes = await _db.ProfilePermissions
            .Where(pp => pp.ProfileId == user.ProfileId)
            .Select(pp => pp.Permission!.Module!.Code)
            .Distinct()
            .ToListAsync();
        var permissions = moduleCodes.Select(c => "module:" + c).ToList();

        return Ok(new LoginWithProfileResponseDto
        {
            Token = plainTextToken,
            User = new AuthUserWithProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Active = user.Active,
                Profile = new AuthProfileDto
                {
                    Id = user.Profile.Id,
                    Name = user.Profile.Name,
                    Code = user.Profile.Code,
                    Description = user.Profile.Description
                },
                Permissions = permissions
            }
        });
    }

    // POST api/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
            return Unauthorized(new { message = "Token no provisto" });

        var plainTextToken = authHeader.Substring(7);
        var parts = plainTextToken.Split('|');
        if (parts.Length != 2 || !ulong.TryParse(parts[0], out var tokenId))
            return Unauthorized(new { message = "Token inválido" });

        var token = await _db.PersonalAccessTokens.FindAsync(tokenId);
        if (token != null)
        {
            _db.PersonalAccessTokens.Remove(token);
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = "Sesión cerrada" });
    }

    // GET api/auth/me
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
            return Unauthorized(new { message = "Token no provisto" });

        var plainTextToken = authHeader.Substring(7);
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
}
