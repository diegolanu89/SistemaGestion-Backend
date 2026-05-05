namespace bdt_evm_app.DTOs;

public class ActionDto
{
    public ulong Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public byte Level { get; set; }
    public bool Active { get; set; }
}
