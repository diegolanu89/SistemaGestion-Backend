using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("change_requests")]
public class ChangeRequest
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("project_id")]
    public ulong ProjectId { get; set; }
    [Column("code")]
    public string Code { get; set; } = string.Empty;
    [Column("title")]
    public string Title { get; set; } = string.Empty;
    [Column("description")]
    public string? Description { get; set; }
    [Column("requested_by")]
    public string? RequestedBy { get; set; }
    [Column("requested_date")]
    public DateOnly RequestedDate { get; set; }
    [Column("status")]
    public string Status { get; set; } = "propuesto";
    [Column("bac_hours_increment")]
    public decimal BacHoursIncrement { get; set; }
    [Column("bac_cost_increment")]
    public decimal BacCostIncrement { get; set; }
    [Column("approved_by")]
    public string? ApprovedBy { get; set; }
    [Column("approved_date")]
    public DateOnly? ApprovedDate { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}