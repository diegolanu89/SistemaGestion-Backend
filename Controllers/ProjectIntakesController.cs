using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.DTOs;
using bdt_evm_app.Models;
using bdt_evm_app.Services;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/project-intakes")]
[RequirePermission("PROJECTS_CREATE")]
public class ProjectIntakesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProjectIntakeService _intakeService;
    private readonly ILogger<ProjectIntakesController> _logger;

    public ProjectIntakesController(AppDbContext db, ProjectIntakeService intakeService, ILogger<ProjectIntakesController> logger)
    {
        _db = db;
        _intakeService = intakeService;
        _logger = logger;
    }

    // GET /api/project-intakes/next-number?type=30
    [HttpGet("next-number")]
    public async Task<IActionResult> GetNextNumber([FromQuery] string type)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(type))
                return UnprocessableEntity(new { success = false, message = "El parámetro 'type' es obligatorio" });

            var typeExists = await _db.ProjectIntakeTypeRefs.AnyAsync(t => t.Code == type && t.IsActive);
            if (!typeExists)
                return UnprocessableEntity(new { success = false, message = $"Tipo de proyecto '{type}' no válido o inactivo" });

            var nextNumber = await _intakeService.GenerateInternalProjectNumberAsync(type);
            return Ok(new { success = true, data = new { nextNumber } });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener próximo número de proyecto para tipo {Type}", type);
            return StatusCode(500, new { success = false, message = "Error al calcular el próximo número", error = e.Message });
        }
    }

    // GET /api/project-intakes/options
    // Debe ir antes de {id} para que no sea tratado como un id
    [HttpGet("options")]
    public async Task<IActionResult> GetOptions()
    {
        try
        {
            var types = await _db.ProjectIntakeTypeRefs
                .Where(t => t.IsActive)
                .OrderBy(t => t.Code)
                .Select(t => new ProjectIntakeTypeRefDto
                {
                    Id = t.Id,
                    Code = t.Code,
                    Label = t.Label,
                    Description = t.Description,
                    InternalLabel = t.InternalLabel,
                    SecondaryLabel = t.SecondaryLabel,
                    RegistrationLabel = t.RegistrationLabel,
                    RequiresBusinessStatusDate = t.RequiresBusinessStatusDate,
                    RequiresActualEndDate = t.RequiresActualEndDate,
                    RequiresCommercialFields = t.RequiresCommercialFields,
                    IsActive = t.IsActive
                })
                .ToListAsync();

            var categories = await _db.ProjectIntakeCategoryRefs
                .Where(c => c.IsActive)
                .OrderBy(c => c.Label)
                .Select(c => new ProjectIntakeCategoryRefDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Label = c.Label,
                    Description = c.Description,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            var statuses = await _db.ProjectIntakeStatusRefs
                .Where(s => s.IsActive)
                .OrderBy(s => s.Label)
                .Select(s => new ProjectIntakeStatusRefDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    Label = s.Label,
                    Description = s.Description,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            var clients = await _db.TimesheetClients
                .Where(c => c.Status == "activo")
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name, c.ExternalId })
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var leaders = await _db.UserLeaders
                .Where(ul => ul.EndDate == null || ul.EndDate >= today)
                .Include(ul => ul.Leader)
                .Select(ul => new { ul.Leader!.Id, ul.Leader!.Name, ul.Leader!.Email })
                .Distinct()
                .OrderBy(u => u.Name)
                .ToListAsync();

            return Ok(new { success = true, data = new { types, categories, statuses, clients, leaders } });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener opciones de project-intakes");
            return StatusCode(500, new { success = false, message = "Error al obtener opciones", error = e.Message });
        }
    }

    // GET /api/project-intakes
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int per_page = 15,
        [FromQuery] string? project_type = null,
        [FromQuery] string? project_status_code = null,
        [FromQuery] string? is_active = null,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? category = null)
    {
        try
        {
            var query = _db.ProjectIntakeRecords
                .Include(r => r.TypeRef)
                .Include(r => r.CategoryRef)
                .Include(r => r.StatusRef)
                .Include(r => r.TimesheetProject)
                .Include(r => r.Client)
                .Include(r => r.LeaderTimesheetUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(project_type))
                query = query.Where(r => r.ProjectType == project_type);

            if (!string.IsNullOrEmpty(project_status_code))
                query = query.Where(r => r.ProjectStatusCode == project_status_code);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(r =>
                    (r.ProjectName != null && r.ProjectName.Contains(search)) ||
                    (r.ClientName != null && r.ClientName.Contains(search)) ||
                    (r.Observations != null && r.Observations.Contains(search)));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.ProjectStatusCode == status);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(r => r.CategoryCode == category);

            // Por defecto muestra solo activos; pasar is_active=false para ver dados de baja
            var showActive = is_active?.ToLower() != "false";
            query = query.Where(r => r.IsActive == showActive);

            query = query
                .OrderByDescending(r => r.CreatedAt)
                .ThenBy(r => r.InternalProjectNumber);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * per_page)
                .Take(per_page)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = items.Select(MapToDto),
                current_page = page,
                per_page,
                total,
                last_page = (int)Math.Ceiling((double)total / per_page)
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al listar project-intakes");
            return StatusCode(500, new { success = false, message = "Error al listar proyectos", error = e.Message });
        }
    }

    // GET /api/project-intakes/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(ulong id)
    {
        try
        {
            var record = await _db.ProjectIntakeRecords
                .Include(r => r.TypeRef)
                .Include(r => r.CategoryRef)
                .Include(r => r.StatusRef)
                .Include(r => r.TimesheetProject)
                .Include(r => r.Client)
                .Include(r => r.LeaderTimesheetUser)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (record == null)
                return NotFound(new { success = false, message = "Proyecto no encontrado" });

            return Ok(new { success = true, data = MapToDto(record) });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener project-intake {Id}", id);
            return StatusCode(500, new { success = false, message = "Error al obtener el proyecto", error = e.Message });
        }
    }

    // POST /api/project-intakes
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectIntakeDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.ProjectType))
                return UnprocessableEntity(new { success = false, message = "project_type es obligatorio" });

            if (string.IsNullOrWhiteSpace(dto.ProjectName))
                return UnprocessableEntity(new { success = false, message = "project_name es obligatorio" });

            var typeRef = await _db.ProjectIntakeTypeRefs
                .FirstOrDefaultAsync(t => t.Code == dto.ProjectType && t.IsActive);

            if (typeRef == null)
                return UnprocessableEntity(new { success = false, message = $"Tipo de proyecto '{dto.ProjectType}' no válido o inactivo" });

            var validationErrors = _intakeService.ValidateConditionalFields(dto, typeRef);
            if (validationErrors.Any())
                return UnprocessableEntity(new { success = false, message = "Errores de validación", errors = validationErrors });

            var userId = HttpContext.Items["UserId"] as ulong?;
            var internalNumber = await _intakeService.GenerateInternalProjectNumberAsync(dto.ProjectType);

            string? resolvedClientName = null;
            if (dto.ClientId.HasValue)
            {
                var client = await _db.TimesheetClients.FindAsync(dto.ClientId.Value);
                if (client == null)
                    return UnprocessableEntity(new { success = false, message = $"Cliente con id '{dto.ClientId}' no encontrado" });
                resolvedClientName = client.Name;
            }

            if (dto.LeaderTimesheetUserId.HasValue)
            {
                var leaderExists = await _db.TimesheetUsers.AnyAsync(u => u.Id == dto.LeaderTimesheetUserId.Value);
                if (!leaderExists)
                    return UnprocessableEntity(new { success = false, message = $"Líder con id '{dto.LeaderTimesheetUserId}' no encontrado" });
            }

            var record = new ProjectIntakeRecord
            {
                ProjectType = dto.ProjectType,
                InternalProjectNumber = internalNumber,
                SecondaryProjectNumber = dto.SecondaryProjectNumber,
                RegistrationDate = dto.RegistrationDate,
                ClientId = dto.ClientId,
                ClientName = resolvedClientName,
                ProjectName = dto.ProjectName,
                CategoryCode = dto.CategoryCode,
                ProjectStatusCode = dto.ProjectStatusCode,
                BusinessStatusDate = dto.BusinessStatusDate,
                EstimatedEndDate = dto.EstimatedEndDate,
                ActualEndDate = dto.ActualEndDate,
                CommercialStatus = dto.CommercialStatus,
                LeaderTimesheetUserId = dto.LeaderTimesheetUserId,
                Observations = dto.Observations,
                RequiresTimesheetCreation = dto.RequiresTimesheetCreation,
                IsActive = true,
                CreatedBy = userId,
                UpdatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            string? clockifyMessage = null;

            if (dto.RequiresTimesheetCreation)
            {
                try
                {
                    var clockifyName = $"{internalNumber} - {dto.ProjectName}";
                    var (clockifyRecordId, _, message) = await _intakeService.CreateInClockifyAsync(clockifyName, dto.ClientId);
                    record.TimesheetRecordId = clockifyRecordId;
                    clockifyMessage = message;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear proyecto en Clockify para intake '{ProjectName}'", dto.ProjectName);
                    return UnprocessableEntity(new
                    {
                        success = false,
                        message = "El proyecto fue validado pero falló la creación en Clockify",
                        error = ex.Message
                    });
                }
            }

            _db.ProjectIntakeRecords.Add(record);
            await _db.SaveChangesAsync();

            await _db.Entry(record).Reference(r => r.TypeRef).LoadAsync();
            await _db.Entry(record).Reference(r => r.CategoryRef).LoadAsync();
            await _db.Entry(record).Reference(r => r.StatusRef).LoadAsync();
            if (record.TimesheetRecordId.HasValue)
                await _db.Entry(record).Reference(r => r.TimesheetProject).LoadAsync();
            if (record.ClientId.HasValue)
                await _db.Entry(record).Reference(r => r.Client).LoadAsync();
            if (record.LeaderTimesheetUserId.HasValue)
                await _db.Entry(record).Reference(r => r.LeaderTimesheetUser).LoadAsync();

            return StatusCode(201, new
            {
                success = true,
                message = clockifyMessage != null
                    ? $"Proyecto creado exitosamente. Clockify: {clockifyMessage}"
                    : "Proyecto creado exitosamente",
                data = MapToDto(record)
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al crear project-intake");
            return StatusCode(500, new { success = false, message = "Error al crear el proyecto", error = e.Message });
        }
    }

    // PUT /api/project-intakes/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(ulong id, [FromBody] UpdateProjectIntakeDto dto)
    {
        try
        {
            var record = await _db.ProjectIntakeRecords.FindAsync(id);

            if (record == null)
                return NotFound(new { success = false, message = "Proyecto no encontrado" });

            if (!record.IsActive)
                return UnprocessableEntity(new { success = false, message = "No se puede modificar un proyecto dado de baja" });

            var userId = HttpContext.Items["UserId"] as ulong?;

            if (dto.SecondaryProjectNumber != null) record.SecondaryProjectNumber = dto.SecondaryProjectNumber;
            if (dto.RegistrationDate.HasValue) record.RegistrationDate = dto.RegistrationDate;
            if (dto.ClientId.HasValue)
            {
                var client = await _db.TimesheetClients.FindAsync(dto.ClientId.Value);
                if (client == null)
                    return UnprocessableEntity(new { success = false, message = $"Cliente con id '{dto.ClientId}' no encontrado" });
                record.ClientId = dto.ClientId;
                record.ClientName = client.Name;
            }
            if (dto.ProjectName != null) record.ProjectName = dto.ProjectName;
            if (dto.CategoryCode != null) record.CategoryCode = dto.CategoryCode;
            if (dto.ProjectStatusCode != null) record.ProjectStatusCode = dto.ProjectStatusCode;
            if (dto.BusinessStatusDate.HasValue) record.BusinessStatusDate = dto.BusinessStatusDate;
            if (dto.EstimatedEndDate.HasValue) record.EstimatedEndDate = dto.EstimatedEndDate;
            if (dto.ActualEndDate.HasValue) record.ActualEndDate = dto.ActualEndDate;
            if (dto.CommercialStatus != null) record.CommercialStatus = dto.CommercialStatus;
            if (dto.LeaderTimesheetUserId.HasValue)
            {
                var leaderExists = await _db.TimesheetUsers.AnyAsync(u => u.Id == dto.LeaderTimesheetUserId.Value);
                if (!leaderExists)
                    return UnprocessableEntity(new { success = false, message = $"Líder con id '{dto.LeaderTimesheetUserId}' no encontrado" });
                record.LeaderTimesheetUserId = dto.LeaderTimesheetUserId;
            }
            if (dto.Observations != null) record.Observations = dto.Observations;
            if (dto.RequiresTimesheetCreation.HasValue) record.RequiresTimesheetCreation = dto.RequiresTimesheetCreation.Value;

            string? clockifyMessage = null;

            if (dto.RequiresTimesheetCreation == true && record.TimesheetRecordId == null)
            {
                try
                {
                    var clockifyName = $"{record.InternalProjectNumber} - {record.ProjectName ?? string.Empty}";
                    var (clockifyRecordId, _, message) = await _intakeService.CreateInClockifyAsync(clockifyName, record.ClientId);
                    record.TimesheetRecordId = clockifyRecordId;
                    clockifyMessage = message;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear proyecto en Clockify durante update '{ProjectName}'", record.ProjectName);
                    return UnprocessableEntity(new
                    {
                        success = false,
                        message = "El proyecto fue validado pero falló la sincronización con Clockify",
                        error = ex.Message
                    });
                }
            }

            record.UpdatedBy = userId;
            record.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await _db.Entry(record).Reference(r => r.TypeRef).LoadAsync();
            await _db.Entry(record).Reference(r => r.CategoryRef).LoadAsync();
            await _db.Entry(record).Reference(r => r.StatusRef).LoadAsync();
            if (record.TimesheetRecordId.HasValue)
                await _db.Entry(record).Reference(r => r.TimesheetProject).LoadAsync();
            if (record.ClientId.HasValue)
                await _db.Entry(record).Reference(r => r.Client).LoadAsync();
            if (record.LeaderTimesheetUserId.HasValue)
                await _db.Entry(record).Reference(r => r.LeaderTimesheetUser).LoadAsync();

            return Ok(new
            {
                success = true,
                message = clockifyMessage != null
                    ? $"Proyecto actualizado exitosamente. Clockify: {clockifyMessage}"
                    : "Proyecto actualizado exitosamente",
                data = MapToDto(record)
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al actualizar project-intake {Id}", id);
            return StatusCode(500, new { success = false, message = "Error al actualizar el proyecto", error = e.Message });
        }
    }

    // DELETE /api/project-intakes/{id}  →  baja lógica
    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(ulong id)
    {
        try
        {
            var record = await _db.ProjectIntakeRecords.FindAsync(id);

            if (record == null)
                return NotFound(new { success = false, message = "Proyecto no encontrado" });

            if (!record.IsActive)
                return UnprocessableEntity(new { success = false, message = "El proyecto ya está dado de baja" });

            var userId = HttpContext.Items["UserId"] as ulong?;
            record.IsActive = false;
            record.UpdatedBy = userId;
            record.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Proyecto dado de baja exitosamente" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al dar de baja project-intake {Id}", id);
            return StatusCode(500, new { success = false, message = "Error al dar de baja el proyecto", error = e.Message });
        }
    }

    private static ProjectIntakeRecordDto MapToDto(ProjectIntakeRecord r) => new()
    {
        Id = r.Id,
        ProjectType = r.ProjectType,
        InternalProjectNumber = r.InternalProjectNumber,
        SecondaryProjectNumber = r.SecondaryProjectNumber,
        RegistrationDate = r.RegistrationDate,
        ClientId = r.ClientId,
        ClientName = r.Client?.Name ?? r.ClientName,
        ProjectName = r.ProjectName,
        CategoryCode = r.CategoryCode,
        ProjectStatusCode = r.ProjectStatusCode,
        BusinessStatusDate = r.BusinessStatusDate,
        EstimatedEndDate = r.EstimatedEndDate,
        ActualEndDate = r.ActualEndDate,
        CommercialStatus = r.CommercialStatus,
        LeaderTimesheetUserId = r.LeaderTimesheetUserId,
        LeaderName = r.LeaderTimesheetUser?.Name,
        Observations = r.Observations,
        RequiresTimesheetCreation = r.RequiresTimesheetCreation,
        TimesheetRecordId = r.TimesheetRecordId,
        IsActive = r.IsActive,
        CreatedBy = r.CreatedBy,
        UpdatedBy = r.UpdatedBy,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        TypeRef = r.TypeRef != null ? new ProjectIntakeTypeRefDto
        {
            Id = r.TypeRef.Id,
            Code = r.TypeRef.Code,
            Label = r.TypeRef.Label,
            Description = r.TypeRef.Description,
            InternalLabel = r.TypeRef.InternalLabel,
            SecondaryLabel = r.TypeRef.SecondaryLabel,
            RegistrationLabel = r.TypeRef.RegistrationLabel,
            RequiresBusinessStatusDate = r.TypeRef.RequiresBusinessStatusDate,
            RequiresActualEndDate = r.TypeRef.RequiresActualEndDate,
            RequiresCommercialFields = r.TypeRef.RequiresCommercialFields,
            IsActive = r.TypeRef.IsActive
        } : null,
        CategoryRef = r.CategoryRef != null ? new ProjectIntakeCategoryRefDto
        {
            Id = r.CategoryRef.Id,
            Code = r.CategoryRef.Code,
            Label = r.CategoryRef.Label,
            Description = r.CategoryRef.Description,
            IsActive = r.CategoryRef.IsActive
        } : null,
        StatusRef = r.StatusRef != null ? new ProjectIntakeStatusRefDto
        {
            Id = r.StatusRef.Id,
            Code = r.StatusRef.Code,
            Label = r.StatusRef.Label,
            Description = r.StatusRef.Description,
            IsActive = r.StatusRef.IsActive
        } : null,
        TimesheetProjectName = r.TimesheetProject?.Name
    };
}
