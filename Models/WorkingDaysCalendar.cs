using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("working_days_calendar")]
public class WorkingDaysCalendar
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("month_key")]
    public string MonthKey { get; set; } = string.Empty;
    [Column("month_label")]
    public string MonthLabel { get; set; } = string.Empty;
    [Column("year")]
    public int Year { get; set; }
    [Column("month")]
    public int Month { get; set; }
    [Column("total_days")]
    public int TotalDays { get; set; }
    [Column("working_days")]
    public int WorkingDays { get; set; }
    [Column("hours_month")]
    public decimal HoursMonth { get; set; }
    [Column("holiday_days")]
    public int HolidayDays { get; set; }
    [Column("holidays_list")]
    public string? HolidaysList { get; set; }
    [Column("notes")]
    public string? Notes { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}