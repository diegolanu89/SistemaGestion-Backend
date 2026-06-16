namespace bdt_evm_app.DTOs;

public class EtcEntryDto
{
    public string UserName { get; set; } = string.Empty;
    public string MonthKey { get; set; } = string.Empty;
    public string? MonthLabel { get; set; }
    public decimal Hours { get; set; }
}

public class EtcUserDto
{
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
}

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
    public EtcUserDto? User { get; set; }
}

public class CreateEtcRecordDto
{
    public List<string> Users { get; set; } = new();
    public string MonthKey { get; set; } = string.Empty;
    public string? MonthLabel { get; set; }
    public decimal Hours { get; set; }
}

public class UpdateEtcRecordDto
{
    public string UserName { get; set; } = string.Empty;
    public string MonthKey { get; set; } = string.Empty;
    public string? MonthLabel { get; set; }
    public decimal Hours { get; set; }
}

public class BulkEtcDto
{
    public ulong ProjectId { get; set; }
    public List<EtcEntryDto> Entries { get; set; } = new();
}

public class CreateSnapshotDto
{
    public List<EtcEntryDto> Entries { get; set; } = new();
}

public class ValidateEtcCapacityDto
{
    public ulong ProjectId { get; set; }
    public List<EtcEntryDto> Entries { get; set; } = new();
}

public class EtcResourceRowDto
{
    public ulong? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Dictionary<string, decimal> HoursByMonth { get; set; } = new();
    public decimal Total { get; set; }
}

public class EtcSummaryDto
{
    public object? Snapshot { get; set; }
    public List<string> Months { get; set; } = new();
    public List<EtcResourceRowDto> Resources { get; set; } = new();
    public Dictionary<string, decimal> TotalsByMonth { get; set; } = new();
    public decimal GrandTotal { get; set; }
}
