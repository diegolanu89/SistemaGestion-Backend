using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/timesheet-users")]
public class TimesheetUsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<TimesheetUsersController> _logger;

    public TimesheetUsersController(AppDbContext db, ILogger<TimesheetUsersController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET api/timesheet-users
    [RequirePermission("SETTINGS_ACCESS", "ETC_ACCESS", "ESTIMATED_PROJECTS_ACCESS", "REPORTS_ACCESS")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? active,
        [FromQuery] int per_page = 100,
        [FromQuery] int page = 1)
    {
        try
        {
            var query = _db.TimesheetUsers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u =>
                    u.Name.Contains(search) ||
                    (u.Email != null && u.Email.Contains(search)));

            if (active.HasValue)
                query = query.Where(u => u.Active == active.Value);

            query = query.OrderBy(u => u.Name);

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
                    data = items,
                    current_page = page,
                    per_page,
                    total,
                    last_page = (int)Math.Ceiling((double)total / per_page)
                }
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al listar timesheet_users");
            return StatusCode(500, new { success = false, message = "Error al obtener los usuarios" });
        }
    }

    // POST api/timesheet-users
    [RequirePermission("SETTINGS_ACCESS")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTimesheetUserDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return UnprocessableEntity(new { success = false, message = "El nombre es requerido" });

            var user = new TimesheetUser
            {
                TimesheetUserId = null,
                Name = dto.Name,
                Email = dto.Email,
                Active = dto.Active,
                Role = dto.Role,
                DefaultMonthHours = dto.DefaultMonthHours,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.TimesheetUsers.Add(user);
            await _db.SaveChangesAsync();

            return StatusCode(201, new { success = true, data = user });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al crear timesheet_user");
            return StatusCode(500, new { success = false, message = "Error al crear el usuario" });
        }
    }

    // PUT api/timesheet-users/{id}
    [RequirePermission("SETTINGS_ACCESS")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateTimesheetUserDto dto)
    {
        try
        {
            var user = await _db.TimesheetUsers.FindAsync(id);
            if (user == null)
                return NotFound(new { success = false, message = "Usuario no encontrado" });

            if (!string.IsNullOrEmpty(dto.TimesheetUserId) && dto.TimesheetUserId != user.TimesheetUserId)
            {
                var exists = await _db.TimesheetUsers
                    .AnyAsync(u => u.TimesheetUserId == dto.TimesheetUserId && u.Id != id);
                if (exists)
                    return UnprocessableEntity(new { success = false, message = "El timesheet_user_id ya existe" });
                user.TimesheetUserId = dto.TimesheetUserId;
            }

            if (dto.Name != null) user.Name = dto.Name;
            if (dto.Email != null) user.Email = dto.Email;
            if (dto.Active.HasValue) user.Active = dto.Active.Value;
            if (dto.Role != null) user.Role = dto.Role;
            if (dto.DefaultMonthHours.HasValue) user.DefaultMonthHours = dto.DefaultMonthHours;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { success = true, data = user });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al actualizar timesheet_user {Id}", id);
            return StatusCode(500, new { success = false, message = "Error al actualizar el usuario" });
        }
    }

    // DELETE api/timesheet-users/{id}
    [RequirePermission("SETTINGS_ACCESS")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(ulong id)
    {
        try
        {
            var user = await _db.TimesheetUsers.FindAsync(id);
            if (user == null)
                return NotFound(new { success = false, message = "Usuario no encontrado" });

            _db.TimesheetUsers.Remove(user);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Usuario eliminado correctamente" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar timesheet_user {Id}", id);
            return StatusCode(500, new { success = false, message = "Error al eliminar el usuario" });
        }
    }

    // GET api/timesheet-users/options
    // Devuelve lista plana sin paginar, pensada para poblar dropdowns/selects.
    // Por defecto trae solo activos; pasar ?active=false para incluir también inactivos.
    [RequirePermission("SETTINGS_ACCESS", "ETC_ACCESS", "ESTIMATED_PROJECTS_ACCESS", "REPORTS_ACCESS")]
    [HttpGet("options")]
    public async Task<IActionResult> GetOptions([FromQuery] bool active = true)
    {
        try
        {
            var query = _db.TimesheetUsers.AsQueryable();
            if (active)
                query = query.Where(u => u.Active);
            var users = await query
                .OrderBy(u => u.Name)
                .Select(u => new TimesheetUserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email
                })
                .ToListAsync();
            return Ok(new { success = true, data = new { users } });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener opciones de timesheet_users");
            return StatusCode(500, new { success = false, message = "Error al obtener las opciones", error = e.Message });
        }
    }
}