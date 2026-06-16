using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;
using bdt_evm_app.Helpers;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/potencial-projects")]
[RequirePermission("ESTIMATED_PROJECTS_ACCESS")]
public class PotencialProjectsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<PotencialProjectsController> _logger;

    public PotencialProjectsController(AppDbContext db, ILogger<PotencialProjectsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET api/potencial-projects
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var projects = await _db.PotencialProjects
            .Include(p => p.PotencialClient)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return Ok(projects.Select(MapToDto));
    }

    // GET api/potencial-projects/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(ulong id)
    {
        var project = await _db.PotencialProjects
            .Include(p => p.PotencialClient)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            return NotFound(new { message = "Proyecto potencial no encontrado" });

        return Ok(MapToDto(project));
    }

    // POST api/potencial-projects
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PotencialProjectRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return UnprocessableEntity(new { message = "El nombre es requerido" });

        var clientExists = await _db.PotencialClients.AnyAsync(c => c.Id == dto.PotencialClientId);
        if (!clientExists)
            return UnprocessableEntity(new { message = "Cliente potencial no encontrado" });

        var project = new PotencialProject
        {
            Name = dto.Name,
            Code = dto.Code,
            PotencialClientId = dto.PotencialClientId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.PotencialProjects.Add(project);
        await _db.SaveChangesAsync();
        await _db.Entry(project).Reference(p => p.PotencialClient).LoadAsync();

        return StatusCode(201, MapToDto(project));
    }

    // PUT api/potencial-projects/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(ulong id, [FromBody] PotencialProjectRequestDto dto)
    {
        var project = await _db.PotencialProjects.FindAsync(id);
        if (project == null)
            return NotFound(new { message = "Proyecto potencial no encontrado" });

        if (string.IsNullOrWhiteSpace(dto.Name))
            return UnprocessableEntity(new { message = "El nombre es requerido" });

        var clientExists = await _db.PotencialClients.AnyAsync(c => c.Id == dto.PotencialClientId);
        if (!clientExists)
            return UnprocessableEntity(new { message = "Cliente potencial no encontrado" });

        project.Name = dto.Name;
        project.Code = dto.Code;
        project.PotencialClientId = dto.PotencialClientId;
        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _db.Entry(project).Reference(p => p.PotencialClient).LoadAsync();

        return Ok(MapToDto(project));
    }

    // DELETE api/potencial-projects/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(ulong id)
    {
        var project = await _db.PotencialProjects.FindAsync(id);
        if (project == null)
            return NotFound(new { message = "Proyecto potencial no encontrado" });

        _db.PotencialProjects.Remove(project);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Proyecto potencial eliminado" });
    }

    // GET api/potencial-projects/{id}/allocations
    [HttpGet("{id}/allocations")]
    public async Task<IActionResult> GetAllocations(ulong id)
    {
        var project = await _db.PotencialProjects.FindAsync(id);
        if (project == null)
            return NotFound(new { message = "Proyecto potencial no encontrado" });

        var allocations = await _db.PotencialProjectAllocations
            .Include(a => a.User)
            .Where(a => a.PotencialProjectId == id)
            .OrderBy(a => a.UserName)
            .ThenBy(a => a.MonthKey)
            .ToListAsync();

        return Ok(new { allocations = allocations.Select(a => MapAllocationToDto(a)) });
    }

    // POST api/potencial-projects/{id}/allocations
    [HttpPost("{id}/allocations")]
    public async Task<IActionResult> StoreAllocations(ulong id, [FromBody] StoreAllocationsDto dto)
    {
        var project = await _db.PotencialProjects.FindAsync(id);
        if (project == null)
            return NotFound(new { message = "Proyecto potencial no encontrado" });

        if (dto.Entries == null || !dto.Entries.Any())
            return UnprocessableEntity(new { message = "entries es requerido" });

        var existing = await _db.PotencialProjectAllocations
            .Where(a => a.PotencialProjectId == id)
            .ToListAsync();
        _db.PotencialProjectAllocations.RemoveRange(existing);

        foreach (var entry in dto.Entries)
        {
            if (entry.Hours <= 0) continue;

            var userId = entry.UserId;
            if (!userId.HasValue && !string.IsNullOrEmpty(entry.UserName))
            {
                var user = await _db.ClockifyUsers
                    .FirstOrDefaultAsync(u => u.Name.Trim() == entry.UserName.Trim());
                userId = user?.Id;
            }

            _db.PotencialProjectAllocations.Add(new PotencialProjectAllocation
            {
                PotencialProjectId = id,
                MonthKey = entry.MonthKey,
                MonthLabel = MonthHelper.GetMonthLabel(entry.MonthKey),
                UserId = userId,
                UserName = entry.UserName,
                Hours = entry.Hours,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        var allocations = await _db.PotencialProjectAllocations
            .Include(a => a.User)
            .Where(a => a.PotencialProjectId == id)
            .OrderBy(a => a.UserName)
            .ThenBy(a => a.MonthKey)
            .ToListAsync();

        return Ok(new { message = "Asignaciones guardadas", allocations = allocations.Select(a => MapAllocationToDto(a)) });
    }

    // POST api/potencial-projects/capacity-limits
    [HttpPost("capacity-limits")]
    public async Task<IActionResult> GetCapacityLimits([FromBody] CapacityLimitsRequestDto dto)
    {
        if (dto.UserNames == null || !dto.UserNames.Any() || dto.MonthKeys == null || !dto.MonthKeys.Any())
            return UnprocessableEntity(new { message = "user_names y month_keys son requeridos" });

        var capacityData = await GetCapacityData(dto.UserNames, dto.MonthKeys, dto.PotencialProjectId);
        var limits = new Dictionary<string, Dictionary<string, object>>();

        foreach (var userName in dto.UserNames)
        {
            limits[userName] = new Dictionary<string, object>();
            var userId = capacityData.NameToUserId.GetValueOrDefault(userName);

            foreach (var monthKey in dto.MonthKeys)
            {
                var defaultHours = capacityData.DefaultHours.GetValueOrDefault(monthKey, 160.0m);
                var capacity = userId.HasValue && capacityData.CapacityMap.TryGetValue(userId.Value, out var userCap) && userCap.TryGetValue(monthKey, out var cap)
                    ? cap : defaultHours;
                var etcKey = $"{userId}_{monthKey}";
                var etcHours = userId.HasValue ? capacityData.EtcSums.GetValueOrDefault(etcKey, 0m) : 0m;
                var otherPotencial = userId.HasValue ? capacityData.PotencialByUserMonth.GetValueOrDefault(etcKey, 0m) : 0m;
                var available = Math.Max(0, capacity - etcHours - otherPotencial);

                limits[userName][monthKey] = new
                {
                    capacity = Math.Round(capacity, 2),
                    etc_hours = Math.Round(etcHours, 2),
                    other_potencial_hours = Math.Round(otherPotencial, 2),
                    available = Math.Round(available, 2)
                };
            }
        }

        return Ok(new { limits });
    }

    // POST api/potencial-projects/validate-capacity
    [HttpPost("validate-capacity")]
    public async Task<IActionResult> ValidateCapacity([FromBody] ValidateCapacityRequestDto dto)
    {
        if (dto.Entries == null || !dto.Entries.Any())
            return UnprocessableEntity(new { valid = false, errors = new List<object>(), message = "entries es requerido" });

        var monthKeys = dto.Entries.Select(e => e.MonthKey).Distinct().ToList();
        var userNames = dto.Entries.Select(e => e.UserName).Distinct().ToList();

        var capacityData = await GetCapacityData(userNames, monthKeys, dto.PotencialProjectId);
        var errors = new List<object>();

        foreach (var entry in dto.Entries)
        {
            if (entry.Hours <= 0) continue;

            var userName = entry.UserName.Trim();
            var monthKey = entry.MonthKey;
            var monthLabel = MonthHelper.GetMonthLabel(monthKey);
            var userId = capacityData.NameToUserId.GetValueOrDefault(userName);

            if (!userId.HasValue)
            {
                errors.Add(new
                {
                    user_name = userName,
                    month_key = monthKey,
                    month_label = monthLabel,
                    hours_entered = entry.Hours,
                    capacity = (decimal?)null,
                    etc_hours = (decimal?)null,
                    other_potencial_hours = (decimal?)null,
                    available = (decimal?)null,
                    message = $"El usuario \"{userName}\" no está en usuarios clocky. Agregalo ahí primero."
                });
                continue;
            }

            var defaultHours = capacityData.DefaultHours.GetValueOrDefault(monthKey, 160.0m);
            var capacity = capacityData.CapacityMap.TryGetValue(userId.Value, out var userCap) && userCap.TryGetValue(monthKey, out var cap)
                ? cap : defaultHours;
            var etcKey = $"{userId}_{monthKey}";
            var etcHours = capacityData.EtcSums.GetValueOrDefault(etcKey, 0m);
            var otherPotencial = capacityData.PotencialByUserMonth.GetValueOrDefault(etcKey, 0m);
            var available = Math.Max(0, capacity - etcHours - otherPotencial);

            if (entry.Hours > available)
            {
                errors.Add(new
                {
                    user_name = userName,
                    month_key = monthKey,
                    month_label = monthLabel,
                    hours_entered = entry.Hours,
                    capacity = Math.Round(capacity, 2),
                    etc_hours = Math.Round(etcHours, 2),
                    other_potencial_hours = Math.Round(otherPotencial, 2),
                    available = Math.Round(available, 2),
                    message = $"{userName} ({monthLabel}): capacidad {capacity}h, ETC {Math.Round(etcHours, 2)}h, otros estimados {Math.Round(otherPotencial, 2)}h. Quedan {Math.Round(available, 2)}h libres."
                });
            }
        }

        return Ok(new { valid = !errors.Any(), errors });
    }

    private async Task<CapacityData> GetCapacityData(List<string> userNames, List<string> monthKeys, ulong? excludeProjectId)
    {
        var nameToUserId = new Dictionary<string, ulong?>();
        foreach (var name in userNames)
        {
            var user = await _db.ClockifyUsers
                .FirstOrDefaultAsync(u => u.Name.Trim() == name.Trim());
            nameToUserId[name] = user?.Id;
        }

        var userIds = nameToUserId.Values.Where(id => id.HasValue).Select(id => id!.Value).ToList();

        var calendars = await _db.WorkingDaysCalendars
            .Where(c => monthKeys.Contains(c.MonthKey))
            .ToListAsync();

        var defaultHours = monthKeys.ToDictionary(
            mk => mk,
            mk => (decimal)(calendars.FirstOrDefault(c => c.MonthKey == mk)?.HoursMonth ?? 160m)
        );

        var capacities = await _db.UserMonthlyCapacities
            .Where(c => userIds.Contains(c.UserId) && monthKeys.Contains(c.MonthKey))
            .ToListAsync();

        var capacityMap = new Dictionary<ulong, Dictionary<string, decimal>>();
        foreach (var c in capacities)
        {
            if (!capacityMap.ContainsKey(c.UserId))
                capacityMap[c.UserId] = new Dictionary<string, decimal>();
            capacityMap[c.UserId][c.MonthKey] = c.Hours;
        }

        // ETC sums por usuario y mes
        var snapshotIds = new List<ulong>();
        foreach (var userId in userIds)
        {
            var snapshotId = await _db.EtcSnapshots
                .Where(s => s.ProjectId != 0)
                .OrderByDescending(s => s.Version)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();
            if (snapshotId > 0) snapshotIds.Add(snapshotId);
        }

        var etcRecords = await _db.EtcRecords
            .Where(r => userIds.Contains(r.UserId ?? 0) && monthKeys.Contains(r.MonthKey))
            .ToListAsync();

        var etcSums = etcRecords
            .GroupBy(r => $"{r.UserId}_{r.MonthKey}")
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Hours));

        var potencialQuery = _db.PotencialProjectAllocations
            .Where(a => monthKeys.Contains(a.MonthKey) && a.Hours > 0);

        if (excludeProjectId.HasValue)
            potencialQuery = potencialQuery.Where(a => a.PotencialProjectId != excludeProjectId.Value);

        var potencialAll = await potencialQuery.ToListAsync();
        var potencialByUserMonth = new Dictionary<string, decimal>();

        foreach (var row in potencialAll)
        {
            var uid = row.UserId;
            if (!uid.HasValue && !string.IsNullOrEmpty(row.UserName))
            {
                var user = await _db.ClockifyUsers
                    .FirstOrDefaultAsync(u => u.Name.Trim() == row.UserName.Trim());
                uid = user?.Id;
            }
            if (!uid.HasValue) continue;
            var key = $"{uid}_{row.MonthKey}";
            potencialByUserMonth[key] = potencialByUserMonth.GetValueOrDefault(key, 0m) + row.Hours;
        }

        return new CapacityData
        {
            NameToUserId = nameToUserId,
            DefaultHours = defaultHours,
            CapacityMap = capacityMap,
            EtcSums = etcSums,
            PotencialByUserMonth = potencialByUserMonth
        };
    }

    private static string RoleToShort(string? role)
    {
        if (string.IsNullOrEmpty(role)) return string.Empty;
        return role.ToLower() switch
        {
            "líder" or "lider" => "LD",
            "analista" => "AF",
            "desarrollador" => "DEV",
            "qa" => "QA",
            _ => role.Length >= 3 ? role[..3].ToUpper() : role.ToUpper()
        };
    }

    private static object MapAllocationToDto(PotencialProjectAllocation a) => new
    {
        id = a.Id,
        potencial_project_id = a.PotencialProjectId,
        month_key = a.MonthKey,
        month_label = a.MonthLabel,
        user_id = a.UserId,
        user_name = a.UserName,
        hours = a.Hours,
        role = a.User?.Role,
        role_short = RoleToShort(a.User?.Role),
        created_at = a.CreatedAt,
        updated_at = a.UpdatedAt
    };

    private static PotencialProjectDto MapToDto(PotencialProject p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Code = p.Code,
        ClientId = p.PotencialClientId,
        ClientName = p.PotencialClient?.Name
    };

    private class CapacityData
    {
        public Dictionary<string, ulong?> NameToUserId { get; set; } = new();
        public Dictionary<string, decimal> DefaultHours { get; set; } = new();
        public Dictionary<ulong, Dictionary<string, decimal>> CapacityMap { get; set; } = new();
        public Dictionary<string, decimal> EtcSums { get; set; } = new();
        public Dictionary<string, decimal> PotencialByUserMonth { get; set; } = new();
    }
}