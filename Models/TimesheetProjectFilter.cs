using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("clockify_project_filters")]
public class TimesheetProjectFilter
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("project_id")]
    public ulong ProjectId { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    public TimesheetProject? Project { get; set; }
}