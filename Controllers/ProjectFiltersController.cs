using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/project-filters")]
[RequirePermission("PROJECTS_ACCESS")]
public class ProjectFiltersController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProjectFiltersController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/project-filters
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var filters = await _db.TimesheetProjectFilters
            .Include(f => f.Project)
                .ThenInclude(p => p!.Client)
            .OrderBy(f => f.Project!.Code == null || f.Project.Code == "" ? 1 : 0)
            .ThenBy(f => f.Project!.Code)
            .ToListAsync();

        return Ok(filters.Select(f => new ProjectFilterDto
        {
            Id = f.Id,
            ProjectId = f.ProjectId,
            CreatedAt = f.CreatedAt,
            UpdatedAt = f.UpdatedAt,
            Project = f.Project != null ? new ProjectDto
            {
                Id = f.Project.Id,
                ClockifyProjectId = f.Project.ClockifyProjectId,
                Name = f.Project.Name,
                Code = f.Project.Code,
                ClientId = f.Project.ClientId,
                ClientName = f.Project.Client?.Name,
                Status = f.Project.Status,
                BacBaseHours = f.Project.BacBaseHours,
                BacBaseCost = f.Project.BacBaseCost,
                BacTotalHours = f.Project.BacTotalHours,
                BacTotalCost = f.Project.BacTotalCost,
                HourlyRate = f.Project.HourlyRate,
                EtcCalculationMode = f.Project.EtcCalculationMode
            } : null
        }));
    }

    // POST api/project-filters
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectFilterDto dto)
    {
        if (dto.ProjectIds == null || !dto.ProjectIds.Any())
            return UnprocessableEntity(new { message = "project_ids es requerido" });

        var results = new List<TimesheetProjectFilter>();

        foreach (var projectId in dto.ProjectIds)
        {
            var projectExists = await _db.TimesheetProjects.AnyAsync(p => p.Id == projectId);
            if (!projectExists) continue;

            var existing = await _db.TimesheetProjectFilters
                .FirstOrDefaultAsync(f => f.ProjectId == projectId);

            if (existing != null)
            {
                results.Add(existing);
            }
            else
            {
                var filter = new TimesheetProjectFilter
                {
                    ProjectId = projectId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.TimesheetProjectFilters.Add(filter);
                results.Add(filter);
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Filtros creados correctamente",
            updated = results.Select(f => new { f.Id, f.ProjectId, f.CreatedAt, f.UpdatedAt })
        });
    }

    // DELETE api/project-filters/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(ulong id)
    {
        var filter = await _db.TimesheetProjectFilters.FindAsync(id);
        if (filter == null)
            return NotFound(new { message = "Filtro no encontrado" });

        _db.TimesheetProjectFilters.Remove(filter);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Filtro eliminado correctamente" });
    }
}
