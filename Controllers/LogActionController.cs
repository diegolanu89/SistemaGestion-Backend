using Microsoft.AspNetCore.Mvc;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/log-action")]
public class LogActionController : ControllerBase
{
    private readonly ILogger<LogActionController> _logger;

    public LogActionController(ILogger<LogActionController> logger)
    {
        _logger = logger;
    }

    // POST api/log-action
    [HttpPost]
    public IActionResult Store([FromBody] LogActionDto dto)
    {
        var level = dto.Level?.ToLower() switch
        {
            "error" or "warning" => dto.Level.ToLower(),
            _ => "info"
        };

        var context = dto.Context?
            .Where(kv => kv.Value == null || kv.Value is string or bool or int or long or double or float or decimal)
            .ToDictionary(kv => kv.Key, kv => kv.Value) ?? new();

        context["source"] = "frontend";

        var message = $"[frontend:{dto.Action ?? "frontend_action"}] {System.Text.Json.JsonSerializer.Serialize(context)}";

        if (level == "error")
            _logger.LogError(message);
        else if (level == "warning")
            _logger.LogWarning(message);
        else
            _logger.LogInformation(message);

        return StatusCode(201, new { ok = true });
    }
}

public class LogActionDto
{
    public string? Action { get; set; }
    public string? Level { get; set; }
    public Dictionary<string, object?>? Context { get; set; }
}
