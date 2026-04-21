namespace bdt_evm_app.DTOs;

public class EtcRecordDto
{
    public ulong Id { get; set; }
    public ulong ProjectId { get; set; }
    public ulong? SnapshotId { get; set; }
    public ulong? UserId { get; set; }
    public string? UserName { get; set; }
    public string MonthKey { get; set; } = string.Empty;
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}