using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("user_dashboard_filters")]
public class UserDashboardFilter
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("user_id")]
    public ulong UserId { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("leader_id")]
    public string? LeaderId { get; set; }
    [Column("month_keys")]
    public string? MonthKeys { get; set; }
    [Column("project_id")]
    public string? ProjectId { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}