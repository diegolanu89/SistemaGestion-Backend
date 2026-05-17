using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("project_tracking_updates")]
public class ProjectTrackingUpdate
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("project_tracking_id")]
    public ulong ProjectTrackingId { get; set; }
    [Column("change_end_date")]
    public DateOnly? ChangeEndDate { get; set; }
    [Column("observations")]
    public string? Observations { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public ProjectTracking? ProjectTracking { get; set; }
}
