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

    // DEPRECATED:
    // Este endpoint queda obsoleto por motivos de seguridad.
    // El frontend ya NO debe consultar permisos por profileId enviado por cliente.
    // Utilizar:
    // GET /api/auth/permissions
    // que resuelve los permisos desde el usuario autenticado (cookie HttpOnly + middleware).
    [Obsolete(
        "Deprecated for security reasons. Use GET /api/auth/permissions instead."
    )]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("{id}/permissions")]
    public async Task<IActionResult> GetPermissions(ulong id)
    {
        return StatusCode(410, new
        {
            message = "Endpoint deprecated. Use /api/auth/permissions"
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

        return await GetPermissions(id);
    }
}
