using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("user_vacation_periods")]
public class UserVacationPeriod
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("user_id")]
    public ulong UserId { get; set; }
    [Column("date_from")]
    public DateOnly DateFrom { get; set; }
    [Column("date_to")]
    public DateOnly DateTo { get; set; }
    [Column("total_days")]
    public int TotalDays { get; set; }
    [Column("notes")]
    public string? Notes { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    public TimesheetUser? User { get; set; }
}
