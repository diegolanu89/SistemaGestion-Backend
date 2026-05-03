using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("profile_permissions")]
public class ProfilePermission
{
    [Column("profile_id")]
    public ulong ProfileId { get; set; }
    [Column("permission_id")]
    public ulong PermissionId { get; set; }
    [Column("action_id")]
    public ulong ActionId { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    public Profile? Profile { get; set; }
    public Permission? Permission { get; set; }
    public PermissionAction? Action { get; set; }
}
