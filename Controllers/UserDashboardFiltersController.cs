using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/app/dashboard-filters")]
[RequirePermission("DASHBOARD_EVM_ACCESS")]
public class UserDashboardFiltersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<UserDashboardFiltersController> _logger;

    public UserDashboardFiltersController(
        AppDbContext db,
        ILogger<UserDashboardFiltersController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private ulong? GetCurrentUserId() =>
        HttpContext.Items["UserId"] as ulong?;

    // =========================================================
    // GET api/app/dashboard-filters
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetCurrentUserId();

        if (!userId.HasValue)
            return Unauthorized(new { message = "No autenticado" });

        try
        {
            var filters = await _db.UserDashboardFilters
                .Where(f => f.UserId == userId.Value)
                .OrderBy(f => f.Name)
                .ToListAsync();

            return Ok(filters.Select(MapToDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo filtros del dashboard.");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "No fue posible obtener los filtros guardados."
            });
        }
    }

    // =========================================================
    // POST api/app/dashboard-filters
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDashboardFilterDto dto)
    {
        var userId = GetCurrentUserId();

        if (!userId.HasValue)
            return Unauthorized(new { message = "No autenticado" });

        if (string.IsNullOrWhiteSpace(dto.Name))
            return UnprocessableEntity(new
            {
                message = "El nombre es requerido"
            });

        try
        {
            var filter = new UserDashboardFilter
            {
                UserId = userId.Value,
                Name = dto.Name.Trim(),
                LeaderId = string.IsNullOrWhiteSpace(dto.LeaderId)
                    ? null
                    : dto.LeaderId,
                MonthKeys = dto.MonthKeys != null && dto.MonthKeys.Any()
                    ? JsonSerializer.Serialize(dto.MonthKeys)
                    : null,
                ProjectId = string.IsNullOrWhiteSpace(dto.ProjectId)
                    ? null
                    : dto.ProjectId,
                SourceType = dto.SourceType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.UserDashboardFilters.Add(filter);

            await _db.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, MapToDto(filter));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando filtro del dashboard.");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "No fue posible guardar el filtro."
            });
        }
    }

    // =========================================================
    // PUT api/app/dashboard-filters/{id}
    // =========================================================

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        ulong id,
        [FromBody] UpdateDashboardFilterDto dto)
    {
        var userId = GetCurrentUserId();

        if (!userId.HasValue)
            return Unauthorized(new { message = "No autenticado" });

        try
        {
            var filter = await _db.UserDashboardFilters
                .FirstOrDefaultAsync(f =>
                    f.Id == id &&
                    f.UserId == userId.Value);

            if (filter == null)
                return NotFound(new
                {
                    message = "Filtro no encontrado"
                });

            if (dto.Name != null)
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return UnprocessableEntity(new
                    {
                        message = "El nombre no puede estar vacío"
                    });
                }

                filter.Name = dto.Name.Trim();
            }

            if (dto.LeaderId != null)
            {
                filter.LeaderId = string.IsNullOrWhiteSpace(dto.LeaderId)
                    ? null
                    : dto.LeaderId;
            }

            if (dto.MonthKeys != null)
            {
                filter.MonthKeys = dto.MonthKeys.Any()
                    ? JsonSerializer.Serialize(dto.MonthKeys)
                    : null;
            }

            if (dto.ProjectId != null)
            {
                filter.ProjectId = string.IsNullOrWhiteSpace(dto.ProjectId)
                    ? null
                    : dto.ProjectId;
            }

            if (dto.SourceType != null)
            {
                filter.SourceType = dto.SourceType;
            }

            filter.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(MapToDto(filter));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando filtro del dashboard.");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "No fue posible actualizar el filtro."
            });
        }
    }

    // =========================================================
    // DELETE api/app/dashboard-filters/{id}
    // =========================================================

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(ulong id)
    {
        var userId = GetCurrentUserId();

        if (!userId.HasValue)
            return Unauthorized(new { message = "No autenticado" });

        try
        {
            var filter = await _db.UserDashboardFilters
                .FirstOrDefaultAsync(f =>
                    f.Id == id &&
                    f.UserId == userId.Value);

            if (filter == null)
            {
                return NotFound(new
                {
                    message = "Filtro no encontrado"
                });
            }

            _db.UserDashboardFilters.Remove(filter);

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Filtro eliminado"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando filtro del dashboard.");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "No fue posible eliminar el filtro."
            });
        }
    }

    // =========================================================
    // DTO MAPPER
    // =========================================================

    private static DashboardFilterDto MapToDto(UserDashboardFilter filter)
    {
        return new DashboardFilterDto
        {
            Id = filter.Id,
            Name = filter.Name,
            LeaderId = filter.LeaderId,
            MonthKeys = string.IsNullOrWhiteSpace(filter.MonthKeys)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(filter.MonthKeys)
                    ?? new List<string>(),
            ProjectId = filter.ProjectId,
            SourceType = filter.SourceType
        };
    }
}