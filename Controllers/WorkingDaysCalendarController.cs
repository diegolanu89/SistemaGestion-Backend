using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/working-days-calendar")]
[RequirePermission("SETTINGS_ACCESS")]
public class WorkingDaysCalendarController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<WorkingDaysCalendarController> _logger;

    public WorkingDaysCalendarController(AppDbContext db, ILogger<WorkingDaysCalendarController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET api/working-days-calendar
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? month_key,
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] string sort_by = "year",
        [FromQuery] string sort_order = "desc",
        [FromQuery] int per_page = 15,
        [FromQuery] int page = 1)
    {
        try
        {
            var query = _db.WorkingDaysCalendars.AsQueryable();

            if (!string.IsNullOrEmpty(month_key))
                query = query.Where(c => c.MonthKey == month_key);
            if (year.HasValue)
                query = query.Where(c => c.Year == year.Value);
            if (month.HasValue)
                query = query.Where(c => c.Month == month.Value);

            query = sort_by switch
            {
                "month_key" => sort_order == "asc" ? query.OrderBy(c => c.MonthKey) : query.OrderByDescending(c => c.MonthKey),
                "month" => sort_order == "asc" ? query.OrderBy(c => c.Year).ThenBy(c => c.Month) : query.OrderByDescending(c => c.Year).ThenByDescending(c => c.Month),
                _ => sort_order == "asc" ? query.OrderBy(c => c.Year).ThenBy(c => c.Month) : query.OrderByDescending(c => c.Year).ThenByDescending(c => c.Month)
            };

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * per_page)
                .Take(per_page)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    data = items.Select(MapToDto),
                    current_page = page,
                    per_page,
                    total,
                    last_page = (int)Math.Ceiling((double)total / per_page)
                }
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al listar working_days_calendar");
            return StatusCode(500, new { success = false, message = "Error al obtener los registros", error = e.Message });
        }
    }

    // GET api/working-days-calendar/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(ulong id)
    {
        try
        {
            var calendar = await _db.WorkingDaysCalendars.FindAsync(id);
            if (calendar == null)
                return NotFound(new { success = false, message = "Registro no encontrado" });

            return Ok(new { success = true, data = MapToDto(calendar) });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, message = "Error al obtener el registro", error = e.Message });
        }
    }

    // GET api/working-days-calendar/month/{monthKey}
    [HttpGet("month/{monthKey}")]
    public async Task<IActionResult> GetByMonthKey(string monthKey)
    {
        try
        {
            var calendar = await _db.WorkingDaysCalendars
                .FirstOrDefaultAsync(c => c.MonthKey == monthKey);

            if (calendar == null)
                return NotFound(new { success = false, message = "Calendario no encontrado para el mes especificado" });

            return Ok(new { success = true, data = MapToDto(calendar) });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, message = "Error al obtener el calendario", error = e.Message });
        }
    }

    // POST api/working-days-calendar
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkingDaysCalendarDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.MonthKey))
                return UnprocessableEntity(new { success = false, message = "month_key es requerido" });

            if (dto.Year < 2000 || dto.Year > 2100)
                return UnprocessableEntity(new { success = false, message = "El año debe estar entre 2000 y 2100" });

            if (dto.Month < 1 || dto.Month > 12)
                return UnprocessableEntity(new { success = false, message = "El mes debe estar entre 1 y 12" });

            if (dto.TotalDays < 1 || dto.TotalDays > 31)
                return UnprocessableEntity(new { success = false, message = "total_days debe estar entre 1 y 31" });

            var exists = await _db.WorkingDaysCalendars
                .AnyAsync(c => c.MonthKey == dto.MonthKey);
            if (exists)
                return UnprocessableEntity(new { success = false, message = "Ya existe un calendario para ese mes" });

            var calendar = new WorkingDaysCalendar
            {
                MonthKey = dto.MonthKey,
                Year = dto.Year,
                Month = dto.Month,
                TotalDays = dto.TotalDays,
                WorkingDays = dto.WorkingDays,
                HoursMonth = dto.WorkingDays * 8,
                HolidayDays = dto.HolidayDays,
                HolidaysList = dto.HolidaysList,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.WorkingDaysCalendars.Add(calendar);
            await _db.SaveChangesAsync();

            return StatusCode(201, new
            {
                success = true,
                message = "Calendario creado exitosamente",
                data = MapToDto(calendar)
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al crear working_days_calendar");
            return StatusCode(500, new { success = false, message = "Error al crear el calendario", error = e.Message });
        }
    }

    // PUT api/working-days-calendar/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateWorkingDaysCalendarDto dto)
    {
        try
        {
            var calendar = await _db.WorkingDaysCalendars.FindAsync(id);
            if (calendar == null)
                return NotFound(new { success = false, message = "Registro no encontrado" });

            if (dto.Year.HasValue && (dto.Year < 2000 || dto.Year > 2100))
                return UnprocessableEntity(new { success = false, message = "El año debe estar entre 2000 y 2100" });

            if (dto.Month.HasValue && (dto.Month < 1 || dto.Month > 12))
                return UnprocessableEntity(new { success = false, message = "El mes debe estar entre 1 y 12" });

            if (dto.TotalDays.HasValue && (dto.TotalDays < 1 || dto.TotalDays > 31))
                return UnprocessableEntity(new { success = false, message = "total_days debe estar entre 1 y 31" });

            if (!string.IsNullOrEmpty(dto.MonthKey) && dto.MonthKey != calendar.MonthKey)
            {
                var exists = await _db.WorkingDaysCalendars
                    .AnyAsync(c => c.MonthKey == dto.MonthKey);
                if (exists)
                    return UnprocessableEntity(new { success = false, message = "Ya existe un calendario para ese mes" });
                calendar.MonthKey = dto.MonthKey;
            }

            if (dto.Year.HasValue) calendar.Year = dto.Year.Value;
            if (dto.Month.HasValue) calendar.Month = dto.Month.Value;
            if (dto.TotalDays.HasValue) calendar.TotalDays = dto.TotalDays.Value;
            if (dto.WorkingDays.HasValue)
            {
                calendar.WorkingDays = dto.WorkingDays.Value;
                calendar.HoursMonth = dto.WorkingDays.Value * 8;
            }
            if (dto.HolidayDays.HasValue) calendar.HolidayDays = dto.HolidayDays.Value;
            if (dto.HolidaysList != null) calendar.HolidaysList = dto.HolidaysList;
            if (dto.Notes != null) calendar.Notes = dto.Notes;
            calendar.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Calendario actualizado exitosamente",
                data = MapToDto(calendar)
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al actualizar working_days_calendar {Id}", id);
            return StatusCode(500, new { success = false, message = "Error al actualizar el calendario", error = e.Message });
        }
    }

    // DELETE api/working-days-calendar/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(ulong id)
    {
        try
        {
            var calendar = await _db.WorkingDaysCalendars.FindAsync(id);
            if (calendar == null)
                return NotFound(new { success = false, message = "Registro no encontrado" });

            _db.WorkingDaysCalendars.Remove(calendar);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Calendario eliminado exitosamente" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar working_days_calendar {Id}", id);
            return StatusCode(500, new { success = false, message = "Error al eliminar el calendario", error = e.Message });
        }
    }

    private static WorkingDaysCalendarDto MapToDto(WorkingDaysCalendar c) => new()
    {
        Id = c.Id,
        MonthKey = c.MonthKey,
        Year = c.Year,
        Month = c.Month,
        TotalDays = c.TotalDays,
        WorkingDays = c.WorkingDays,
        HoursMonth = c.HoursMonth,
        HolidayDays = c.HolidayDays,
        HolidaysList = c.HolidaysList,
        Notes = c.Notes,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}