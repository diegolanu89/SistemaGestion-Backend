using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.Helpers;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/projects")]
[RequirePermission("DASHBOARD_HOURS_ACCESS")]
public class UserHoursController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<UserHoursController> _logger;

    public UserHoursController(AppDbContext db, ILogger<UserHoursController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET api/projects/{id}/user-hours
    [HttpGet("{projectId}/user-hours")]
    public async Task<IActionResult> GetUserHours(
        ulong projectId,
        [FromQuery] string? from,
        [FromQuery] string? to)
    {
        var project = await _db.TimesheetProjects.FindAsync(projectId);
        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        var query = _db.TimesheetTimeEntries
            .Where(t => t.ProjectId == projectId);

        if (!string.IsNullOrEmpty(from) && DateOnly.TryParse(from, out var fromDate))
            query = query.Where(t => DateOnly.FromDateTime(t.StartTime) >= fromDate);

        if (!string.IsNullOrEmpty(to) && DateOnly.TryParse(to, out var toDate))
            query = query.Where(t => DateOnly.FromDateTime(t.StartTime) <= toDate);

        var results = await query
            .GroupBy(t => new
            {
                MonthKey = t.StartTime.ToString().Substring(0, 7),
                t.UserId
            })
            .Select(g => new
            {
                MonthKey = g.Key.MonthKey,
                UserId = g.Key.UserId,
                TotalHours = g.Sum(t => t.DurationHours)
            })
            .OrderBy(r => r.MonthKey)
            .ThenBy(r => r.UserId)
            .ToListAsync();

        var userIds = results
            .Where(r => r.UserId.HasValue)
            .Select(r => r.UserId!.Value)
            .Distinct()
            .ToList();

        var users = await _db.TimesheetUsers
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name);

        var groupedByMonth = new Dictionary<string, object>();

        foreach (var result in results)
        {
            var monthKey = result.MonthKey;
            var userName = result.UserId.HasValue && users.TryGetValue(result.UserId.Value, out var name)
                ? name : "Usuario Desconocido";

            if (!groupedByMonth.ContainsKey(monthKey))
            {
                groupedByMonth[monthKey] = new
                {
                    month_key = monthKey,
                    month_label = MonthHelper.GetMonthLabel(monthKey),
                    users = new List<object>()
                };
            }

            ((List<object>)((dynamic)groupedByMonth[monthKey]).users).Add(new
            {
                full_name = userName,
                hours = result.TotalHours
            });
        }

        return Ok(groupedByMonth.Values.ToList());
    }

}