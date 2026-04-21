namespace bdt_evm_app.DTOs;

public class UpdateClockifyUserDto
{
    public string? ClockifyUserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool? Active { get; set; }
    public string? Role { get; set; }
    public decimal? DefaultMonthHours { get; set; }
}