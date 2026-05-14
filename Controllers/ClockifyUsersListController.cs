using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/users")]
[RequirePermission("ADMIN_ACCESS")]
public class ClockifyUsersListController : ControllerBase
{
    private readonly AppDbContext _db;

    public ClockifyUsersListController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/users
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.ClockifyUsers
            .Where(u => u.Active)
            .OrderBy(u => u.Name)
            .Select(u => new
            {
                id = u.Id,
                name = u.Name,
                email = u.Email
            })
            .ToListAsync();

        return Ok(users);
    }
}