using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("personal_access_tokens")]
public class PersonalAccessToken
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("tokenable_type")]
    public string TokenableType { get; set; } = string.Empty;
    [Column("tokenable_id")]
    public ulong TokenableId { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("token")]
    public string Token { get; set; } = string.Empty;
    [Column("abilities")]
    public string? Abilities { get; set; }
    [Column("last_used_at")]
    public DateTime? LastUsedAt { get; set; }
    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}