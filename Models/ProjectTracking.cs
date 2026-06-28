using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("project_trackings")]
public class ProjectTracking
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("start_date")]
    public DateOnly? StartDate { get; set; }
    [Column("planned_end_date")]
    public DateOnly? PlannedEndDate { get; set; }
    [Column("actual_end_date")]
    public DateOnly? ActualEndDate { get; set; }
    [Column("implementation_date")]
    public DateOnly? ImplementationDate { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public List<ProjectTrackingUpdate> Updates { get; set; } = new();
}
