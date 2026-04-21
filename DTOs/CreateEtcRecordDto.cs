namespace bdt_evm_app.DTOs;

public class CreateEtcRecordDto
{
    public List<string> Users { get; set; } = new();
    public string MonthKey { get; set; } = string.Empty;
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Hours { get; set; }
}