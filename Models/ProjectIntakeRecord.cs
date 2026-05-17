using System.ComponentModel.DataAnnotations.Schema;

namespace bdt_evm_app.Models;

[Table("project_intake_records")]
public class ProjectIntakeRecord
{
    [Column("id")]
    public ulong Id { get; set; }
    [Column("project_type")]
    public string? ProjectType { get; set; }
    [Column("internal_project_number")]
    public string? InternalProjectNumber { get; set; }
    [Column("secondary_project_number")]
    public string? SecondaryProjectNumber { get; set; }
    [Column("registration_date")]
    public DateOnly? RegistrationDate { get; set; }
    [Column("client_name")]
    public string? ClientName { get; set; }
    [Column("project_name")]
    public string? ProjectName { get; set; }
    [Column("category_code")]
    public string? CategoryCode { get; set; }
    [Column("project_status_code")]
    public string? ProjectStatusCode { get; set; }
    [Column("business_status_date")]
    public DateOnly? BusinessStatusDate { get; set; }
    [Column("estimated_end_date")]
    public DateOnly? EstimatedEndDate { get; set; }
    [Column("actual_end_date")]
    public DateOnly? ActualEndDate { get; set; }
    [Column("commercial_status")]
    public string? CommercialStatus { get; set; }
    [Column("client_id")]
    public ulong? ClientId { get; set; }
    [Column("leader_clockify_user_id")]
    public ulong? LeaderClockifyUserId { get; set; }
    [Column("observations")]
    public string? Observations { get; set; }
    [Column("requires_clockify_creation")]
    public bool RequiresClockifyCreation { get; set; } = false;
    [Column("clockify_record_id")]
    public ulong? ClockifyRecordId { get; set; }
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
    [Column("created_by")]
    public ulong? CreatedBy { get; set; }
    [Column("updated_by")]
    public ulong? UpdatedBy { get; set; }
    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    // Navigation properties
    public ProjectIntakeTypeRef? TypeRef { get; set; }
    public ProjectIntakeCategoryRef? CategoryRef { get; set; }
    public ProjectIntakeStatusRef? StatusRef { get; set; }
    public ClockifyProject? ClockifyProject { get; set; }
    public ClockifyClient? Client { get; set; }
    public ClockifyUser? LeaderClockifyUser { get; set; }
}
