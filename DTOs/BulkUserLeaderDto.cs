namespace bdt_evm_app.DTOs;

public class BulkUserLeaderDto
{
    public List<ulong> UserIds { get; set; } = new();
    public ulong LeaderId { get; set; }
    public DateOnly? StartDate { get; set; }
}
