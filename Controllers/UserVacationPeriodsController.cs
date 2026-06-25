using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/user-vacation-periods")]
[RequirePermission("SETTINGS_ACCESS")]
public class UserVacationPeriodsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<UserVacationPeriodsController> _logger;

    public UserVacationPeriodsController(AppDbContext db, ILogger<UserVacationPeriodsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET api/user-vacation-periods
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var query = _db.UserVacationPeriods
            .Include(p => p.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.User != null && p.User.Name.Contains(search));

        var periods = await query
            .OrderByDescending(p => p.DateFrom)
            .ToListAsync();

        return Ok(periods.Select(MapToDto));
    }

    // POST api/user-vacation-periods
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVacationPeriodDto dto)
    {
        if (dto.Entries == null || !dto.Entries.Any())
            return UnprocessableEntity(new { message = "entries es requerido y debe tener al menos un elemento" });

        var created = new List<VacationPeriodDto>();

        foreach (var entry in dto.Entries)
        {
            var userExists = await _db.TimesheetUsers.AnyAsync(u => u.Id == entry.UserId);
            if (!userExists)
                return UnprocessableEntity(new { message = $"Usuario {entry.UserId} no encontrado" });

            if (entry.DateTo < entry.DateFrom)
                return UnprocessableEntity(new { message = "La fecha hasta debe ser mayor o igual que la fecha desde" });

            var totalDays = entry.DateTo.DayNumber - entry.DateFrom.DayNumber + 1;

            var period = new UserVacationPeriod
            {
                UserId = entry.UserId,
                DateFrom = entry.DateFrom,
                DateTo = entry.DateTo,
                TotalDays = totalDays,
                Notes = entry.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.UserVacationPeriods.Add(period);
            await _db.SaveChangesAsync();

            await _db.Entry(period).Reference(p => p.User).LoadAsync();
            created.Add(MapToDto(period));
        }

        return StatusCode(201, new { message = "Registros creados", data = created });
    }

    // DELETE api/user-vacation-periods/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(ulong id)
    {
        var period = await _db.UserVacationPeriods.FindAsync(id);
        if (period == null)
            return NotFound(new { message = "Registro no encontrado" });

        _db.UserVacationPeriods.Remove(period);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Registro eliminado" });
    }

    private static VacationPeriodDto MapToDto(UserVacationPeriod p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        UserName = p.User?.Name,
        DateFrom = p.DateFrom.ToString("yyyy-MM-dd"),
        DateTo = p.DateTo.ToString("yyyy-MM-dd"),
        TotalDays = p.TotalDays,
        Notes = p.Notes
    };
}