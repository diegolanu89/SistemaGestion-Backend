using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/app/users")]
[RequirePermission("ADMIN_ACCESS")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/app/users
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users
            .Include(u => u.Profile)
            .OrderBy(u => u.Name)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                ProfileId = u.ProfileId,
                ProfileName = u.Profile != null ? u.Profile.Name : null,
                ProfileCode = u.Profile != null ? u.Profile.Code : null,
                Active = u.Active
            })
            .ToListAsync();

        return Ok(users);
    }

    // GET api/app/users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(ulong id)
    {
        var user = await _db.Users
            .Include(u => u.Profile)
            .Where(u => u.Id == id)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                ProfileId = u.ProfileId,
                ProfileName = u.Profile != null ? u.Profile.Name : null,
                ProfileCode = u.Profile != null ? u.Profile.Code : null,
                Active = u.Active
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new { message = "No encontrado" });

        return Ok(user);
    }

    // POST api/app/users
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return UnprocessableEntity(new { message = "El nombre es requerido" });

        if (string.IsNullOrWhiteSpace(dto.Email) || !IsValidEmail(dto.Email))
            return UnprocessableEntity(new { message = "El correo no es válido" });

        if (string.IsNullOrWhiteSpace(dto.Password))
            return UnprocessableEntity(new { message = "La contraseña es requerida" });

        if (dto.Password.Length < 8)
            return UnprocessableEntity(new { message = "La contraseña debe tener al menos 8 caracteres" });

        if (dto.Password != dto.PasswordConfirmation)
            return UnprocessableEntity(new { message = "Las contraseñas no coinciden" });

        var profileExists = await _db.Profiles.AnyAsync(p => p.Id == dto.ProfileId);
        if (!profileExists)
            return UnprocessableEntity(new { message = "El perfil especificado no existe" });

        var emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
            return UnprocessableEntity(new { message = "El correo ya está en uso" });

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            ProfileId = dto.ProfileId,
            Active = dto.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await _db.Entry(user).Reference(u => u.Profile).LoadAsync();

        var result = new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            ProfileId = user.ProfileId,
            ProfileName = user.Profile?.Name,
            ProfileCode = user.Profile?.Code,
            Active = user.Active
        };

        return StatusCode(201, result);
    }

    // PUT api/app/users/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateUserDto dto)
    {
        var user = await _db.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound(new { message = "No encontrado" });

        if (dto.Name != null && string.IsNullOrWhiteSpace(dto.Name))
            return UnprocessableEntity(new { message = "El nombre no puede estar vacío" });

        if (dto.Email != null && !IsValidEmail(dto.Email))
            return UnprocessableEntity(new { message = "El correo no es válido" });

        if (dto.Password != null)
        {
            if (dto.Password.Length < 8)
                return UnprocessableEntity(new { message = "La contraseña debe tener al menos 8 caracteres" });

            if (dto.Password != dto.PasswordConfirmation)
                return UnprocessableEntity(new { message = "Las contraseñas no coinciden" });

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        if (dto.Name != null) user.Name = dto.Name;
        if (dto.Email != null)
        {
            var emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id);
            if (emailExists)
                return UnprocessableEntity(new { message = "El correo ya está en uso" });
            user.Email = dto.Email;
        }
        if (dto.ProfileId != null) user.ProfileId = dto.ProfileId;
        if (dto.Active != null) user.Active = dto.Active.Value;

        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _db.Entry(user).Reference(u => u.Profile).LoadAsync();

        return Ok(new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            ProfileId = user.ProfileId,
            ProfileName = user.Profile?.Name,
            ProfileCode = user.Profile?.Code,
            Active = user.Active
        });
    }

    // DELETE api/app/users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(ulong id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound(new { message = "No encontrado" });

        user.Active = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Usuario desactivado" });
    }

    private static bool IsValidEmail(string email)
    {
        try { _ = new System.Net.Mail.MailAddress(email); return true; }
        catch { return false; }
    }
}