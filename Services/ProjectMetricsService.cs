using bdt_evm_app.Data;
using bdt_evm_app.Models;
using Microsoft.EntityFrameworkCore;

namespace bdt_evm_app.Services;

public class ProjectMetricsService
{
    private readonly AppDbContext _db;

    public ProjectMetricsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<object> CalculateEVM(ClockifyProject project, string? from = null, string? to = null)
    {
        // AC — todas las horas trabajadas sin filtro de fecha
        var acBase = await _db.ClockifyTimeEntries
            .Where(t => t.ProjectId == project.Id && t.ChangeRequestId == null)
            .SumAsync(t => t.DurationHours);

        var acCc = await _db.ClockifyTimeEntries
            .Where(t => t.ProjectId == project.Id && t.ChangeRequestId != null)
            .SumAsync(t => t.DurationHours);

        var acTotal = acBase + acCc;

        // BAC — ya calculado en bac_total_hours
        var bacTotal = project.BacTotalHours;

        // ETC — snapshot efectivo
        var etc = await CalculateEtc(project);

        // EAC y VAC
        var eac = acTotal + etc;
        var vac = bacTotal - eac;

        // Costos
        var rate = project.HourlyRate;
        var acCost = acTotal * rate;
        var etcCost = etc * rate;
        var eacCost = eac * rate;
        var vacCost = vac * rate;

        return new
        {
            project_id = project.Id,
            project_name = project.Name,
            filters = new { from, to },
            hours = new
            {
                bac_base_hours = project.BacBaseHours,
                bac_total_hours = bacTotal,
                ac_hours_base = acBase,
                ac_hours_cc = acCc,
                ac_hours_total = acTotal,
                etc_hours = etc,
                eac_hours = eac,
                vac_hours = vac
            },
            cost = new
            {
                hourly_rate = rate,
                ac_cost = acCost,
                etc_cost = etcCost,
                eac_cost = eacCost,
                vac_cost = vacCost
            }
        };
    }

    private async Task<decimal> CalculateEtc(ClockifyProject project)
    {
        var calculationMode = project.EtcCalculationMode ?? "manual";

        // Obtener snapshot efectivo
        var snapshotId = await GetLatestSnapshotIdForProject((int)project.Id);

        List<EtcRecord> records;
        if (snapshotId.HasValue)
        {
            records = await _db.EtcRecords
                .Where(r => r.SnapshotId == (ulong)snapshotId.Value)
                .ToListAsync();
        }
        else
        {
            records = await _db.EtcRecords
                .Where(r => r.ProjectId == project.Id && r.SnapshotId == null)
                .ToListAsync();
        }

        var hasEtcRecords = records.Count > 0;

        if (calculationMode == "automatic" || hasEtcRecords)
        {
            // Deduplicar por usuario+mes
            var seen = new HashSet<string>();
            var uniqueRecords = new List<EtcRecord>();

            foreach (var record in records)
            {
                var userName = (record.UserName ?? string.Empty).Trim();
                var key = $"{userName}|{record.MonthKey}";
                if (seen.Add(key))
                    uniqueRecords.Add(record);
            }

            return uniqueRecords.Sum(r => r.Hours);
        }

        return 0;
    }

    private async Task<int?> GetLatestSnapshotIdForProject(int projectId)
    {
        var snapshot = await _db.EtcSnapshots
            .Where(s => s.ProjectId == (ulong)projectId)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync();

        return snapshot != null ? (int)snapshot.Id : null;
    }
}