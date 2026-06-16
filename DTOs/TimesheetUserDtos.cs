namespace bdt_evm_app.DTOs;

public class TimesheetUserResponseDto
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public class CreateTimesheetUserDto
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool Active { get; set; } = true;
    public string? Role { get; set; }
    public decimal? DefaultMonthHours { get; set; }
}

public class UpdateTimesheetUserDto
{
    public string? TimesheetUserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool? Active { get; set; }
    public string? Role { get; set; }
    public decimal? DefaultMonthHours { get; set; }
}
