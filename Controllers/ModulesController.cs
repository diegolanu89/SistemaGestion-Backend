using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/app/modules")]
public class ModulesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ModulesController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/app/modules
    // GET api/app/modules?withScreens=true
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool withScreens = false)
    {
        if (!withScreens)
        {
            var modules = await _db.Modules
                .Where(m => m.Active)
                .OrderBy(m => m.Name)
                .Select(m => new ModuleDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Code = m.Code,
                    Description = m.Description,
                    Active = m.Active
                })
                .ToListAsync();

            return Ok(modules);
        }

        var modulesWithScreens = await _db.Modules
            .Where(m => m.Active)
            .OrderBy(m => m.Name)
            .Select(m => new ModuleWithPermissionsDto
            {
                Id = m.Id,
                Name = m.Name,
                Code = m.Code,
                Description = m.Description,
                Active = m.Active,
                Permissions = _db.Permissions
                    .Where(p => p.ModuleId == m.Id && p.Active)
                    .OrderBy(p => p.Code)
                    .Select(p => new ModulePermissionDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Code = p.Code,
                        Description = p.Description
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(modulesWithScreens);
    }

    // GET api/app/modules/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(ulong id)
    {
        var module = await _db.Modules.FindAsync(id);

        if (module == null)
            return NotFound(new { message = "Módulo no encontrado" });

        return Ok(new ModuleDto
        {
            Id = module.Id,
            Name = module.Name,
            Code = module.Code,
            Description = module.Description,
            Active = module.Active
        });
    }
}
