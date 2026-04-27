namespace bdt_evm_app.DTOs;

public class ProfileDto
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateProfileDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateProfileDto
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
}