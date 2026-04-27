using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/app/profiles")]
public class ProfilesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProfilesController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/app/profiles
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var profiles = await _db.Profiles
            .OrderBy(p => p.Name)
            .Select(p => new ProfileDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description
            })
            .ToListAsync();

        return Ok(profiles);
    }

    // GET api/app/profiles/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(ulong id)
    {
        var profile = await _db.Profiles.FindAsync(id);

        if (profile == null)
            return NotFound(new { message = "Perfil no encontrado" });

        return Ok(new ProfileDto
        {
            Id = profile.Id,
            Name = profile.Name,
            Code = profile.Code,
            Description = profile.Description
        });
    }

    // POST api/app/profiles
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProfileDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return UnprocessableEntity(new { message = "El nombre es requerido" });

        if (string.IsNullOrWhiteSpace(dto.Code))
            return UnprocessableEntity(new { message = "El código es requerido" });

        var codeExists = await _db.Profiles.AnyAsync(p => p.Code == dto.Code);
        if (codeExists)
            return UnprocessableEntity(new { message = "El código ya está en uso" });

        var profile = new Profile
        {
            Name = dto.Name.Trim(),
            Code = dto.Code.Trim(),
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Profiles.Add(profile);
        await _db.SaveChangesAsync();

        return StatusCode(201, new ProfileDto
        {
            Id = profile.Id,
            Name = profile.Name,
            Code = profile.Code,
            Description = profile.Description
        });
    }

    // PUT api/app/profiles/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateProfileDto dto)
    {
        var profile = await _db.Profiles.FindAsync(id);

        if (profile == null)
            return NotFound(new { message = "Perfil no encontrado" });

        if (dto.Name != null)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return UnprocessableEntity(new { message = "El nombre no puede estar vacío" });
            profile.Name = dto.Name.Trim();
        }

        if (dto.Code != null)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return UnprocessableEntity(new { message = "El código no puede estar vacío" });

            var codeExists = await _db.Profiles.AnyAsync(p => p.Code == dto.Code && p.Id != id);
            if (codeExists)
                return UnprocessableEntity(new { message = "El código ya está en uso" });

            profile.Code = dto.Code.Trim();
        }

        if (dto.Description != null)
            profile.Description = dto.Description.Trim();

        profile.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new ProfileDto
        {
            Id = profile.Id,
            Name = profile.Name,
            Code = profile.Code,
            Description = profile.Description
        });
    }

    // DELETE api/app/profiles/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(ulong id)
    {
        var profile = await _db.Profiles.FindAsync(id);

        if (profile == null)
            return NotFound(new { message = "Perfil no encontrado" });

        var hasUsers = await _db.Users.AnyAsync(u => u.ProfileId == id);
        if (hasUsers)
            return UnprocessableEntity(new { message = "No se puede eliminar el perfil porque tiene usuarios asignados" });

        _db.Profiles.Remove(profile);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Perfil eliminado" });
    }
}
