using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace bdt_evm_app.Models;

[Table("timesheet_users")]
public class TimesheetUser
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("timesheet_user_id")]
    public string? ClockifyUserId { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("email")]
    public string? Email { get; set; }
    [Column("role")]
    public string? Role { get; set; }
    [Column("active")]
    public bool Active { get; set; } = true;
    [Column("default_month_hours")]
    public decimal? DefaultMonthHours { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
