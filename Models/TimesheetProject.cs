using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("timesheet_projects")]
public class TimesheetProject
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("timesheet_project_id")]
    public string TimesheetProjectId { get; set; } = string.Empty;
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("code")]
    public string? Code { get; set; }
    [Column("client_id")]
    public ulong? ClientId { get; set; }
    [Column("status")]
    public string Status { get; set; } = "activo";
    [Column("bac_base_hours")]
    public decimal BacBaseHours { get; set; }
    [Column("bac_base_cost")]
    public decimal BacBaseCost { get; set; }
    [Column("bac_total_hours")]
    public decimal BacTotalHours { get; set; }
    [Column("bac_total_cost")]
    public decimal BacTotalCost { get; set; }
    [Column("hourly_rate")]
    public decimal HourlyRate { get; set; }
    [Column("etc_calculation_mode")]
    public string EtcCalculationMode { get; set; } = "manual";
    [Column("project_tracking_id")]
    public ulong? ProjectTrackingId { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    public TimesheetClient? Client { get; set; }
    public TimesheetProjectFilter? Filter { get; set; }
    public ProjectTracking? ProjectTracking { get; set; }
    public List<ChangeRequest> ChangeRequests { get; set; } = new();
}
