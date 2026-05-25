using System.Text.Json;
using bdt_evm_app.Models;
using bdt_evm_app.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace bdt_evm_app.Data;

// EF Core SaveChangesInterceptor para RF-11.
//
// Flujo:
//   1. SavingChangesAsync — recorre el ChangeTracker, snapshotea valores
//      anteriores y referencias a las entries (las PKs generadas todavía
//      no están disponibles para Added).
//   2. SavedChangesAsync  — ya con PKs, construye las filas de
//      change_audit_log y las graba con un SaveChanges adicional.
//      El propio interceptor detecta "todas las entries son ChangeAuditLog"
//      y short-circuita → no hay recursión.
//
// Skip: PersonalAccessToken (lo actualiza el SanctumMiddleware en cada
// request, generaría 1 fila por request) y ChangeAuditLog (loop).
//
// Login/logout NO pasan por acá: no son CRUD de entidad de negocio. Los
// inserta AuthController explícitamente.
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions _json = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly HashSet<string> _skipEntities = new(StringComparer.Ordinal)
    {
        nameof(ChangeAuditLog),
        nameof(PersonalAccessToken)
    };

    private readonly IHttpContextAccessor _http;
    private List<PendingAudit>? _pending;

    public AuditSaveChangesInterceptor(IHttpContextAccessor http)
    {
        _http = http;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var ctx = eventData.Context;
        if (ctx == null) return ValueTask.FromResult(result);

        _pending = new List<PendingAudit>();

        foreach (var entry in ctx.ChangeTracker.Entries())
        {
            if (entry.Entity is null) continue;

            var entityName = entry.Entity.GetType().Name;
            if (_skipEntities.Contains(entityName)) continue;

            string eventType;
            string? oldJson;
            string? newJson;

            switch (entry.State)
            {
                case EntityState.Added:
                    eventType = AuditEventType.Create;
                    oldJson = null;
                    // PKs generadas por la DB todavía no están aquí (EF usa
                    // un sentinel temporal). Se serializa en SavedChangesAsync.
                    newJson = null;
                    break;

                case EntityState.Modified:
                    if (!entry.Properties.Any(p => p.IsModified)) continue;
                    eventType = AuditEventType.Update;
                    // Para UPDATE: sólo las columnas que cambiaron, con su
                    // valor anterior y nuevo. Hace que el diff sea trivial
                    // de leer en la pantalla de auditoría.
                    oldJson = SerializeProperties(entry, modifiedOnly: true, useOriginal: true);
                    newJson = SerializeProperties(entry, modifiedOnly: true, useOriginal: false);
                    break;

                case EntityState.Deleted:
                    eventType = AuditEventType.Delete;
                    oldJson = SerializeProperties(entry, modifiedOnly: false, useOriginal: true);
                    newJson = null;
                    break;

                default:
                    continue;
            }

            _pending.Add(new PendingAudit
            {
                Entry = entry,
                EntityName = entityName,
                EventType = eventType,
                OldJson = oldJson,
                NewJson = newJson
            });
        }

        return ValueTask.FromResult(result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        var ctx = eventData.Context;
        if (ctx == null || _pending is null || _pending.Count == 0)
        {
            _pending = null;
            return result;
        }

        var pending = _pending;
        _pending = null;

        var (userId, userEmail) = ResolveUser();
        var requestId = ResolveRequestId();
        var ip = ResolveIp();
        var now = DateTime.UtcNow;

        var rows = new List<ChangeAuditLog>(pending.Count);
        foreach (var p in pending)
        {
            var newJson = p.EventType == AuditEventType.Create
                ? SerializeProperties(p.Entry, modifiedOnly: false, useOriginal: false)
                : p.NewJson;

            rows.Add(new ChangeAuditLog
            {
                Ts = now,
                UserId = userId,
                UserEmail = userEmail,
                Module = AuditModuleMap.Resolve(p.EntityName),
                Entity = p.EntityName,
                RecordId = ResolveRecordId(p.Entry),
                EventType = p.EventType,
                OldValue = p.OldJson,
                NewValue = newJson,
                RequestId = requestId,
                Ip = ip
            });
        }

        ctx.Set<ChangeAuditLog>().AddRange(rows);
        await ctx.SaveChangesAsync(cancellationToken);

        return result;
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        _pending = null;
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _pending = null;
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    // ----- helpers --------------------------------------------------------

    private (ulong? UserId, string? Email) ResolveUser()
    {
        var http = _http.HttpContext;
        if (http == null) return (null, null);

        ulong? userId = null;
        if (http.Items.TryGetValue("UserId", out var raw) && raw is ulong u)
            userId = u;

        var email = http.Items.TryGetValue("UserEmail", out var emailRaw)
            ? emailRaw as string
            : null;

        return (userId, email);
    }

    private string? ResolveRequestId()
        => _http.HttpContext?.TraceIdentifier;

    private string? ResolveIp()
        => _http.HttpContext?.Connection.RemoteIpAddress?.ToString();

    private static string? ResolveRecordId(EntityEntry entry)
    {
        var pk = entry.Metadata.FindPrimaryKey();
        if (pk == null) return null;

        var values = pk.Properties
            .Select(p => entry.CurrentValues[p]?.ToString() ?? string.Empty)
            .ToArray();

        return values.Length == 1 ? values[0] : string.Join(":", values);
    }

    private static string SerializeProperties(EntityEntry entry, bool modifiedOnly, bool useOriginal)
    {
        var dict = new SortedDictionary<string, object?>(StringComparer.Ordinal);
        var props = modifiedOnly
            ? entry.Properties.Where(p => p.IsModified)
            : entry.Properties;

        foreach (var p in props)
        {
            // Evita serializar campos sensibles (hash de password, etc.).
            // Si después aparecen más, agregar acá.
            if (string.Equals(p.Metadata.Name, "Password", StringComparison.OrdinalIgnoreCase)) continue;

            dict[p.Metadata.Name] = useOriginal ? p.OriginalValue : p.CurrentValue;
        }

        return JsonSerializer.Serialize(dict, _json);
    }

    private sealed class PendingAudit
    {
        public required EntityEntry Entry { get; init; }
        public required string EntityName { get; init; }
        public required string EventType { get; init; }
        public string? OldJson { get; init; }
        public string? NewJson { get; init; }
    }
}
