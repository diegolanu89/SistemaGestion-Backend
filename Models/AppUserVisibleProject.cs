using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("app_user_visible_projects")]
public class AppUserVisibleProject
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("user_id")]
    public ulong UserId { get; set; }
    [Column("project_id")]
    public ulong ProjectId { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    public ClockifyProject? Project { get; set; }
}
