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
    public TimesheetUserResponseDto? User { get; set; }
    public TimesheetUserResponseDto? Leader { get; set; }
}

public class CreateUserLeaderDto
{
    public ulong UserId { get; set; }
    public ulong LeaderId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateUserLeaderDto
{
    public ulong? UserId { get; set; }
    public ulong? LeaderId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Notes { get; set; }
}

public class BulkUserLeaderDto
{
    public List<ulong> UserIds { get; set; } = new();
    public ulong LeaderId { get; set; }
    public DateOnly? StartDate { get; set; }
}
