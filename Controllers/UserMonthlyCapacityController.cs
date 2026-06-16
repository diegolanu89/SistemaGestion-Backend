using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;
using bdt_evm_app.Helpers;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/timesheet-users/{userId}/capacities")]
[RequirePermission("SETTINGS_ACCESS")]
public class UserMonthlyCapacityController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<UserMonthlyCapacityController> _logger;

    public UserMonthlyCapacityController(AppDbContext db, ILogger<UserMonthlyCapacityController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET api/timesheet-users/{userId}/capacities
    [HttpGet]
    public async Task<IActionResult> GetAll(ulong userId)
    {
        var user = await _db.TimesheetUsers.FindAsync(userId);
        if (user == null)
            return NotFound(new { success = false, message = "Usuario no encontrado" });

        var records = await _db.UserMonthlyCapacities
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.MonthKey)
            .ToListAsync();

        return Ok(new { success = true, data = records });
    }

    // POST api/timesheet-users/{userId}/capacities
    [HttpPost]
    public async Task<IActionResult> Store(ulong userId, [FromBody] CreateUserMonthlyCapacityDto dto)
    {
        var user = await _db.TimesheetUsers.FindAsync(userId);
        if (user == null)
            return NotFound(new { success = false, message = "Usuario no encontrado" });

        if (dto.Entries == null || !dto.Entries.Any())
            return UnprocessableEntity(new { success = false, message = "entries es requerido y debe tener al menos un elemento" });

        foreach (var entry in dto.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.MonthKey))
                return UnprocessableEntity(new { success = false, message = "month_key es requerido en cada entrada" });

            if (entry.Hours < 0)
                return UnprocessableEntity(new { success = false, message = "hours no puede ser negativo" });

            if (entry.Hours <= 0)
            {
                // Si horas es 0 o negativo, eliminar el registro si existe
                var toDelete = await _db.UserMonthlyCapacities
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.MonthKey == entry.MonthKey);

                if (toDelete != null)
                    _db.UserMonthlyCapacities.Remove(toDelete);

                continue;
            }

            var existing = await _db.UserMonthlyCapacities
                .FirstOrDefaultAsync(c => c.UserId == userId && c.MonthKey == entry.MonthKey);

            if (existing != null)
            {
                existing.MonthLabel = MonthHelper.GetMonthLabel(entry.MonthKey);
                existing.Hours = entry.Hours;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.UserMonthlyCapacities.Add(new UserMonthlyCapacity
                {
                    UserId = userId,
                    MonthKey = entry.MonthKey,
                    MonthLabel = MonthHelper.GetMonthLabel(entry.MonthKey),
                    Hours = entry.Hours,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _db.SaveChangesAsync();

        var records = await _db.UserMonthlyCapacities
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.MonthKey)
            .ToListAsync();

        return Ok(new { success = true, data = records });
    }
}