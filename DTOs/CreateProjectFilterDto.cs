namespace bdt_evm_app.DTOs;

public class CreateProjectFilterDto
{
    public List<ulong> ProjectIds { get; set; } = new();
}