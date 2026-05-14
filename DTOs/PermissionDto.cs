namespace bdt_evm_app.DTOs;

public class PermissionDto
{
    public ulong Id { get; set; }
    public ulong ModuleId { get; set; }
    public string ModuleCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Active { get; set; }
}

public class UpdatePermissionModuleDto
{
    public ulong ModuleId { get; set; }
}
