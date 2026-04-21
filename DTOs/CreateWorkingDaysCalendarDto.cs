namespace bdt_evm_app.DTOs;

public class CreateWorkingDaysCalendarDto
{
    public string MonthKey { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalDays { get; set; }
    public int WorkingDays { get; set; }
    public int HolidayDays { get; set; } = 0;
    public string? HolidaysList { get; set; }
    public string? Notes { get; set; }
}
