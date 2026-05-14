using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/app/actions")]
[RequirePermission("ADMIN_ACCESS")]
public class ActionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ActionsController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/app/actions
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var actions = await _db.Actions
            .Where(a => a.Active)
            .OrderBy(a => a.Level)
            .Select(a => new ActionDto
            {
                Id = a.Id,
                Code = a.Code,
                Name = a.Name,
                Description = a.Description,
                Level = a.Level,
                Active = a.Active
            })
            .ToListAsync();

        return Ok(actions);
    }
}
