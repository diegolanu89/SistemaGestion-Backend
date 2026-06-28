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
    [Column("commercial_status")]
    public string? CommercialStatus { get; set; }
    [Column("client_id")]
    public ulong? ClientId { get; set; }
    [Column("leader_timesheet_user_id")]
    public ulong? LeaderTimesheetUserId { get; set; }
    [Column("observations")]
    public string? Observations { get; set; }
    [Column("requires_timesheet_creation")]
    public bool RequiresTimesheetCreation { get; set; } = false;
    [Column("timesheet_record_id")]
    public ulong? TimesheetRecordId { get; set; }
    [Column("project_tracking_id")]
    public ulong? ProjectTrackingId { get; set; }
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
    public TimesheetProject? TimesheetProject { get; set; }
    public TimesheetClient? Client { get; set; }
    public TimesheetUser? LeaderTimesheetUser { get; set; }
    public ProjectTracking? ProjectTracking { get; set; }
}
