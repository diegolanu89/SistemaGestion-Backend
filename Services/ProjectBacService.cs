using bdt_evm_app.Data;
using bdt_evm_app.Models;
using Microsoft.EntityFrameworkCore;

namespace bdt_evm_app.Services;

public class ProjectBacService
{
    private readonly AppDbContext _db;

    public ProjectBacService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ClockifyProject> RecalculateTotal(ClockifyProject project)
    {
        var increments = await _db.ChangeRequests
            .Where(cr => cr.ProjectId == project.Id &&
                         cr.BacHoursIncrement > 0)
            .GroupBy(cr => cr.ProjectId)
            .Select(g => new
            {
                HoursInc = g.Sum(cr => cr.BacHoursIncrement),
                CostInc = g.Sum(cr => cr.BacCostIncrement)
            })
            .FirstOrDefaultAsync();

        var hoursInc = increments?.HoursInc ?? 0;
        var costInc = increments?.CostInc ?? 0;

        var newTotalHours = project.BacBaseHours + hoursInc;
        var newTotalCost = project.BacBaseCost + costInc;

        // Solo persistir si el total realmente cambió. RecalculateTotal se
        // invoca también desde lecturas (GetAll, GetById); sin este guard,
        // el bump incondicional de UpdatedAt deja la entidad en Modified y el
        // AuditSaveChangesInterceptor escribe una fila UPDATE espuria por cada
        // GET. Como total = base + incrementos, comparar el total cubre además
        // los cambios de base que llegan vía UpdateBaseAndRecalculate.
        if (project.BacTotalHours != newTotalHours || project.BacTotalCost != newTotalCost)
        {
            project.BacTotalHours = newTotalHours;
            project.BacTotalCost = newTotalCost;
            project.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return project;
    }

    public async Task<ClockifyProject> UpdateBaseAndRecalculate(ClockifyProject project, decimal? bacBaseHours, decimal? bacBaseCost)
    {
        if (bacBaseHours.HasValue) project.BacBaseHours = bacBaseHours.Value;
        if (bacBaseCost.HasValue) project.BacBaseCost = bacBaseCost.Value;
        return await RecalculateTotal(project);
    }
}
