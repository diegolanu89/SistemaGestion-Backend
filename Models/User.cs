using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace bdt_evm_app.Models;

[Table("users")]
public class User
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("email")]
    public string Email { get; set; } = string.Empty;
    [Column("email_verified_at")]
    public DateTime? EmailVerifiedAt { get; set; }
    [JsonIgnore]
    [Column("password")]
    public string Password { get; set; } = string.Empty;
    [Column("profile_id")]
    public ulong? ProfileId { get; set; }
    [Column("active")]
    public bool Active { get; set; }
    [Column("remember_token")]
    public string? RememberToken { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    public Profile? Profile { get; set; }
}
