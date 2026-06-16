using bdt_evm_app.Data;
using bdt_evm_app.Models;
using Microsoft.EntityFrameworkCore;

namespace bdt_evm_app.Services;

public class EtcService
{
    private readonly AppDbContext _db;

    public EtcService(AppDbContext db) => _db = db;

    public async Task<(EtcSnapshot? Snapshot, List<EtcRecord> Records)> GetRecordsForProject(ulong projectId, bool wantBaseline = false)
    {
        EtcSnapshot? snapshot;

        if (wantBaseline)
            snapshot = await _db.EtcSnapshots
                .Where(s => s.ProjectId == projectId)
                .OrderBy(s => s.Version)
                .FirstOrDefaultAsync();
        else
            snapshot = await _db.EtcSnapshots
                .Where(s => s.ProjectId == projectId)
                .OrderByDescending(s => s.Version)
                .FirstOrDefaultAsync();

        List<EtcRecord> records;
        if (snapshot != null)
            records = await _db.EtcRecords
                .Where(r => r.SnapshotId == snapshot.Id)
                .OrderBy(r => r.MonthKey)
                .ThenBy(r => r.UserName)
                .ToListAsync();
        else
            records = await _db.EtcRecords
                .Where(r => r.ProjectId == projectId && r.SnapshotId == null)
                .OrderBy(r => r.MonthKey)
                .ThenBy(r => r.UserName)
                .ToListAsync();

        return (snapshot, records);
    }

    public async Task<Dictionary<ulong, decimal>> GetTotalHoursByProjectIds(IEnumerable<ulong> projectIds)
    {
        var ids = projectIds.ToList();

        var latestSnapshots = await _db.EtcSnapshots
            .Where(s => ids.Contains(s.ProjectId))
            .GroupBy(s => s.ProjectId)
            .Select(g => new { ProjectId = g.Key, SnapshotId = g.OrderByDescending(s => s.Version).First().Id })
            .ToListAsync();

        var projectsWithSnapshot = latestSnapshots.Select(s => s.ProjectId).ToHashSet();
        var snapshotIds = latestSnapshots.Select(s => s.SnapshotId).ToList();
        var projectsWithoutSnapshot = ids.Where(id => !projectsWithSnapshot.Contains(id)).ToList();

        var result = new Dictionary<ulong, decimal>();

        if (snapshotIds.Any())
        {
            var hoursFromSnapshots = await _db.EtcRecords
                .Where(r => r.SnapshotId.HasValue && snapshotIds.Contains(r.SnapshotId.Value))
                .GroupBy(r => r.ProjectId)
                .Select(g => new { ProjectId = g.Key, Total = g.Sum(r => r.Hours) })
                .ToListAsync();

            foreach (var x in hoursFromSnapshots)
                result[x.ProjectId] = x.Total;
        }

        if (projectsWithoutSnapshot.Any())
        {
            var hoursFromNullSnapshot = await _db.EtcRecords
                .Where(r => projectsWithoutSnapshot.Contains(r.ProjectId) && r.SnapshotId == null)
                .GroupBy(r => r.ProjectId)
                .Select(g => new { ProjectId = g.Key, Total = g.Sum(r => r.Hours) })
                .ToListAsync();

            foreach (var x in hoursFromNullSnapshot)
                result[x.ProjectId] = x.Total;
        }

        return result;
    }
}
