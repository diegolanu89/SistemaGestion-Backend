using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("user_leaders")]
public class UserLeader
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("user_id")]
    public ulong UserId { get; set; }
    [Column("leader_id")]
    public ulong LeaderId { get; set; }
    [Column("start_date")]
    public DateOnly StartDate { get; set; }
    [Column("end_date")]
    public DateOnly? EndDate { get; set; }
    [Column("notes")]
    public string? Notes { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    public TimesheetUser? User { get; set; }
    public TimesheetUser? Leader { get; set; }
}
