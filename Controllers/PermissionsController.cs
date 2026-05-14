using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/app/permissions")]
[RequirePermission("ADMIN_ACCESS")]
public class PermissionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PermissionsController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/app/permissions
    // GET api/app/permissions?moduleId=1
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ulong? moduleId = null)
    {
        var query = _db.Permissions
            .Include(p => p.Module)
            .Where(p => p.Active);

        if (moduleId.HasValue)
            query = query.Where(p => p.ModuleId == moduleId.Value);

        var permissions = await query
            .OrderBy(p => p.Code)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                ModuleId = p.ModuleId,
                ModuleCode = p.Module != null ? p.Module.Code : string.Empty,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                Active = p.Active
            })
            .ToListAsync();

        return Ok(permissions);
    }

    // GET api/app/permissions/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(ulong id)
    {
        var permission = await _db.Permissions
            .Include(p => p.Module)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (permission == null)
            return NotFound(new { message = "Permiso no encontrado" });

        return Ok(new PermissionDto
        {
            Id = permission.Id,
            ModuleId = permission.ModuleId,
            ModuleCode = permission.Module?.Code ?? string.Empty,
            Name = permission.Name,
            Code = permission.Code,
            Description = permission.Description,
            Active = permission.Active
        });
    }

    // PATCH api/app/permissions/{id}/module
    // Reasigna el módulo al que pertenece un permiso.
    // No permite crear/eliminar permisos ni cambiar code/name (eso es seed-managed).
    [HttpPatch("{id}/module")]
    public async Task<IActionResult> UpdateModule(ulong id, [FromBody] UpdatePermissionModuleDto dto)
    {
        var permission = await _db.Permissions.FirstOrDefaultAsync(p => p.Id == id);

        if (permission == null)
            return NotFound(new { message = "Permiso no encontrado" });

        var moduleExists = await _db.Modules.AnyAsync(m => m.Id == dto.ModuleId && m.Active);
        if (!moduleExists)
            return UnprocessableEntity(new { message = "Módulo inválido o inactivo" });

        permission.ModuleId = dto.ModuleId;
        permission.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _db.Entry(permission).Reference(p => p.Module).LoadAsync();

        return Ok(new PermissionDto
        {
            Id = permission.Id,
            ModuleId = permission.ModuleId,
            ModuleCode = permission.Module?.Code ?? string.Empty,
            Name = permission.Name,
            Code = permission.Code,
            Description = permission.Description,
            Active = permission.Active
        });
    }
}
