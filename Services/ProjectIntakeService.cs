using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;

namespace bdt_evm_app.Services;

public class ProjectIntakeService
{
    private readonly AppDbContext _db;
    private readonly ClockifyService _clockify;
    private readonly ILogger<ProjectIntakeService> _logger;

    public ProjectIntakeService(AppDbContext db, ClockifyService clockify, ILogger<ProjectIntakeService> logger)
    {
        _db = db;
        _clockify = clockify;
        _logger = logger;
    }

    // Genera el próximo internal_project_number para un tipo dado (ej: "30" → "30.006")
    public async Task<string> GenerateInternalProjectNumberAsync(string projectType)
    {
        var existing = await _db.ProjectIntakeRecords
            .Where(r => r.ProjectType == projectType && r.InternalProjectNumber != null)
            .Select(r => r.InternalProjectNumber!)
            .ToListAsync();

        var maxCorrelative = existing
            .Select(n => {
                var parts = n.Split('.');
                return parts.Length == 2 && int.TryParse(parts[1], out var num) ? num : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        return $"{projectType}.{(maxCorrelative + 1):D3}";
    }

    // Valida campos condicionales según los flags del tipo de proyecto
    public List<string> ValidateConditionalFields(CreateProjectIntakeDto dto, ProjectIntakeTypeRef typeRef)
    {
        var errors = new List<string>();

        if (typeRef.RequiresBusinessStatusDate && dto.BusinessStatusDate == null)
            errors.Add($"business_status_date es obligatorio para el tipo '{typeRef.Label}'");

        if (typeRef.RequiresActualEndDate && dto.ActualEndDate == null)
            errors.Add($"actual_end_date es obligatorio para el tipo '{typeRef.Label}'");

        if (typeRef.RequiresCommercialFields && string.IsNullOrWhiteSpace(dto.CommercialStatus))
            errors.Add($"commercial_status es obligatorio para el tipo '{typeRef.Label}'");

        return errors;
    }

    // Crea el proyecto en Clockify y lo registra en timesheet_projects
    // Retorna el id interno (bigint) del registro creado en timesheet_projects
    public async Task<(ulong clockifyRecordId, string clockifyExternalId, string message)> CreateInClockifyAsync(string projectName, ulong? clientId)
    {
        string? clientExternalId = null;

        if (clientId.HasValue)
        {
            var client = await _db.TimesheetClients.FindAsync(clientId.Value);

            clientExternalId = client?.ExternalId;
        }

        var externalProject = await _clockify.CreateProjectAsync(projectName, clientExternalId);

        if (!externalProject.TryGetProperty("id", out var idProp))
            throw new Exception("Clockify no devolvió un id de proyecto válido");

        var externalId = idProp.GetString()
            ?? throw new Exception("Clockify devolvió un id vacío");

        var existing = await _db.TimesheetProjects
            .FirstOrDefaultAsync(p => p.TimesheetProjectId == externalId);

        if (existing != null)
            return (existing.Id, externalId, "Proyecto vinculado a registro existente en Clockify");

        var parts = projectName.Split(" - ", 2);

        var projectCode = parts.Length > 1
            ? parts[0].Trim()
            : null;

        var projectDisplayName = parts.Length > 1
            ? parts[1].Trim()
            : projectName;

        var timesheetProject = new TimesheetProject
        {
            TimesheetProjectId = externalId,

            Code = projectCode,

            Name = projectDisplayName,

            Status = "activo",

            CreatedAt = DateTime.UtcNow,

            UpdatedAt = DateTime.UtcNow
        };

        _db.TimesheetProjects.Add(timesheetProject);

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Proyecto '{Name}' creado en Clockify con id externo {ExternalId}",
            projectName,
            externalId);

        return (
            timesheetProject.Id,
            externalId,
            "Proyecto creado exitosamente en Clockify"
        );
    }
}
