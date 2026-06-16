namespace bdt_evm_app.DTOs;

public class WorkingDaysCalendarDto
{
    public ulong Id { get; set; }
    public string MonthKey { get; set; } = string.Empty;
    public string MonthLabel { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalDays { get; set; }
    public int WorkingDays { get; set; }
    public decimal HoursMonth { get; set; }
    public int HolidayDays { get; set; }
    public string? HolidaysList { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class MonthOptionDto
{
    public string MonthKey { get; set; } = string.Empty;
    public string MonthLabel { get; set; } = string.Empty;
    public int WorkingDays { get; set; }
    public decimal HoursMonth { get; set; }
}

public class CreateWorkingDaysCalendarDto
{
    public string MonthKey { get; set; } = string.Empty;
    public string? MonthLabel { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalDays { get; set; }
    public int WorkingDays { get; set; }
    public int HolidayDays { get; set; } = 0;
    public string? HolidaysList { get; set; }
    public string? Notes { get; set; }
}

public class UpdateWorkingDaysCalendarDto
{
    public string? MonthKey { get; set; }
    public string? MonthLabel { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? TotalDays { get; set; }
    public int? WorkingDays { get; set; }
    public int? HolidayDays { get; set; }
    public string? HolidaysList { get; set; }
    public string? Notes { get; set; }
}
