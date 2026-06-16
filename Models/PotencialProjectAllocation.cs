using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("potencial_project_allocations")]
public class PotencialProjectAllocation
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("potencial_project_id")]
    public ulong PotencialProjectId { get; set; }
    [Column("month_key")]
    public string MonthKey { get; set; } = string.Empty;
    [Column("month_label")]
    public string? MonthLabel { get; set; }
    [Column("user_id")]
    public ulong? UserId { get; set; }
    [Column("user_name")]
    public string? UserName { get; set; }
    [Column("hours")]
    public decimal Hours { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    public TimesheetUser? User { get; set; }
}