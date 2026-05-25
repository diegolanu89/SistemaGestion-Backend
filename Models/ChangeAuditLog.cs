using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("change_audit_log")]
public class ChangeAuditLog
{
    [Column("id")]
    public ulong Id { get; set; }

    [Column("ts")]
    public DateTime Ts { get; set; }

    [Column("user_id")]
    public ulong? UserId { get; set; }

    [Column("user_email")]
    public string? UserEmail { get; set; }

    [Column("module")]
    public string? Module { get; set; }

    [Column("entity")]
    public string Entity { get; set; } = string.Empty;

    [Column("record_id")]
    public string? RecordId { get; set; }

    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    [Column("old_value")]
    public string? OldValue { get; set; }

    [Column("new_value")]
    public string? NewValue { get; set; }

    [Column("request_id")]
    public string? RequestId { get; set; }

    [Column("ip")]
    public string? Ip { get; set; }
}

public static class AuditEventType
{
    public const string Create = "create";
    public const string Update = "update";
    public const string Delete = "delete";
    public const string Login  = "login";
    public const string Logout = "logout";
}
