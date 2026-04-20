using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("clockify_projects")]
public class ClockifyProject
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("clockify_project_id")]
    public string ClockifyProjectId { get; set; } = string.Empty;
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("code")]
    public string? Code { get; set; }
    [Column("client_id")]
    public ulong? ClientId { get; set; }
    [Column("status")]
    public string Status { get; set; } = "activo";
    [Column("start_date")]
    public DateOnly? StartDate { get; set; }
    [Column("end_date_planned")]
    public DateOnly? EndDatePlanned { get; set; }
    [Column("end_date_actual")]
    public DateOnly? EndDateActual { get; set; }
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
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    public ClockifyClient? Client { get; set; }
    public ClockifyProjectFilter? Filter { get; set; }
    public List<ChangeRequest> ChangeRequests { get; set; } = new();
}
