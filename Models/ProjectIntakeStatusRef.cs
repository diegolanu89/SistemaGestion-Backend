using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("project_intake_status_refs")]
public class ProjectIntakeStatusRef
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("code")]
    public string Code { get; set; } = string.Empty;
    [Column("label")]
    public string Label { get; set; } = string.Empty;
    [Column("description")]
    public string? Description { get; set; }
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
