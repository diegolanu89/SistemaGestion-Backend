using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;
using bdt_evm_app.Services;

namespace bdt_evm_app.Controllers;

[ApiController]
public class EtcController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly EtcService _etcService;
    private readonly ILogger<EtcController> _logger;

    public EtcController(AppDbContext db, EtcService etcService, ILogger<EtcController> logger)
    {
        _db = db;
        _etcService = etcService;
        _logger = logger;
    }

    // GET api/projects/{projectId}/etc
    [HttpGet("api/projects/{projectId}/etc")]
    [RequirePermission("ETC_ACCESS")]
    public async Task<IActionResult> GetByProject(ulong projectId, [FromQuery] string? snapshot)
    {
        var project = await _db.ClockifyProjects.FindAsync(projectId);
        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        var wantBaseline = snapshot == "baseline";
        var (etcSnapshot, records) = await _etcService.GetRecordsForProject(projectId, wantBaseline);

        var userIds = records.Where(r => r.UserId.HasValue).Select(r => r.UserId!.Value).Distinct().ToList();
        var usersById = await _db.ClockifyUsers
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var recordDtos = records.Select(r => new EtcRecordDto
        {
            Id = r.Id,
            ProjectId = r.ProjectId,
            SnapshotId = r.SnapshotId,
            UserId = r.UserId,
            UserName = r.UserName,
            MonthKey = r.MonthKey,
            MonthLabel = r.MonthLabel,
            Hours = r.Hours,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            User = r.UserId.HasValue && usersById.TryGetValue(r.UserId.Value, out var u)
                ? new EtcUserDto { Id = u.Id, Name = u.Name, Email = u.Email }
                : null
        }).ToList();

        return Ok(new
        {
            snapshot = etcSnapshot != null ? new
            {
                id = etcSnapshot.Id,
                version = etcSnapshot.Version,
                label = etcSnapshot.Label,
                created_at = etcSnapshot.CreatedAt?.ToString("o")
            } : null,
            records = recordDtos
        });
    }

    // GET api/projects/{projectId}/etc/summary
    // Devuelve totales por recurso (fila) y por mes (columna) para la grilla ETC
    [HttpGet("api/projects/{projectId}/etc/summary")]
    [RequirePermission("ETC_ACCESS")]
    public async Task<IActionResult> GetSummary(ulong projectId, [FromQuery] string? snapshot)
    {
        var project = await _db.ClockifyProjects.FindAsync(projectId);
        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        var wantBaseline = snapshot == "baseline";
        var (etcSnapshot, records) = await _etcService.GetRecordsForProject(projectId, wantBaseline);

        // Meses únicos ordenados cronológicamente
        var months = records.Select(r => r.MonthKey).Distinct().OrderBy(m => m).ToList();

        // Agrupar por recurso → filas
        var resourceRows = records
            .GroupBy(r => r.UserName ?? "Sin nombre")
            .Select(g => new EtcResourceRowDto
            {
                UserId = g.First().UserId,
                UserName = g.Key,
                HoursByMonth = months.ToDictionary(m => m, m => g.Where(r => r.MonthKey == m).Sum(r => r.Hours)),
                Total = g.Sum(r => r.Hours)
            })
            .OrderBy(r => r.UserName)
            .ToList();

        // Totales por mes → columnas
        var totalsByMonth = months.ToDictionary(
            m => m,
            m => records.Where(r => r.MonthKey == m).Sum(r => r.Hours)
        );

        var summary = new EtcSummaryDto
        {
            Snapshot = etcSnapshot != null ? new
            {
                id = etcSnapshot.Id,
                version = etcSnapshot.Version,
                label = etcSnapshot.Label,
                created_at = etcSnapshot.CreatedAt?.ToString("o")
            } : null,
            Months = months,
            Resources = resourceRows,
            TotalsByMonth = totalsByMonth,
            GrandTotal = records.Sum(r => r.Hours)
        };

        return Ok(new { success = true, data = summary });
    }

    // POST api/projects/{projectId}/etc
    [HttpPost("api/projects/{projectId}/etc")]
    [RequirePermission("ETC_EDIT")]
    public async Task<IActionResult> Create(ulong projectId, [FromBody] CreateEtcRecordDto dto)
    {
        var project = await _db.ClockifyProjects.FindAsync(projectId);
        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        if (dto.Users == null || !dto.Users.Any())
            return UnprocessableEntity(new { error = "users es requerido" });

        if (string.IsNullOrEmpty(dto.MonthKey) || dto.MonthKey.Length != 7)
            return UnprocessableEntity(new { error = "month_key debe tener formato YYYY-MM" });

        var entries = dto.Users.Select(u => new EtcEntryDto
        {
            UserName = u,
            MonthKey = dto.MonthKey,
            MonthLabel = dto.MonthLabel,
            Hours = dto.Hours
        }).ToList();

        var capacityErrors = await ValidateCapacityForEntries((int)projectId, entries);
        if (capacityErrors.Any())
            return UnprocessableEntity(new { error = "Validación de capacidad", message = string.Join("\n", capacityErrors.Select(e => e.Message)) });

        var snapshot = await GetOrCreateCurrentSnapshot((int)projectId);
        var records = new List<EtcRecord>();

        foreach (var userName in dto.Users)
        {
            var user = await _db.ClockifyUsers.FirstOrDefaultAsync(u => u.Name.Trim() == userName.Trim());
            var record = new EtcRecord
            {
                ProjectId = projectId,
                SnapshotId = snapshot.Id,
                UserId = user?.Id,
                UserName = userName,
                MonthKey = dto.MonthKey,
                MonthLabel = dto.MonthLabel,
                Hours = dto.Hours,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.EtcRecords.Add(record);
            records.Add(record);
        }

        await _db.SaveChangesAsync();
        return StatusCode(201, new { message = "Registros ETC creados exitosamente", records });
    }

    // PUT api/etc/{id}
    [HttpPut("api/etc/{id}")]
    [RequirePermission("ETC_EDIT")]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateEtcRecordDto dto)
    {
        var record = await _db.EtcRecords.FindAsync(id);
        if (record == null)
            return NotFound(new { message = "Registro no encontrado" });

        if (dto.Hours > 0)
        {
            var user = await _db.ClockifyUsers.FirstOrDefaultAsync(u => u.Name.Trim() == dto.UserName.Trim());
            if (user == null)
                return UnprocessableEntity(new { error = "Validación de capacidad", message = $"El usuario \"{dto.UserName}\" no está en usuarios clocky." });

            var capacity = await GetUserCapacity(user.Id, dto.MonthKey);
            var latestSnapshotIds = await GetLatestSnapshotIdsPerProject();
            var projectIdsWithSnapshots = await _db.EtcSnapshots
                .Select(s => s.ProjectId).Distinct().ToListAsync();
            var hoursTaken = await _db.EtcRecords
                .Where(r =>
                    r.UserId == user.Id &&
                    r.MonthKey == dto.MonthKey &&
                    r.ProjectId != record.ProjectId &&
                    (latestSnapshotIds.Contains(r.SnapshotId ?? 0) ||
                     (r.SnapshotId == null && !projectIdsWithSnapshots.Contains(r.ProjectId))))
                .SumAsync(r => r.Hours);

            var hoursFree = Math.Max(0, capacity - hoursTaken);
            if (dto.Hours > hoursFree)
                return UnprocessableEntity(new
                {
                    error = "Validación de capacidad",
                    message = $"{dto.UserName} ({dto.MonthLabel}): tiene {Math.Round(hoursTaken, 2)}h tomadas y {Math.Round(hoursFree, 2)}h libres."
                });

            record.UserId = user.Id;
        }

        record.UserName = dto.UserName;
        record.MonthKey = dto.MonthKey;
        record.MonthLabel = dto.MonthLabel;
        record.Hours = dto.Hours;
        record.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Registro ETC actualizado exitosamente", record });
    }

    // DELETE api/etc/{id}
    [HttpDelete("api/etc/{id}")]
    [RequirePermission("ETC_EDIT")]
    public async Task<IActionResult> Delete(ulong id)
    {
        var record = await _db.EtcRecords.FindAsync(id);
        if (record == null)
            return NotFound(new { message = "Registro no encontrado" });

        _db.EtcRecords.Remove(record);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Registro ETC eliminado exitosamente" });
    }

    // DELETE api/etc/project/{projectId}
    [HttpDelete("api/etc/project/{projectId}")]
    [RequirePermission("ETC_EDIT")]
    public async Task<IActionResult> DeleteByProject(ulong projectId)
    {
        var project = await _db.ClockifyProjects.FindAsync(projectId);
        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        var records = await _db.EtcRecords
            .Where(r => r.ProjectId == projectId)
            .ToListAsync();

        var snapshots = await _db.EtcSnapshots
            .Where(s => s.ProjectId == projectId)
            .ToListAsync();

        _db.EtcRecords.RemoveRange(records);
        _db.EtcSnapshots.RemoveRange(snapshots);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Registros ETC eliminados exitosamente",
            deleted = records.Count
        });
    }

    // GET api/etc/projects-summary
    [HttpGet("api/etc/projects-summary")]
    [RequirePermission("ETC_ACCESS")]
    public async Task<IActionResult> ProjectsWithEtc()
    {
        var latestSnapshotIds = await GetLatestSnapshotIdsPerProject();
        var projectIdsWithSnapshots = await _db.EtcSnapshots.Select(s => s.ProjectId).Distinct().ToListAsync();

        var projectIds = await _db.EtcRecords
            .Where(r => latestSnapshotIds.Contains(r.SnapshotId ?? 0) ||
                       (r.SnapshotId == null && !projectIdsWithSnapshots.Contains(r.ProjectId)))
            .Select(r => r.ProjectId)
            .Distinct()
            .ToListAsync();

        var projects = await _db.ClockifyProjects
            .Include(p => p.Client)
            .Where(p => projectIds.Contains(p.Id))
            .OrderBy(p => p.Name)
            .ToListAsync();

        return Ok(projects.Select(p => new
        {
            id = p.Id,
            name = p.Name,
            code = p.Code,
            client_id = p.ClientId,
            client_name = p.Client?.Name
        }));
    }

    // POST api/projects/{projectId}/etc/finalize-baseline
    [HttpPost("api/projects/{projectId}/etc/finalize-baseline")]
    [RequirePermission("ETC_EDIT")]
    public async Task<IActionResult> FinalizeBaseline(ulong projectId)
    {
        var project = await _db.ClockifyProjects.FindAsync(projectId);
        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        var existingSnapshot = await _db.EtcSnapshots
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync();

        if (existingSnapshot != null)
            return Ok(new
            {
                message = "El proyecto ya tiene línea base.",
                snapshot = new { id = existingSnapshot.Id, version = existingSnapshot.Version, label = existingSnapshot.Label, created_at = existingSnapshot.CreatedAt?.ToString("o") }
            });

        var recordsWithoutSnapshot = await _db.EtcRecords
            .CountAsync(r => r.ProjectId == projectId && r.SnapshotId == null);

        if (recordsWithoutSnapshot == 0)
            return UnprocessableEntity(new { error = "No hay registros ETC para grabar como línea base." });

        var snapshot = new EtcSnapshot
        {
            ProjectId = projectId,
            Version = 1,
            Label = "Línea base",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.EtcSnapshots.Add(snapshot);
        await _db.SaveChangesAsync();

        await _db.EtcRecords
            .Where(r => r.ProjectId == projectId && r.SnapshotId == null)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.SnapshotId, snapshot.Id));

        return Ok(new
        {
            message = "Línea base grabada correctamente.",
            snapshot = new { id = snapshot.Id, version = snapshot.Version, label = snapshot.Label, created_at = snapshot.CreatedAt?.ToString("o") }
        });
    }

    // POST api/projects/{projectId}/etc/snapshot
    [HttpPost("api/projects/{projectId}/etc/snapshot")]
    [RequirePermission("ETC_EDIT")]
    public async Task<IActionResult> CreateSnapshot(ulong projectId, [FromBody] CreateSnapshotDto dto)
    {
        var project = await _db.ClockifyProjects.FindAsync(projectId);
        if (project == null)
            return NotFound(new { message = "Proyecto no encontrado" });

        if (dto.Entries == null || !dto.Entries.Any())
            return UnprocessableEntity(new { error = "entries es requerido" });

        var lastSnapshot = await _db.EtcSnapshots
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync();

        var nextVersion = lastSnapshot != null ? lastSnapshot.Version + 1 : 1;

        var snapshot = new EtcSnapshot
        {
            ProjectId = projectId,
            Version = nextVersion,
            Label = $"Semana {nextVersion}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.EtcSnapshots.Add(snapshot);
        await _db.SaveChangesAsync();

        var records = new List<EtcRecord>();
        foreach (var entry in dto.Entries)
        {
            var user = await _db.ClockifyUsers.FirstOrDefaultAsync(u => u.Name.Trim() == entry.UserName.Trim());
            var record = new EtcRecord
            {
                ProjectId = projectId,
                SnapshotId = snapshot.Id,
                UserId = user?.Id,
                UserName = entry.UserName,
                MonthKey = entry.MonthKey,
                MonthLabel = entry.MonthLabel,
                Hours = entry.Hours,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.EtcRecords.Add(record);
            records.Add(record);
        }

        await _db.SaveChangesAsync();
        return StatusCode(201, new
        {
            message = "Nueva versión creada correctamente.",
            snapshot = new { id = snapshot.Id, version = snapshot.Version, label = snapshot.Label, created_at = snapshot.CreatedAt?.ToString("o") },
            records
        });
    }

    // POST api/etc/bulk
    [HttpPost("api/etc/bulk")]
    [RequirePermission("ETC_EDIT")]
    public async Task<IActionResult> StoreBulk([FromBody] BulkEtcDto dto)
    {
        if (dto.Entries == null || !dto.Entries.Any())
            return UnprocessableEntity(new { error = "entries es requerido" });

        var project = await _db.ClockifyProjects.FindAsync(dto.ProjectId);
        if (project == null)
            return UnprocessableEntity(new { error = "Proyecto no encontrado" });

        var capacityErrors = await ValidateCapacityForEntries((int)dto.ProjectId, dto.Entries);
        if (capacityErrors.Any())
            return UnprocessableEntity(new { error = "Validación de capacidad", message = string.Join("\n", capacityErrors.Select(e => e.Message)) });

        var snapshot = await GetOrCreateCurrentSnapshot((int)dto.ProjectId);
        var records = new List<EtcRecord>();

        foreach (var entry in dto.Entries)
        {
            var user = await _db.ClockifyUsers.FirstOrDefaultAsync(u => u.Name.Trim() == entry.UserName.Trim());
            var record = new EtcRecord
            {
                ProjectId = dto.ProjectId,
                SnapshotId = snapshot.Id,
                UserId = user?.Id,
                UserName = entry.UserName,
                MonthKey = entry.MonthKey,
                MonthLabel = entry.MonthLabel,
                Hours = entry.Hours,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.EtcRecords.Add(record);
            records.Add(record);
        }

        await _db.SaveChangesAsync();
        return StatusCode(201, new { message = "Registros ETC creados exitosamente", records });
    }

    // POST api/etc/validate-capacity
    [HttpPost("api/etc/validate-capacity")]
    [RequirePermission("ETC_ACCESS")]
    public async Task<IActionResult> ValidateCapacity([FromBody] ValidateEtcCapacityDto dto)
    {
        if (dto.Entries == null || !dto.Entries.Any())
            return UnprocessableEntity(new { valid = false, errors = new List<object>(), message = "entries es requerido" });

        var errors = await ValidateCapacityForEntries((int)dto.ProjectId, dto.Entries);
        return Ok(new { valid = !errors.Any(), errors });
    }

    // GET api/etc/export-capacities
    [HttpGet("api/etc/export-capacities")]
    [RequirePermission("ETC_ACCESS")]
    public async Task<IActionResult> ExportCapacities()
    {
        var currentMonth = DateTime.UtcNow.ToString("yyyy-MM");
        var latestSnapshotIds = await GetLatestSnapshotIdsPerProject();
        var projectIdsWithSnapshots = await _db.EtcSnapshots.Select(s => s.ProjectId).Distinct().ToListAsync();

        var records = await _db.EtcRecords
             .Where(r =>
                 r.MonthKey.CompareTo(currentMonth) >= 0 &&
                 (latestSnapshotIds.Contains(r.SnapshotId ?? 0) ||
                  (r.SnapshotId == null && !projectIdsWithSnapshots.Contains(r.ProjectId))))
            .OrderBy(r => r.MonthKey)
            .ThenBy(r => r.ProjectId)
            .ThenBy(r => r.UserName)
            .ToListAsync();

        var projectIds = records.Select(r => r.ProjectId).Distinct().ToList();
        var projects = await _db.ClockifyProjects
            .Where(p => projectIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        return Ok(records.Select(r => new
        {
            project_id = r.ProjectId,
            project_name = projects.TryGetValue(r.ProjectId, out var p) ? p.Name : "Proyecto no encontrado",
            user_name = r.UserName ?? "Sin nombre",
            hours = r.Hours,
            month_key = r.MonthKey,
            month_label = r.MonthLabel
        }));
    }

    private async Task<EtcSnapshot> GetOrCreateCurrentSnapshot(int projectId)
    {
        var snapshot = await _db.EtcSnapshots
            .Where(s => s.ProjectId == (ulong)projectId)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync();

        if (snapshot != null) return snapshot;

        snapshot = new EtcSnapshot
        {
            ProjectId = (ulong)projectId,
            Version = 1,
            Label = "Línea base",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.EtcSnapshots.Add(snapshot);
        await _db.SaveChangesAsync();
        return snapshot;
    }

    private async Task<List<ulong>> GetLatestSnapshotIdsPerProject()
    {
        return await _db.EtcSnapshots
            .GroupBy(s => s.ProjectId)
            .Select(g => g.OrderByDescending(s => s.Version).First().Id)
            .ToListAsync();
    }

    private async Task<decimal> GetUserCapacity(ulong userId, string monthKey)
    {
        var capacity = await _db.UserMonthlyCapacities
            .Where(c => c.UserId == userId && c.MonthKey == monthKey)
            .Select(c => (decimal?)c.Hours)
            .FirstOrDefaultAsync();

        if (capacity.HasValue) return capacity.Value;

        var cal = await _db.WorkingDaysCalendars
            .Where(c => c.MonthKey == monthKey)
            .FirstOrDefaultAsync();

        return cal?.HoursMonth ?? 160m;
    }

    private async Task<List<(string Message, string UserName, string MonthKey)>> ValidateCapacityForEntries(int projectId, List<EtcEntryDto> entries)
    {
        var errors = new List<(string Message, string UserName, string MonthKey)>();
        var monthKeys = entries.Select(e => e.MonthKey).Distinct().ToList();
        var userNames = entries.Select(e => e.UserName).Distinct().ToList();

        var calendars = await _db.WorkingDaysCalendars
            .Where(c => monthKeys.Contains(c.MonthKey))
            .ToDictionaryAsync(c => c.MonthKey);

        var nameToUser = new Dictionary<string, ClockifyUser?>();
        foreach (var name in userNames)
        {
            var user = await _db.ClockifyUsers.FirstOrDefaultAsync(u => u.Name.Trim() == name.Trim());
            nameToUser[name] = user;
        }

        var userIds = nameToUser.Values.Where(u => u != null).Select(u => u!.Id).ToList();
        var capacities = await _db.UserMonthlyCapacities
            .Where(c => userIds.Contains(c.UserId) && monthKeys.Contains(c.MonthKey))
            .ToListAsync();

        var latestSnapshotIds = await GetLatestSnapshotIdsPerProject();
        var projectIdsWithSnapshots = await _db.EtcSnapshots
            .Select(s => s.ProjectId).Distinct().ToListAsync();

        foreach (var entry in entries)
        {
            if (entry.Hours <= 0) continue;

            var user = nameToUser.GetValueOrDefault(entry.UserName);
            if (user == null)
            {
                errors.Add(($"El usuario \"{entry.UserName}\" no está en usuarios clocky.", entry.UserName, entry.MonthKey));
                continue;
            }

            var capacity = capacities.FirstOrDefault(c => c.UserId == user.Id && c.MonthKey == entry.MonthKey)?.Hours
                ?? (calendars.TryGetValue(entry.MonthKey, out var cal) ? cal.HoursMonth : 160m);

            var hoursTaken = await _db.EtcRecords
                .Where(r =>
                    r.UserId == user.Id &&
                    r.MonthKey == entry.MonthKey &&
                    r.ProjectId != (ulong)projectId &&
                    (latestSnapshotIds.Contains(r.SnapshotId ?? 0) ||
                     (r.SnapshotId == null && !projectIdsWithSnapshots.Contains(r.ProjectId))))
                .SumAsync(r => r.Hours);

            var hoursFree = Math.Max(0, capacity - hoursTaken);
            if (entry.Hours > hoursFree)
                errors.Add(($"{entry.UserName} ({entry.MonthLabel}): tiene {Math.Round(hoursTaken, 2)}h tomadas y {Math.Round(hoursFree, 2)}h libres. No podés cargar más de {Math.Round(hoursFree, 2)}h.", entry.UserName, entry.MonthKey));
        }

        return errors;
    }
}