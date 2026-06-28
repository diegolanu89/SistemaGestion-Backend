namespace bdt_evm_app.DTOs;

public class DashboardFilterDto
{
    public ulong Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? LeaderId { get; set; }

    public List<string> MonthKeys { get; set; } = new();

    public string? ProjectId { get; set; }

    public string? SourceType { get; set; }
}

public class CreateDashboardFilterDto
{
    public string Name { get; set; } = string.Empty;

    public string? LeaderId { get; set; }

    public List<string>? MonthKeys { get; set; }

    public string? ProjectId { get; set; }

    public string? SourceType { get; set; }
}

public class UpdateDashboardFilterDto
{
    public string? Name { get; set; }

    public string? LeaderId { get; set; }

    public List<string>? MonthKeys { get; set; }

    public string? ProjectId { get; set; }

    public string? SourceType { get; set; }
}