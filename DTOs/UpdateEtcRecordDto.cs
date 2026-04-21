namespace bdt_evm_app.DTOs;

public class UpdateEtcRecordDto
{
    public string UserName { get; set; } = string.Empty;
    public string MonthKey { get; set; } = string.Empty;
    public string MonthLabel { get; set; } = string.Empty;
    public decimal Hours { get; set; }
}
