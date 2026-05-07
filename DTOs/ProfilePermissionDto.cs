namespace bdt_evm_app.DTOs;

public class ProfilePermissionsResponseDto
{
    public ulong ProfileId { get; set; }
    public List<ProfilePermissionItemDto> Permissions { get; set; } = new();
}

public class ProfilePermissionItemDto
{
    public ulong PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string ModuleCode { get; set; } = string.Empty;
    public ProfilePermissionActionDto Action { get; set; } = new();
}

public class ProfilePermissionActionDto
{
    public ulong Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public byte Level { get; set; }
}

public class SyncProfilePermissionsDto
{
    public List<ProfilePermissionAssignmentDto> Permissions { get; set; } = new();
}

public class ProfilePermissionAssignmentDto
{
    public ulong PermissionId { get; set; }
    public ulong ActionId { get; set; }
}
