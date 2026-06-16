using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/time-entries")]
[RequirePermission("ETC_ACCESS")]
public class TimeEntryController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<TimeEntryController> _logger;

    public TimeEntryController(AppDbContext db, ILogger<TimeEntryController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET api/time-entries/last-date
    [HttpGet("last-date")]
    public async Task<IActionResult> GetLastDate()
    {
        try
        {
            var lastDate = await _db.TimesheetTimeEntries
                .MaxAsync(t => (DateTime?)t.StartTime);

            return Ok(new
            {
                last_date = lastDate.HasValue
                    ? DateOnly.FromDateTime(lastDate.Value).ToString("yyyy-MM-dd")
                    : (string?)null
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener la última fecha");
            return StatusCode(500, new { error = "Error al obtener la última fecha", message = e.Message });
        }
    }
}