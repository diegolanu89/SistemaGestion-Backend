using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/app/users")]
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
}