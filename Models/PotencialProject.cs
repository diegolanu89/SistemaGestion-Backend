using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("potencial_projects")]
public class PotencialProject
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("code")]
    public string? Code { get; set; }
    [Column("potencial_client_id")]
    public ulong PotencialClientId { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    public PotencialClient? PotencialClient { get; set; }
    public List<PotencialProjectAllocation> Allocations { get; set; } = new();
}
