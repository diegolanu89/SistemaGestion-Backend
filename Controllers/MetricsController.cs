using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.Services;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/projects")]
[RequirePermission("DASHBOARD_EVM_ACCESS")]
public class MetricsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProjectMetricsService _metricsService;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(AppDbContext db, ProjectMetricsService metricsService, ILogger<MetricsController> logger)
    {
        _db = db;
        _metricsService = metricsService;
        _logger = logger;
    }

    // GET api/projects/{id}/metrics
    [HttpGet("{id}/metrics")]
    public async Task<IActionResult> ProjectMetrics(ulong id, [FromQuery] string? from, [FromQuery] string? to)
    {
        try
        {
            var project = await _db.TimesheetProjects.FindAsync(id);
            if (project == null)
                return NotFound(new { message = "Proyecto no encontrado" });

            var metrics = await _metricsService.CalculateEVM(project, from, to);
            return Ok(metrics);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al calcular métricas del proyecto {Id}", id);
            return StatusCode(500, new { error = "Error al calcular métricas", message = e.Message });
        }
    }

    // GET api/projects/metrics/batch
    [HttpGet("metrics/batch")]
    public async Task<IActionResult> BatchMetrics([FromQuery] string project_ids)
    {
        if (string.IsNullOrEmpty(project_ids))
            return BadRequest(new { error = "El parámetro project_ids es requerido" });

        var ids = project_ids
            .Split(',')
            .Select(s => ulong.TryParse(s.Trim(), out var n) ? n : 0)
            .Where(n => n > 0)
            .ToList();

        if (!ids.Any())
            return BadRequest(new { error = "No se proporcionaron IDs válidos" });

        if (ids.Count > 100)
            return BadRequest(new { error = "Máximo 100 proyectos por llamada" });

        try
        {
            var projects = await _db.TimesheetProjects
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            if (!projects.Any())
                return Ok(new { metrics = Array.Empty<object>() });

            var metrics = new List<object>();
            foreach (var project in projects)
            {
                try
                {
                    var m = await _metricsService.CalculateEVM(project);
                    metrics.Add(m);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error calculando métricas para proyecto {Id}", project.Id);
                    metrics.Add(new
                    {
                        project_id = project.Id,
                        project_name = project.Name,
                        error = "Error al calcular métricas"
                    });
                }
            }

            return Ok(new { metrics });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error en batch metrics");
            return StatusCode(500, new { error = "Error al calcular métricas", message = e.Message });
        }
    }
}