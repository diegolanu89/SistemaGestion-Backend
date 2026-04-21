namespace bdt_evm_app.DTOs;

public class UserLeaderDto
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public ulong LeaderId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ClockifyUserDto? User { get; set; }
    public ClockifyUserDto? Leader { get; set; }
}

public class ClockifyUserDto
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
}