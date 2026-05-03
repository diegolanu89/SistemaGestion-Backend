using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("actions")]
public class PermissionAction
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("code")]
    public string Code { get; set; } = string.Empty;
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("description")]
    public string? Description { get; set; }
    [Column("level")]
    public byte Level { get; set; }
    [Column("active")]
    public bool Active { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
