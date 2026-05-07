namespace bdt_evm_app.DTOs;

public class ModuleDto
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Active { get; set; }
}

public class ModuleWithPermissionsDto : ModuleDto
{
    public List<ModulePermissionDto> Permissions { get; set; } = new();
}

public class ModulePermissionDto
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
}
