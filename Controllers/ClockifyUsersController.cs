using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/clockify-users")]
public class ClockifyUsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<ClockifyUsersController> _logger;

    public ClockifyUsersController(AppDbContext db, ILogger<ClockifyUsersController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET api/clockify-users
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? active,
        [FromQuery] int per_page = 100,
        [FromQuery] int page = 1)
    {
        try
        {
            var query = _db.ClockifyUsers.AsQueryable();

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
            _logger.LogError(e, "Error al listar clockify_users");
            return StatusCode(500, new { success = false, message = "Error al obtener los usuarios" });
        }
    }

    // POST api/clockify-users
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClockifyUserDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return UnprocessableEntity(new { success = false, message = "El nombre es requerido" });

            if (!string.IsNullOrEmpty(dto.ClockifyUserId))
            {
                var exists = await _db.ClockifyUsers
                    .AnyAsync(u => u.ClockifyUserId == dto.ClockifyUserId);
                if (exists)
                    return UnprocessableEntity(new { success = false, message = "El clockify_user_id ya existe" });
            }

            var user = new ClockifyUser
            {
                ClockifyUserId = dto.ClockifyUserId ?? $"manual_{Guid.NewGuid():N}",
                Name = dto.Name,
                Email = dto.Email,
                Active = dto.Active,
                Role = dto.Role,
                DefaultMonthHours = dto.DefaultMonthHours,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.ClockifyUsers.Add(user);
            await _db.SaveChangesAsync();

            return StatusCode(201, new { success = true, data = user });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al crear clockify_user");
            return StatusCode(500, new { success = false, message = "Error al crear el usuario" });
        }
    }

    // PUT api/clockify-users/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateClockifyUserDto dto)
    {
        try
        {
            var user = await _db.ClockifyUsers.FindAsync(id);
            if (user == null)
                return NotFound(new { success = false, message = "Usuario no encontrado" });

            if (!string.IsNullOrEmpty(dto.ClockifyUserId) && dto.ClockifyUserId != user.ClockifyUserId)
            {
                var exists = await _db.ClockifyUsers
                    .AnyAsync(u => u.ClockifyUserId == dto.ClockifyUserId && u.Id != id);
                if (exists)
                    return UnprocessableEntity(new { success = false, message = "El clockify_user_id ya existe" });
                user.ClockifyUserId = dto.ClockifyUserId;
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
            _logger.LogError(e, "Error al actualizar clockify_user {Id}", id);
            return StatusCode(500, new { success = false, message = "Error al actualizar el usuario" });
        }
    }

    // DELETE api/clockify-users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(ulong id)
    {
        try
        {
            var user = await _db.ClockifyUsers.FindAsync(id);
            if (user == null)
                return NotFound(new { success = false, message = "Usuario no encontrado" });

            _db.ClockifyUsers.Remove(user);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Usuario eliminado correctamente" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar clockify_user {Id}", id);
            return StatusCode(500, new { success = false, message = "Error al eliminar el usuario" });
        }
    }
}