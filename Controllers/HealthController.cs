using Microsoft.AspNetCore.Mvc;
using bdt_evm_app.Data;
using Microsoft.EntityFrameworkCore;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _db;

    public HealthController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/health
    [HttpGet]
    public async Task<IActionResult> Check()
    {
        var dbStatus = "ok";
        try
        {
            await _db.Database.ExecuteSqlRawAsync("SELECT 1");
        }
        catch
        {
            dbStatus = "error";
        }

        var statusCode = dbStatus == "ok" ? 200 : 503;
        return StatusCode(statusCode, new
        {
            status = "ok",
            timestamp = DateTime.UtcNow.ToString("o"),
            database = dbStatus
        });
    }
}