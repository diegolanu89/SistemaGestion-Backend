namespace bdt_evm_app.DTOs;

public class ProjectFilterDto
{
    public ulong Id { get; set; }
    public ulong ProjectId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ProjectDto? Project { get; set; }
}
