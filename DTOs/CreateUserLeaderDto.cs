namespace bdt_evm_app.DTOs;

public class CreateUserLeaderDto
{
    public ulong UserId { get; set; }
    public ulong LeaderId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Notes { get; set; }
}