using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace bdt_evm_app.Models;

[Table("profiles")]
public class Profile
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("code")]
    public string Code { get; set; } = string.Empty;
    [Column("description")]
    public string? Description { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}