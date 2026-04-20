using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;

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
}