using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/app/profiles")]
[RequirePermission("ADMIN_ACCESS")]
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

    // GET api/app/profiles/{id}/permissions
    [HttpGet("{id}/permissions")]
    public async Task<IActionResult> GetPermissions(ulong id)
    {
        var profileExists = await _db.Profiles.AnyAsync(p => p.Id == id);

        if (!profileExists)
            return NotFound(new { message = "Perfil no encontrado" });

        var items = await _db.ProfilePermissions
            .Where(pp => pp.ProfileId == id)
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
            ProfileId = id,
            Permissions = items
        });
    }

    // PUT api/app/profiles/{id}/permissions
    [HttpPut("{id}/permissions")]
    public async Task<IActionResult> SyncPermissions(ulong id, [FromBody] SyncProfilePermissionsDto dto)
    {
        if (dto?.Permissions == null)
            return UnprocessableEntity(new { message = "El campo permissions es requerido (puede ser una lista vacía)" });

        var profileExists = await _db.Profiles.AnyAsync(p => p.Id == id);
        if (!profileExists)
            return NotFound(new { message = "Perfil no encontrado" });

        var duplicatePermissionIds = dto.Permissions
            .GroupBy(x => x.PermissionId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicatePermissionIds.Count > 0)
            return UnprocessableEntity(new
            {
                message = "Hay permission_id duplicados en la solicitud",
                duplicates = duplicatePermissionIds
            });

        var requestedPermissionIds = dto.Permissions.Select(x => x.PermissionId).ToList();
        var requestedActionIds = dto.Permissions.Select(x => x.ActionId).Distinct().ToList();

        var validPermissionIds = await _db.Permissions
            .Where(p => requestedPermissionIds.Contains(p.Id) && p.Active)
            .Select(p => p.Id)
            .ToListAsync();

        var invalidPermissionIds = requestedPermissionIds.Except(validPermissionIds).ToList();
        if (invalidPermissionIds.Count > 0)
            return UnprocessableEntity(new
            {
                message = "Hay permission_id inválidos o inactivos",
                invalid_permission_ids = invalidPermissionIds
            });

        var validActionIds = await _db.Actions
            .Where(a => requestedActionIds.Contains(a.Id) && a.Active)
            .Select(a => a.Id)
            .ToListAsync();

        var invalidActionIds = requestedActionIds.Except(validActionIds).ToList();
        if (invalidActionIds.Count > 0)
            return UnprocessableEntity(new
            {
                message = "Hay action_id inválidos o inactivos",
                invalid_action_ids = invalidActionIds
            });

        var existing = await _db.ProfilePermissions
            .Where(pp => pp.ProfileId == id)
            .ToListAsync();

        _db.ProfilePermissions.RemoveRange(existing);

        var now = DateTime.UtcNow;
        var newRows = dto.Permissions.Select(x => new ProfilePermission
        {
            ProfileId = id,
            PermissionId = x.PermissionId,
            ActionId = x.ActionId,
            CreatedAt = now,
            UpdatedAt = now
        }).ToList();

        _db.ProfilePermissions.AddRange(newRows);
        await _db.SaveChangesAsync();

        var items = await _db.ProfilePermissions
            .Where(pp => pp.ProfileId == id)
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
            ProfileId = id,
            Permissions = items
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
            return UnprocessableEntity(new { message = "Ya existe un perfil con ese código" });

        var profile = new Profile
        {
            Name = dto.Name,
            Code = dto.Code.ToUpper(),
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Profiles.Add(profile);

        await _db.SaveChangesAsync();

        return Ok(new ProfileDto
        {
            Id = profile.Id,
            Name = profile.Name,
            Code = profile.Code,
            Description = profile.Description
        });
    }

    // PUT api/app/profiles/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        ulong id,
        [FromBody] UpdateProfileDto dto)
    {
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.Id == id);

        if (profile == null)
            return NotFound(new { message = "Perfil no encontrado" });

        if (profile.Code == "ADMIN")
            return UnprocessableEntity(new
            {
                message = "El perfil ADMIN está protegido"
            });

        if (!string.IsNullOrWhiteSpace(dto.Name))
            profile.Name = dto.Name;

        if (dto.Description != null)
            profile.Description = dto.Description;

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
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.Id == id);

        if (profile == null)
            return NotFound(new { message = "Perfil no encontrado" });

        if (profile.Code == "ADMIN")
            return UnprocessableEntity(new
            {
                message = "El perfil ADMIN está protegido"
            });

        var usersUsingProfile = await _db.Users
            .AnyAsync(u => u.ProfileId == id);

        if (usersUsingProfile)
            return UnprocessableEntity(new
            {
                message = "Existen usuarios asociados al perfil"
            });

        var permissions = await _db.ProfilePermissions
            .Where(x => x.ProfileId == id)
            .ToListAsync();

        _db.ProfilePermissions.RemoveRange(permissions);

        _db.Profiles.Remove(profile);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Perfil eliminado"
        });
    }
}
