using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/user-leaders")]
[RequirePermission("ADMIN_ACCESS")]
public class UserLeadersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<UserLeadersController> _logger;

    public UserLeadersController(AppDbContext db, ILogger<UserLeadersController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET api/user-leaders
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] ulong? user_id,
        [FromQuery] ulong? leader_id,
        [FromQuery] string? active,
        [FromQuery] string sort_by = "start_date",
        [FromQuery] string sort_order = "desc",
        [FromQuery] int per_page = 15,
        [FromQuery] int page = 1)
    {
        try
        {
            var query = _db.UserLeaders
                .Include(ul => ul.User)
                .Include(ul => ul.Leader)
                .AsQueryable();

            if (user_id.HasValue)
                query = query.Where(ul => ul.UserId == user_id.Value);

            if (leader_id.HasValue)
                query = query.Where(ul => ul.LeaderId == leader_id.Value);

            if (active == "true")
                query = query.Where(ul => ul.EndDate == null);
            else if (active == "false")
                query = query.Where(ul => ul.EndDate != null);

            query = sort_by switch
            {
                "start_date" => sort_order == "asc" ? query.OrderBy(ul => ul.StartDate) : query.OrderByDescending(ul => ul.StartDate),
                "end_date" => sort_order == "asc" ? query.OrderBy(ul => ul.EndDate) : query.OrderByDescending(ul => ul.EndDate),
                _ => query.OrderByDescending(ul => ul.StartDate)
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
            _logger.LogError(e, "Error al listar user_leaders");
            return StatusCode(500, new { success = false, message = "Error al obtener los registros", error = e.Message });
        }
    }

    // GET api/user-leaders/options
    [HttpGet("options")]
    public async Task<IActionResult> GetOptions()
    {
        try
        {
            var users = await _db.TimesheetUsers
                .OrderBy(u => u.Name)
                .Select(u => new ClockifyUserDto
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
            return StatusCode(500, new { success = false, message = "Error al obtener las opciones", error = e.Message });
        }
    }

    // GET api/user-leaders/user/{userId}/current
    [HttpGet("user/{userId}/current")]
    public async Task<IActionResult> GetCurrentLeader(ulong userId)
    {
        try
        {
            var leader = await _db.UserLeaders
                .Include(ul => ul.Leader)
                .Where(ul => ul.UserId == userId && ul.EndDate == null)
                .OrderByDescending(ul => ul.StartDate)
                .FirstOrDefaultAsync();

            return Ok(new { success = true, data = leader != null ? MapToDto(leader) : null });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, message = "Error al obtener el líder", error = e.Message });
        }
    }

    // GET api/user-leaders/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(ulong id)
    {
        try
        {
            var leader = await _db.UserLeaders
                .Include(ul => ul.User)
                .Include(ul => ul.Leader)
                .FirstOrDefaultAsync(ul => ul.Id == id);

            if (leader == null)
                return NotFound(new { success = false, message = "Registro no encontrado" });

            return Ok(new { success = true, data = MapToDto(leader) });
        }
        catch (Exception e)
        {
            return StatusCode(500, new { success = false, message = "Error al obtener el registro", error = e.Message });
        }
    }

    // POST api/user-leaders
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserLeaderDto dto)
    {
        try
        {
            if (dto.UserId == dto.LeaderId)
                return UnprocessableEntity(new { success = false, message = "El usuario y el líder no pueden ser el mismo" });

            var userExists = await _db.TimesheetUsers.AnyAsync(u => u.Id == dto.UserId);
            var leaderExists = await _db.TimesheetUsers.AnyAsync(u => u.Id == dto.LeaderId);

            if (!userExists || !leaderExists)
                return UnprocessableEntity(new { success = false, message = "Usuario o líder no encontrado" });

            // Cerrar relación activa anterior si no tiene end_date
            if (!dto.EndDate.HasValue)
            {
                var activeRelations = await _db.UserLeaders
                    .Where(ul => ul.UserId == dto.UserId && ul.EndDate == null)
                    .ToListAsync();

                foreach (var rel in activeRelations)
                {
                    rel.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
                    rel.UpdatedAt = DateTime.UtcNow;
                }
            }

            var userLeader = new UserLeader
            {
                UserId = dto.UserId,
                LeaderId = dto.LeaderId,
                StartDate = dto.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = dto.EndDate,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.UserLeaders.Add(userLeader);
            await _db.SaveChangesAsync();

            await _db.Entry(userLeader).Reference(ul => ul.User).LoadAsync();
            await _db.Entry(userLeader).Reference(ul => ul.Leader).LoadAsync();

            return StatusCode(201, new
            {
                success = true,
                message = "Relación creada exitosamente",
                data = MapToDto(userLeader)
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al crear user_leader");
            return StatusCode(500, new { success = false, message = "Error al crear la relación", error = e.Message });
        }
    }

    // POST api/user-leaders/bulk
    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulk([FromBody] BulkUserLeaderDto dto)
    {
        try
        {
            if (dto.UserIds == null || !dto.UserIds.Any())
                return UnprocessableEntity(new { success = false, message = "user_ids es requerido" });

            var leaderExists = await _db.TimesheetUsers.AnyAsync(u => u.Id == dto.LeaderId);
            if (!leaderExists)
                return UnprocessableEntity(new { success = false, message = "Líder no encontrado" });

            var created = new List<UserLeader>();
            var startDate = dto.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

            foreach (var userId in dto.UserIds)
            {
                if (userId == dto.LeaderId) continue;

                var activeRelations = await _db.UserLeaders
                    .Where(ul => ul.UserId == userId && ul.EndDate == null)
                    .ToListAsync();

                foreach (var rel in activeRelations)
                {
                    rel.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
                    rel.UpdatedAt = DateTime.UtcNow;
                }

                var userLeader = new UserLeader
                {
                    UserId = userId,
                    LeaderId = dto.LeaderId,
                    StartDate = startDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.UserLeaders.Add(userLeader);
                created.Add(userLeader);
            }

            await _db.SaveChangesAsync();

            foreach (var ul in created)
            {
                await _db.Entry(ul).Reference(u => u.User).LoadAsync();
                await _db.Entry(ul).Reference(u => u.Leader).LoadAsync();
            }

            return StatusCode(201, new
            {
                success = true,
                message = $"{created.Count} relación(es) creada(s) exitosamente",
                data = created.Select(MapToDto)
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al crear user_leaders bulk");
            return StatusCode(500, new { success = false, message = "Error al crear la relación", error = e.Message });
        }
    }

    // PUT api/user-leaders/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateUserLeaderDto dto)
    {
        try
        {
            var userLeader = await _db.UserLeaders.FindAsync(id);
            if (userLeader == null)
                return NotFound(new { success = false, message = "Registro no encontrado" });

            if (dto.UserId.HasValue && dto.LeaderId.HasValue && dto.UserId == dto.LeaderId)
                return UnprocessableEntity(new { success = false, message = "El usuario y el líder no pueden ser el mismo" });

            if (dto.UserId.HasValue) userLeader.UserId = dto.UserId.Value;
            if (dto.LeaderId.HasValue) userLeader.LeaderId = dto.LeaderId.Value;
            if (dto.StartDate.HasValue) userLeader.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) userLeader.EndDate = dto.EndDate;
            if (dto.Notes != null) userLeader.Notes = dto.Notes;
            userLeader.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _db.Entry(userLeader).Reference(ul => ul.User).LoadAsync();
            await _db.Entry(userLeader).Reference(ul => ul.Leader).LoadAsync();

            return Ok(new
            {
                success = true,
                message = "Relación actualizada exitosamente",
                data = MapToDto(userLeader)
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al actualizar user_leader {Id}", id);
            return StatusCode(500, new { success = false, message = "Error al actualizar la relación", error = e.Message });
        }
    }

    // DELETE api/user-leaders/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(ulong id)
    {
        try
        {
            var userLeader = await _db.UserLeaders.FindAsync(id);
            if (userLeader == null)
                return NotFound(new { success = false, message = "Registro no encontrado" });

            _db.UserLeaders.Remove(userLeader);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Relación eliminada exitosamente" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar user_leader {Id}", id);
            return StatusCode(500, new { success = false, message = "Error al eliminar la relación", error = e.Message });
        }
    }

    private static UserLeaderDto MapToDto(UserLeader ul) => new()
    {
        Id = ul.Id,
        UserId = ul.UserId,
        LeaderId = ul.LeaderId,
        StartDate = ul.StartDate,
        EndDate = ul.EndDate,
        Notes = ul.Notes,
        CreatedAt = ul.CreatedAt,
        UpdatedAt = ul.UpdatedAt,
        User = ul.User != null ? new ClockifyUserDto
        {
            Id = ul.User.Id,
            Name = ul.User.Name,
            Email = ul.User.Email
        } : null,
        Leader = ul.Leader != null ? new ClockifyUserDto
        {
            Id = ul.Leader.Id,
            Name = ul.Leader.Name,
            Email = ul.Leader.Email
        } : null
    };
}