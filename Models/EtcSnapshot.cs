using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("etc_snapshots")]
public class EtcSnapshot
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("project_id")]
    public ulong ProjectId { get; set; }
    [Column("version")]
    public int Version { get; set; }
    [Column("label")]
    public string? Label { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}