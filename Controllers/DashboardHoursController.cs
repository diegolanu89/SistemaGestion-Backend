using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Attributes;
using bdt_evm_app.Data;
using bdt_evm_app.Models;

namespace bdt_evm_app.Controllers;

[ApiController]
[Route("api/dashboard-hours")]
[RequirePermission("DASHBOARD_HOURS_ACCESS")]
public class DashboardHoursController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<DashboardHoursController> _logger;

    public DashboardHoursController(
        AppDbContext db,
        ILogger<DashboardHoursController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] string? leader_id,
        [FromQuery] string? project_id,
         [FromQuery] string? source_type)
        
    {
        var month_keys = Request.Query
            .Where(q => q.Key == "month_keys" || q.Key == "month_keys[]")
            .SelectMany(q => q.Value)
            .Where(v => !string.IsNullOrEmpty(v))
            .ToArray();

        try
        {
            var filterByLeader =
                !string.IsNullOrEmpty(leader_id) &&
                leader_id != "Todos";

            var filterByMonths =
                month_keys != null &&
                month_keys.Length > 0 &&
                !month_keys.Any(m =>
                    m.Equals("Todo", StringComparison.OrdinalIgnoreCase));

            var filterByProject =
                !string.IsNullOrEmpty(project_id);

            var onlyEtc =
                string.Equals(
                    source_type,
                    "ETC",
                    StringComparison.OrdinalIgnoreCase);

            var onlyPotential =
                string.Equals(
                    source_type,
                    "POTENTIAL",
                    StringComparison.OrdinalIgnoreCase);           

            var monthKeysList =
                filterByMonths
                    ? month_keys!.ToList()
                    : new List<string>();

            // =========================================================
            // 🔹 LEADERS
            // =========================================================

            List<ulong> userIdsWithLeader = new();

            if (filterByLeader && ulong.TryParse(leader_id, out var leaderIdVal))
            {
                userIdsWithLeader = await _db.UserLeaders
                    .Where(ul =>
                        ul.LeaderId == leaderIdVal &&
                        ul.EndDate == null)
                    .Select(ul => ul.UserId)
                    .ToListAsync();
            }

            // =========================================================
            // 🔹 SNAPSHOTS
            // =========================================================

            var latestSnapshotIds =
                await GetLatestSnapshotIdsPerProject();

            var projectIdsWithSnapshots =
                await _db.EtcSnapshots
                    .Select(s => s.ProjectId)
                    .Distinct()
                    .ToListAsync();

            // =========================================================
            // 🔹 LEADER MAP
            // =========================================================

            var allLeaderRows = await _db.UserLeaders
                .Where(ul => ul.EndDate == null)
                .OrderByDescending(ul => ul.StartDate)
                .ToListAsync();

            var leaderByUser = allLeaderRows
                .GroupBy(ul => ul.UserId)
                .ToDictionary(g => g.Key, g => g.First());

            var leaderIdsForMap = leaderByUser.Values
                .Where(ul => ul.LeaderId != 0)
                .Select(ul => ul.LeaderId)
                .Distinct()
                .ToList();

            var leadersMap = await _db.TimesheetUsers
                .Where(u => leaderIdsForMap.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            // =========================================================
            // 🔹 ETC QUERY
            // =========================================================

            var etcQuery = _db.EtcRecords
                .Where(r =>
                    latestSnapshotIds.Contains(r.SnapshotId ?? 0) ||
                    (
                        r.SnapshotId == null &&
                        !projectIdsWithSnapshots.Contains(r.ProjectId)
                    ))
                .AsQueryable();

            if (filterByLeader)
            {
                etcQuery = etcQuery.Where(r =>
                    r.UserId.HasValue &&
                    userIdsWithLeader.Contains(r.UserId.Value));
            }

            if (filterByMonths)
            {
                etcQuery = etcQuery.Where(r =>
                    monthKeysList.Contains(r.MonthKey));
            }

            if (filterByProject && project_id != null)
            {
                if (ulong.TryParse(project_id, out var projIdVal))
                {
                    etcQuery = etcQuery.Where(r =>
                        r.ProjectId == projIdVal);
                }
                else
                {
                    etcQuery = etcQuery.Where(r => false);
                }
            }

            var etcData = await etcQuery.ToListAsync();

            // =========================================================
            // 🔹 ETC PROJECTS
            // =========================================================

            var etcProjectIds = etcData
                .Select(r => r.ProjectId)
                .Distinct()
                .ToList();

            var etcProjects = await _db.TimesheetProjects
                .Include(p => p.Client)
                .Where(p => etcProjectIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            // =========================================================
            // 🔹 POTENTIAL QUERY
            // =========================================================

            var potencialQuery = _db.PotencialProjectAllocations
                .Include(a => a.User)
                .Where(a => a.Hours > 0)
                .AsQueryable();

            if (filterByLeader)
            {
                potencialQuery = potencialQuery.Where(a =>
                    a.UserId.HasValue &&
                    userIdsWithLeader.Contains(a.UserId.Value));
            }

            if (filterByMonths)
            {
                potencialQuery = potencialQuery.Where(a =>
                    monthKeysList.Contains(a.MonthKey));
            }

            // =========================================================
            // 🔹 IMPORTANT FIX
            // =========================================================

            if (filterByProject && project_id != null)
            {
                if (project_id.StartsWith("F-"))
                {
                    var parsed =
                        project_id.Replace("F-", "");

                    if (ulong.TryParse(parsed, out var potencialProjectId))
                    {
                        potencialQuery = potencialQuery.Where(a =>
                            a.PotencialProjectId == potencialProjectId);
                    }
                    else
                    {
                        potencialQuery = potencialQuery.Where(a => false);
                    }
                }
                else
                {
                    potencialQuery = potencialQuery.Where(a => false);
                }
            }

            var potencialData = await potencialQuery.ToListAsync();

            // =========================================================
            // 🔹 POTENTIAL PROJECTS
            // =========================================================

            var potencialProjectIds = potencialData
                .Select(a => a.PotencialProjectId)
                .Distinct()
                .ToList();

            var potencialProjects = await _db.PotencialProjects
                .Include(p => p.PotencialClient)
                .Where(p => potencialProjectIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            // =========================================================
            // 🔹 GROUPING
            // =========================================================

            var grouped =
                new Dictionary<string, IDictionary<string, object?>>();

            var detailsByKey =
                new Dictionary<string, Dictionary<string, IDictionary<string, object?>>>();

            var nameToFirstUserId =
                new Dictionary<string, ulong?>();

            var syntheticId = -1;

            // =========================================================
            // 🔹 ETC LOOP
            // =========================================================

            if (!onlyPotential)
            {
                foreach (var item in etcData)

                {
                var userName =
                    (item.UserName ?? "Sin usuario").Trim();

                var groupKey =
                    "name_" + userName.ToLower();

                var monthKey = item.MonthKey;

                var hours = item.Hours;

                if (
                    filterByMonths &&
                    !monthKeysList.Contains(monthKey))
                {
                    continue;
                }

                if (!grouped.ContainsKey(groupKey))
                {
                    nameToFirstUserId[groupKey] = item.UserId;

                    var leaderRow =
                        item.UserId.HasValue
                            ? leaderByUser.GetValueOrDefault(item.UserId.Value)
                            : null;

                    TimesheetUser? leader =
                        leaderRow != null
                            ? leadersMap.GetValueOrDefault(leaderRow.LeaderId)
                            : null;

                    grouped[groupKey] = new Dictionary<string, object?>
                    {
                        ["user_id"] = syntheticId--,
                        ["user_name"] = userName,
                        ["leader_id"] = leader?.Id,
                        ["leader_name"] = leader?.Name,
                        ["months"] = new Dictionary<string, object>()
                    };

                    detailsByKey[groupKey] =
                        new Dictionary<string, IDictionary<string, object?>>();
                }

                var project =
                    etcProjects.GetValueOrDefault(item.ProjectId);

                var detailKey =
                    "R_" + item.ProjectId;

                var gMonths =
                    (Dictionary<string, object>)grouped[groupKey]["months"]!;

                if (!gMonths.ContainsKey(monthKey))
                {
                    gMonths[monthKey] = new
                    {
                        hours = 0m,
                        expected = 0m
                    };
                }

                var gEx = (dynamic)gMonths[monthKey];

                gMonths[monthKey] = new
                {
                    hours = gEx.hours + hours,
                   expected = 0m
                };

                if (!detailsByKey[groupKey].ContainsKey(detailKey))
                {
                    detailsByKey[groupKey][detailKey] =
                        new Dictionary<string, object?>
                        {
                            ["project_id"] = project?.Id,
                            ["client_name"] = project?.Client?.Name,
                            ["project_name"] = project?.Name,
                            ["project_type"] = "R",
                            ["months"] = new Dictionary<string, object>()
                        };
                }

                var dMonths =
                    (Dictionary<string, object>)
                        detailsByKey[groupKey][detailKey]["months"]!;

                if (!dMonths.ContainsKey(monthKey))
                {
                    dMonths[monthKey] = new
                    {
                        hours = 0m,
                        expected = 0m
                    };
                }

                var dEx = (dynamic)dMonths[monthKey];

                dMonths[monthKey] = new
                {
                    hours = dEx.hours + hours,
                    expected = 0m
                };
            }
            }

            // =========================================================
            // 🔹 POTENTIAL LOOP
            // =========================================================
            if (!onlyEtc)
            {
            foreach (var item in potencialData)
            {
                var userName =
                    (item.UserName ?? "Sin usuario").Trim();

                var groupKey =
                    "name_" + userName.ToLower();

                var monthKey = item.MonthKey;

                var hours = item.Hours;

                if (
                    filterByMonths &&
                    !monthKeysList.Contains(monthKey))
                {
                    continue;
                }

                if (!grouped.ContainsKey(groupKey))
                {
                    nameToFirstUserId[groupKey] = item.UserId;

                    var leaderRow =
                        item.UserId.HasValue
                            ? leaderByUser.GetValueOrDefault(item.UserId.Value)
                            : null;

                    TimesheetUser? leader =
                        leaderRow != null
                            ? leadersMap.GetValueOrDefault(leaderRow.LeaderId)
                            : null;

                    grouped[groupKey] = new Dictionary<string, object?>
                    {
                        ["user_id"] = syntheticId--,
                        ["user_name"] = userName,
                        ["leader_id"] = leader?.Id,
                        ["leader_name"] = leader?.Name,
                        ["months"] = new Dictionary<string, object>()
                    };

                    detailsByKey[groupKey] =
                        new Dictionary<string, IDictionary<string, object?>>();
                }

                var proj =
                    potencialProjects.GetValueOrDefault(item.PotencialProjectId);

                var detailKey =
                    "F_" + item.PotencialProjectId;

                var gMonths =
                    (Dictionary<string, object>)grouped[groupKey]["months"]!;

                if (!gMonths.ContainsKey(monthKey))
                {
                    gMonths[monthKey] = new
                    {
                        hours = 0m,
                        expected = 0m
                    };
                }

                var gEx = (dynamic)gMonths[monthKey];

                gMonths[monthKey] = new
                {
                    hours = gEx.hours + hours,
                    expected = 0m
                };

                if (!detailsByKey[groupKey].ContainsKey(detailKey))
                {
                    detailsByKey[groupKey][detailKey] =
                        new Dictionary<string, object?>
                        {
                            ["project_id"] = (object?)null,
                            ["client_name"] = proj?.PotencialClient?.Name,
                            ["project_name"] = proj?.Name,
                            ["project_type"] = "F",
                            ["months"] = new Dictionary<string, object>()
                        };
                }

                var dMonths =
                    (Dictionary<string, object>)
                        detailsByKey[groupKey][detailKey]["months"]!;

                if (!dMonths.ContainsKey(monthKey))
                {
                    dMonths[monthKey] = new
                    {
                        hours = 0m,
                        expected = 0m
                    };
                }

                var dEx = (dynamic)dMonths[monthKey];

                dMonths[monthKey] = new
                {
                    hours = dEx.hours + hours,
                   expected = 0m
                };
            }
            }
            // =========================================================
            // 🔹 USER IDS
            // =========================================================

            foreach (var groupKey in grouped.Keys.ToList())
            {
                if (
                    !nameToFirstUserId.ContainsKey(groupKey) ||
                    nameToFirstUserId[groupKey] == null)
                {
                    var uName =
                        grouped[groupKey]["user_name"]?.ToString() ?? "";

                    if (
                        !string.IsNullOrEmpty(uName) &&
                        uName != "Sin usuario")
                    {
                        var u =
                            await _db.TimesheetUsers
                                .FirstOrDefaultAsync(cu =>
                                    cu.Name.Trim() == uName.Trim());

                        if (u != null)
                        {
                            nameToFirstUserId[groupKey] = u.Id;
                        }
                    }
                }
            }

            // =========================================================
            // 🔹 ROLES
            // =========================================================

            var userIdsForKpi = nameToFirstUserId.Values
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var rolesByUser = await _db.TimesheetUsers
                .Where(u => userIdsForKpi.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Role);

            var capacityRecords = await _db.UserMonthlyCapacities
                .Where(c => userIdsForKpi.Contains(c.UserId))
                .ToListAsync();

            var userCapacityMap = capacityRecords
                .GroupBy(c => c.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(c => c.MonthKey, c => c.Hours)
                );              

                

            // =========================================================
            // 🔹 RESULT
            // =========================================================

            var result = new List<object>();

            foreach (var groupKey in grouped.Keys)
            {
                var g = grouped[groupKey];

                // =====================================================
                // 🔹 IMPORTANT FIX
                // =====================================================

                var details = detailsByKey[groupKey]
                    .Values
                    .Where(d =>
                    {
                        if (!filterByProject)
                            return true;

                        var detailProjectType =
                            d["project_type"]?.ToString();

                        if (project_id != null && project_id.StartsWith("F-"))
                        {
                            return detailProjectType == "F";
                        }

                        return detailProjectType == "R";
                    })
                    .ToList();

                if (!details.Any())
                    continue;

                var firstDetail = details.FirstOrDefault();

                var firstUserId =
                    nameToFirstUserId.GetValueOrDefault(groupKey);

                var role =
                    firstUserId.HasValue
                        ? rolesByUser.GetValueOrDefault(firstUserId.Value)
                        : null;

                result.Add(new
                {
                    user_id = g["user_id"],
                    user_name = g["user_name"],
                    leader_id = g["leader_id"],
                    leader_name = g["leader_name"],
                    role,
                    role_short = RoleToShort(role),

                    project_id =
                        details.Count == 1
                            ? firstDetail?["project_id"]
                            : null,

                    project_name =
                        details.Count > 1
                            ? $"Varios ({details.Count})"
                            : firstDetail?["project_name"],

                    project_type =
                        details.Count == 1
                            ? firstDetail?["project_type"]
                            : null,

                    client_name =
                        details.Count > 1
                            ? $"Varios ({details.Count})"
                            : firstDetail?["client_name"],

                    months =
                        ((IDictionary<string, object>)g["months"]!)
                            .ToDictionary(
                                month => month.Key,
                                month =>
                                {
                                    dynamic data = month.Value;

                                    decimal expected =
                                        firstUserId.HasValue &&
                                        userCapacityMap.TryGetValue(firstUserId.Value, out var userCapacity) &&
                                        userCapacity.TryGetValue(month.Key, out var capacity)
                                            ? capacity
                                            : 0m;

                                    return new
                                    {
                                        hours = (decimal)data.hours,
                                        expected
                                    };
                                }
                            ),

                    details = details.Select(d => new
                    {
                        project_id = d["project_id"],
                        client_name = d["client_name"],
                        project_name = d["project_name"],
                        project_type = d["project_type"],

                        months =
                            ((IDictionary<string, object>)d["months"]!)
                                .ToDictionary(
                                    month => month.Key,
                                    month =>
                                    {
                                        dynamic data = month.Value;

                                        decimal expected =
                                            firstUserId.HasValue &&
                                            userCapacityMap.TryGetValue(firstUserId.Value, out var userCapacity) &&
                                            userCapacity.TryGetValue(month.Key, out var capacity)
                                                ? capacity
                                                : 0m;

                                        return new
                                        {
                                            hours = (decimal)data.hours,
                                            expected
                                        };
                                    }
                                )
                    }).ToList()
                });
            }

            // =========================================================
            // 🔹 MONTH FILTER
            // =========================================================

            if (filterByMonths)
            {
                result = result.Where(r =>
                {
                    dynamic rowMonths =
                        ((dynamic)r).months;

                    return monthKeysList.Any(mk =>
                        rowMonths.ContainsKey(mk));
                }).ToList();
            }

            result = result
                .OrderBy(r => ((dynamic)r).user_name?.ToString())
                .ToList();

            // =========================================================
            // 🔹 MONTHS
            // =========================================================

            var monthsEtc = await _db.EtcRecords
                .Where(r =>
                    latestSnapshotIds.Contains(r.SnapshotId ?? 0) ||
                    (
                        r.SnapshotId == null &&
                        !projectIdsWithSnapshots.Contains(r.ProjectId)
                    ))
                .Select(r => r.MonthKey)
                .Distinct()
                .ToListAsync();

            var monthsPotencial = await _db.PotencialProjectAllocations
                .Select(a => a.MonthKey)
                .Distinct()
                .ToListAsync();

            var allMonthsFromDb = monthsEtc
                .Union(monthsPotencial)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            var allMonths =
                filterByMonths
                    ? monthKeysList
                        .Intersect(allMonthsFromDb)
                        .OrderBy(m => m)
                        .ToList()
                    : allMonthsFromDb;

            // =========================================================
            // 🔹 CALENDARS
            // =========================================================

            var calendars = await _db.WorkingDaysCalendars
                .Where(c => allMonths.Contains(c.MonthKey))
                .ToDictionaryAsync(c => c.MonthKey);

            var monthHoursMap = allMonths.ToDictionary(
                mk => mk,
                mk =>
                    calendars.TryGetValue(mk, out var cal)
                        ? (decimal?)cal.HoursMonth
                        : null
            );

            // =========================================================
            // 🔹 FILTER OPTIONS
            // =========================================================

            var leaderIdsInUse = await _db.UserLeaders
                .Where(ul => ul.LeaderId != 0)
                .Select(ul => ul.LeaderId)
                .Distinct()
                .ToListAsync();

            var leadersForFilter = await _db.TimesheetUsers
                .Where(u => leaderIdsInUse.Contains(u.Id))
                .OrderBy(u => u.Name)
                .Select(u => new
                {
                    id = u.Id,
                    name = u.Name
                })
                .ToListAsync();

            var monthsForFilter = allMonthsFromDb
                .Select(mk =>
                {
                    var parts = mk.Split('-');

                    return new
                    {
                        month_key = mk,
                        year = int.Parse(parts[0]),
                        month = int.Parse(parts[1])
                    };
                })
                .ToList();

            // =========================================================
            // 🔹 REAL PROJECT IDS WITH VALID DASHBOARD DATA
            // =========================================================

            var allRealProjectIds = await _db.EtcRecords
                .Where(r =>
                    latestSnapshotIds.Contains(r.SnapshotId ?? 0) ||
                    (
                        r.SnapshotId == null &&
                        !projectIdsWithSnapshots.Contains(r.ProjectId)
                    ))
                .Select(r => r.ProjectId)
                .Distinct()
                .ToListAsync();

            // =========================================================
            // 🔹 REAL PROJECTS FOR FILTER
            // =========================================================

            var realProjects = await _db.TimesheetProjects
                .Where(p => allRealProjectIds.Contains(p.Id))
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    id = p.Id.ToString(),
                    name = p.Name,
                    project_type = "R"
                })
                .ToListAsync();

            // =========================================================
            // 🔹 POTENTIAL PROJECT IDS WITH VALID DATA
            // =========================================================

            var allPotentialProjectIds = await _db.PotencialProjectAllocations
                .Where(a => a.Hours > 0)
                .Select(a => a.PotencialProjectId)
                .Distinct()
                .ToListAsync();

            // =========================================================
            // 🔹 POTENTIAL PROJECTS FOR FILTER
            // =========================================================

            var potencialProjectsList = await _db.PotencialProjects
                .Where(p => allPotentialProjectIds.Contains(p.Id))
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    id = "F-" + p.Id,
                    name = p.Name,
                    project_type = "F"
                })
                .ToListAsync();

            // =========================================================
            // 🔹 FINAL PROJECT FILTER OPTIONS
            // =========================================================

            var projectsForFilter = realProjects
                .Cast<object>()
                .Concat(potencialProjectsList.Cast<object>())
                .ToList();

            // =========================================================
            // 🔹 KPI
            // =========================================================

            var capacities = await _db.UserMonthlyCapacities
                .Where(c =>
                    userIdsForKpi.Contains(c.UserId) &&
                    allMonths.Contains(c.MonthKey))
                .ToListAsync();

            var capacityMap = capacities
                .GroupBy(c => c.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(c => c.MonthKey, c => c.Hours)
                );

            var kpisByRole = new Dictionary<string, object>();

            foreach (var row in result)
            {
                var dynRow = (dynamic)row;

                var uName =
                    ((string?)dynRow.user_name ?? "")
                    .ToLower()
                    .Trim();

                var gKey = "name_" + uName;

                var fUserId =
                    nameToFirstUserId.GetValueOrDefault(gKey);

                if (!fUserId.HasValue)
                    continue;

                var userRole =
                    rolesByUser.GetValueOrDefault(fUserId.Value)
                    ?? "Sin función";

                if (!kpisByRole.ContainsKey(userRole))
                {
                    kpisByRole[userRole] = new
                    {
                        role = userRole,
                        months = new Dictionary<string, object>()
                    };
                }

                var kpiMonths =
                    (Dictionary<string, object>)
                        ((dynamic)kpisByRole[userRole]).months;

                foreach (var mk in allMonths)
                {
                    dynamic rowMonths =

                        dynRow.months;

                    decimal need =

                        rowMonths.ContainsKey(mk)

                            ? (decimal)rowMonths[mk].hours

                            : 0m;

                    var availability =
                        capacityMap.TryGetValue(fUserId.Value, out var userCap)
                        &&
                        userCap.TryGetValue(mk, out var cap)
                            ? cap
                            : (
                                monthHoursMap.TryGetValue(mk, out var mh)
                                &&
                                mh.HasValue
                                    ? mh.Value
                                    : 160m
                            );

                    if (!kpiMonths.ContainsKey(mk))
                    {
                        kpiMonths[mk] = new
                        {
                            availability = 0m,
                            need = 0m,
                            difference = 0m,
                            difference_fte = (decimal?)null
                        };
                    }

                    var existing = (dynamic)kpiMonths[mk];

                    kpiMonths[mk] = new
                    {
                        availability =
                            existing.availability + availability,

                        need =
                            existing.need + need,

                        difference = 0m,

                        difference_fte = (decimal?)null
                    };
                }
            }

            foreach (var role in kpisByRole.Keys.ToList())
            {
                var kpiMonths =
                    (Dictionary<string, object>)
                        ((dynamic)kpisByRole[role]).months;

                foreach (var mk in allMonths)
                {
                    if (!kpiMonths.ContainsKey(mk))
                        continue;

                    var existing = (dynamic)kpiMonths[mk];

                    var diff =
                        existing.availability - existing.need;

                    var mhVal =
                        monthHoursMap.TryGetValue(mk, out var mh)
                        &&
                        mh.HasValue
                            ? mh.Value
                            : 0m;

                    kpiMonths[mk] = new
                    {
                        availability = existing.availability,
                        need = existing.need,
                        difference = diff,

                        difference_fte =
                            mhVal > 0
                                ? (decimal?)(diff / mhVal)
                                : null
                    };
                }
            }

            return Ok(new
            {
                success = true,

                data = result,

                months = allMonths,

                month_hours = monthHoursMap,

                options = new
                {
                    leaders = leadersForFilter,
                    months = monthsForFilter,
                    projects = projectsForFilter
                },

                kpis = new
                {
                    by_role = kpisByRole,
                    months = allMonths
                }
            });
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error al obtener datos del dashboard de horas");

            return StatusCode(500, new
            {
                success = false,
                message = "Error al obtener los datos",
                error = e.Message
            });
        }
    }

    private async Task<List<ulong>> GetLatestSnapshotIdsPerProject()
    {
        return await _db.EtcSnapshots
            .GroupBy(s => s.ProjectId)
            .Select(g =>
                g.OrderByDescending(s => s.Version)
                 .First()
                 .Id)
            .ToListAsync();
    }

    private static string? RoleToShort(string? role)
    {
        if (string.IsNullOrEmpty(role))
            return null;

        return role.ToLower() switch
        {
            "líder" or "lider" => "LD",
            "analista" => "AF",
            "desarrollador" => "DEV",
            "qa" => "QA",

            _ => role.Length >= 3
                ? role[..3].ToUpper()
                : role.ToUpper()
        };
    }
}