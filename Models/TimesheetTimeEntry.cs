using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace bdt_evm_app.Models;

[Table("timesheet_time_entries")]
public class TimesheetTimeEntry
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("timesheet_time_entry_id")]
    public string ClockifyTimeEntryId { get; set; } = string.Empty;
    [Column("project_id")]
    public ulong ProjectId { get; set; }
    [Column("user_id")]
    public ulong? UserId { get; set; }
    [Column("description")]
    public string? Description { get; set; }
    [Column("start_time")]
    public DateTime StartTime { get; set; }
    [Column("end_time")]
    public DateTime EndTime { get; set; }
    [Column("duration_hours")]
    public decimal DurationHours { get; set; }
    [Column("billable")]
    public bool Billable { get; set; } = true;
    [Column("change_request_id")]
    public ulong? ChangeRequestId { get; set; }
    [Column("source_raw")]
    public string? SourceRaw { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}