namespace bdt_evm_app.DTOs;

public class CreateClockifyUserDto
{
    public string? ClockifyUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool Active { get; set; } = true;
    public string? Role { get; set; }
    public decimal? DefaultMonthHours { get; set; }
}
