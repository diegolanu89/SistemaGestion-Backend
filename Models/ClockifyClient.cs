using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("clockify_clients")]
public class ClockifyClient
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("external_id")]
    public string? ExternalId { get; set; }
    [Column("status")]
    public string Status { get; set; } = "activo";
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
