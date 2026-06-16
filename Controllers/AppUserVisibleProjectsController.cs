using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/app/my-visible-projects")]
[RequirePermission("PROJECTS_ASSIGN")]
public class AppUserVisibleProjectsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppUserVisibleProjectsController(AppDbContext db)
    {
        _db = db;
    }

    private ulong? GetCurrentUserId() =>
        HttpContext.Items["UserId"] as ulong?;

    // GET api/app/my-visible-projects
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "No autenticado" });

        var rows = await _db.AppUserVisibleProjects
            .Include(v => v.Project)
                .ThenInclude(p => p!.Client)
            .Where(v => v.UserId == userId.Value)
            .OrderBy(v => v.ProjectId)
            .ToListAsync();

        return Ok(rows.Select(row => new
        {
            id = row.Id,
            project_id = row.ProjectId,
            project = row.Project != null ? new
            {
                id = row.Project.Id,
                name = row.Project.Name,
                code = row.Project.Code,
                status = row.Project.Status,
                client = row.Project.Client != null ? new { name = row.Project.Client.Name } : null,
                client_name = row.Project.Client?.Name
            } : null
        }));
    }

    // POST api/app/my-visible-projects
    [HttpPost]
    public async Task<IActionResult> Update([FromBody] UpdateVisibleProjectsDto dto)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "No autenticado" });

        if (dto?.ProjectIds == null)
            return UnprocessableEntity(new { message = "project_ids es requerido" });

        var existingIds = await _db.TimesheetProjects
            .Where(p => dto.ProjectIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        var validIds = dto.ProjectIds.Intersect(existingIds).Distinct().ToList();

        var current = await _db.AppUserVisibleProjects
            .Where(v => v.UserId == userId.Value)
            .ToListAsync();

        _db.AppUserVisibleProjects.RemoveRange(current);

        foreach (var pid in validIds)
        {
            _db.AppUserVisibleProjects.Add(new AppUserVisibleProject
            {
                UserId = userId.Value,
                ProjectId = pid,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "Proyectos actualizados", project_ids = validIds });
    }
}
