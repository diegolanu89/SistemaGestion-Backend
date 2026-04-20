using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("potencial_clients")]
public class PotencialClient
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}