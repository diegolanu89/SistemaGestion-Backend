using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("project_intake_type_refs")]
public class ProjectIntakeTypeRef
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("code")]
    public string Code { get; set; } = string.Empty;
    [Column("label")]
    public string Label { get; set; } = string.Empty;
    [Column("description")]
    public string? Description { get; set; }
    [Column("internal_label")]
    public string InternalLabel { get; set; } = string.Empty;
    [Column("secondary_label")]
    public string SecondaryLabel { get; set; } = string.Empty;
    [Column("registration_label")]
    public string RegistrationLabel { get; set; } = string.Empty;
    [Column("requires_business_status_date")]
    public bool RequiresBusinessStatusDate { get; set; }
    [Column("requires_actual_end_date")]
    public bool RequiresActualEndDate { get; set; }
    [Column("requires_commercial_fields")]
    public bool RequiresCommercialFields { get; set; }
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
