using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("modules")]
public class Module
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("code")]
    public string Code { get; set; } = string.Empty;
    [Column("description")]
    public string? Description { get; set; }
    [Column("active")]
    public bool Active { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
