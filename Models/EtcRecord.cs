using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("etc_records")]
public class EtcRecord
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("project_id")]
    public ulong ProjectId { get; set; }
    [Column("snapshot_id")]
    public ulong? SnapshotId { get; set; }
    [Column("user_id")]
    public ulong? UserId { get; set; }
    [Column("user_name")]
    public string? UserName { get; set; }
    [Column("month_key")]
    public string MonthKey { get; set; } = string.Empty;
    [Column("month_label")]
    public string MonthLabel { get; set; } = string.Empty;
    [Column("hours")]
    public decimal Hours { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}