using System.Text;
using System.Text.Json;
using bdt_evm_app.Models;
using bdt_evm_app.Data;
using Microsoft.EntityFrameworkCore;

namespace bdt_evm_app.Services;

public class ClockifyService
{
    private readonly HttpClient _client;
    private readonly HttpClient _reportsClient;
    private readonly string _workspaceId;
    private readonly string _userId;
    private readonly ILogger<ClockifyService> _logger;

    public ClockifyService(IConfiguration config, ILogger<ClockifyService> logger)
    {
        _logger = logger;
        var apiKey = config["Clockify:ApiKey"] ?? throw new Exception("CLOCKIFY_API_KEY no configurado");
        _workspaceId = config["Clockify:WorkspaceId"] ?? throw new Exception("CLOCKIFY_WORKSPACE_ID no configurado");
        _userId = config["Clockify:UserId"] ?? string.Empty;

        _client = new HttpClient
        {
            BaseAddress = new Uri("https://api.clockify.me/api/v1/"),
            Timeout = TimeSpan.FromMinutes(2)
        };
        _client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        _reportsClient = new HttpClient
        {
            BaseAddress = new Uri("https://reports.api.clockify.me/v1/"),
            Timeout = TimeSpan.FromMinutes(2)
        };
        _reportsClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
    }

    private async Task<List<JsonElement>> GetAsync(string url, Dictionary<string, string>? query = null)
    {
        if (query != null && query.Count > 0)
        {
            var qs = string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
            url = $"{url}?{qs}";
        }

        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<JsonElement>>(content) ?? new();
    }

    public async Task<List<JsonElement>> GetProjects()
    {
        var all = new List<JsonElement>();
        foreach (var archived in new[] { "false", "true" })
        {
            var page = 1;
            List<JsonElement> chunk;
            do
            {
                chunk = await GetAsync($"workspaces/{_workspaceId}/projects", new()
                {
                    ["page"] = page.ToString(),
                    ["page-size"] = "500",
                    ["archived"] = archived
                });
                all.AddRange(chunk);
                page++;
            } while (chunk.Count == 500);
        }
        return all;
    }

    public async Task<List<JsonElement>> GetClients()
    {
        var all = new List<JsonElement>();
        var page = 1;
        List<JsonElement> chunk;
        do
        {
            chunk = await GetAsync($"workspaces/{_workspaceId}/clients", new()
            {
                ["page"] = page.ToString(),
                ["page-size"] = "500"
            });
            all.AddRange(chunk);
            page++;
        } while (chunk.Count == 500);
        return all;
    }

    public async Task<List<JsonElement>> GetUsers(bool onlyActive = false)
    {
        var all = new List<JsonElement>();
        var page = 1;
        List<JsonElement> chunk;
        do
        {
            chunk = await GetAsync($"workspaces/{_workspaceId}/users", new()
            {
                ["page"] = page.ToString(),
                ["page-size"] = "500"
            });
            if (onlyActive)
                chunk = chunk.Where(u =>
                    !u.TryGetProperty("status", out var s) || s.GetString() == "ACTIVE"
                ).ToList();
            all.AddRange(chunk);
            page++;
        } while (chunk.Count == 500);
        return all;
    }

    public async Task<List<JsonElement>> GetTimeEntries(string userId, string? start, string? end, int page, int pageSize)
    {
        var query = new Dictionary<string, string>
        {
            ["page"] = page.ToString(),
            ["page-size"] = pageSize.ToString()
        };
        if (!string.IsNullOrEmpty(start)) query["start"] = start;
        if (!string.IsNullOrEmpty(end)) query["end"] = end;

        return await GetAsync($"workspaces/{_workspaceId}/user/{userId}/time-entries", query);
    }

    public async Task<List<JsonElement>> GetTimeEntriesByProject(string projectId, string? start, string? end, int page, int pageSize)
    {
        try
        {
            return await GetTimeEntriesByProjectUsingReports(projectId, start, end, page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Reports API falló (posiblemente plan gratuito), usando método alternativo");
            return await GetTimeEntriesByProjectAlternative(projectId, start, end, page, pageSize);
        }
    }

    private async Task<List<JsonElement>> GetTimeEntriesByProjectUsingReports(string projectId, string? start, string? end, int page, int pageSize)
    {
        end ??= DateTime.UtcNow.ToString("yyyy-MM-ddT23:59:59Z");
        start ??= DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-ddT00:00:00Z");

        var startDate = DateTime.Parse(start);
        var endDate = DateTime.Parse(end);
        if ((endDate - startDate).TotalDays > 365)
            start = endDate.AddYears(-1).ToString("yyyy-MM-ddT00:00:00Z");

        var body = new
        {
            dateRangeStart = start,
            dateRangeEnd = end,
            projects = new { ids = new[] { projectId } },
            detailedFilter = new { page, pageSize = Math.Min(pageSize, 1000), sortColumn = "DATE" }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(body);
        var response = await _reportsClient.PostAsync(
            $"workspaces/{_workspaceId}/reports/detailed",
            new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        );
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(content);

        if (data.TryGetProperty("timeentries", out var te) && te.ValueKind == System.Text.Json.JsonValueKind.Array)
            return te.EnumerateArray().ToList();
        if (data.TryGetProperty("timeEntries", out var te2) && te2.ValueKind == System.Text.Json.JsonValueKind.Array)
            return te2.EnumerateArray().ToList();

        return new List<System.Text.Json.JsonElement>();
    }

    private async Task<List<System.Text.Json.JsonElement>> GetTimeEntriesByProjectAlternative(string projectId, string? start, string? end, int page, int pageSize)
    {
        var users = await GetUsers();
        var allEntries = new List<System.Text.Json.JsonElement>();
        var entriesNeeded = page * pageSize;

        foreach (var user in users)
        {
            if (allEntries.Count >= entriesNeeded) break;
            if (!user.TryGetProperty("id", out var idProp)) continue;
            var userId = idProp.GetString() ?? string.Empty;

            try
            {
                var userPage = 1;
                List<System.Text.Json.JsonElement> userEntries;
                do
                {
                    userEntries = await GetTimeEntries(userId, start, end, userPage, 100);
                    foreach (var entry in userEntries)
                    {
                        if (entry.TryGetProperty("projectId", out var pid) &&
                            pid.GetString() == projectId)
                            allEntries.Add(entry);
                    }
                    userPage++;
                } while (userEntries.Count == 100 && allEntries.Count < entriesNeeded);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error obteniendo time entries del usuario {UserId}", userId);
            }
        }

        var offset = (page - 1) * pageSize;
        return allEntries.Skip(offset).Take(pageSize).ToList();
    }

    public async IAsyncEnumerable<JsonElement> GetAllUsersTimeEntries(string? start, string? end)
    {
        var users = await GetUsers();
        foreach (var user in users)
        {
            if (!user.TryGetProperty("id", out var idProp)) continue;
            var userId = idProp.GetString() ?? string.Empty;
            var page = 1;
            List<JsonElement> entries;
            do
            {
                entries = await GetTimeEntries(userId, start, end, page, 100);
                foreach (var entry in entries) yield return entry;
                page++;
            } while (entries.Count == 100);
        }
    }

    public async Task<System.Text.Json.JsonElement> CreateProjectAsync(string name, string? clientExternalId = null)
    {
        var body = new Dictionary<string, object> { ["name"] = name, ["isPublic"] = false };
        if (!string.IsNullOrEmpty(clientExternalId))
            body["clientId"] = clientExternalId;

        var json = JsonSerializer.Serialize(body);

        var response = await _client.PostAsync(
            $"workspaces/{_workspaceId}/projects",
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error al crear proyecto en Clockify: {Status} — {Error}", response.StatusCode, error);
            throw new Exception($"Clockify API error {(int)response.StatusCode}: {error}");
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(content);
    }

    public string GetUserId() => _userId;
    public string GetWorkspaceId() => _workspaceId;
}