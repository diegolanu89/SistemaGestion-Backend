using Microsoft.EntityFrameworkCore;
using bdt_evm_app.Models;

namespace bdt_evm_app.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<PersonalAccessToken> PersonalAccessTokens { get; set; }
    public DbSet<ClockifyClient> ClockifyClients { get; set; }
    public DbSet<PotencialClient> PotencialClients { get; set; }
    public DbSet<ClockifyUser> ClockifyUsers { get; set; }
    public DbSet<ClockifyTimeEntry> ClockifyTimeEntries { get; set; }
    public DbSet<ClockifyProject> ClockifyProjects { get; set; }
    public DbSet<ClockifyProjectFilter> ClockifyProjectFilters { get; set; }
    public DbSet<ChangeRequest> ChangeRequests { get; set; }
    public DbSet<AppUserVisibleProject> AppUserVisibleProjects { get; set; }
    public DbSet<EtcSnapshot> EtcSnapshots { get; set; }
    public DbSet<EtcRecord> EtcRecords { get; set; }
    public DbSet<UserLeader> UserLeaders { get; set; }
    public DbSet<WorkingDaysCalendar> WorkingDaysCalendars { get; set; }
    public DbSet<UserMonthlyCapacity> UserMonthlyCapacities { get; set; }
    public DbSet<UserVacationPeriod> UserVacationPeriods { get; set; }
    public DbSet<PotencialProject> PotencialProjects { get; set; }
    public DbSet<PotencialProjectAllocation> PotencialProjectAllocations { get; set; }
    public DbSet<UserDashboardFilter> UserDashboardFilters { get; set; }
    public DbSet<ProjectIntakeRecord> ProjectIntakeRecords { get; set; }
    public DbSet<ProjectIntakeTypeRef> ProjectIntakeTypeRefs { get; set; }
    public DbSet<ProjectIntakeCategoryRef> ProjectIntakeCategoryRefs { get; set; }
    public DbSet<ProjectIntakeStatusRef> ProjectIntakeStatusRefs { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<PermissionAction> Actions { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<ProfilePermission> ProfilePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClockifyProject>()
            .HasMany(p => p.ChangeRequests)
            .WithOne()
            .HasForeignKey(cr => cr.ProjectId);

        modelBuilder.Entity<ClockifyProject>()
            .HasOne(p => p.Filter)
            .WithOne(f => f.Project)
            .HasForeignKey<ClockifyProjectFilter>(f => f.ProjectId);

        modelBuilder.Entity<ClockifyProject>()
            .HasOne(p => p.Client)
            .WithMany()
            .HasForeignKey(p => p.ClientId);

        modelBuilder.Entity<UserLeader>()
            .HasOne(ul => ul.User)
            .WithMany()
            .HasForeignKey(ul => ul.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserLeader>()
            .HasOne(ul => ul.Leader)
            .WithMany()
            .HasForeignKey(ul => ul.LeaderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PotencialProject>()
            .HasOne(p => p.PotencialClient)
            .WithMany()
            .HasForeignKey(p => p.PotencialClientId);

        modelBuilder.Entity<PotencialProject>()
            .HasMany(p => p.Allocations)
            .WithOne()
            .HasForeignKey(a => a.PotencialProjectId);

        modelBuilder.Entity<PotencialProjectAllocation>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId);

        modelBuilder.Entity<AppUserVisibleProject>()
            .HasOne(v => v.Project)
            .WithMany()
            .HasForeignKey(v => v.ProjectId)
            .HasPrincipalKey(p => p.Id);

        modelBuilder.Entity<ProjectIntakeRecord>()
            .HasOne(r => r.TypeRef)
            .WithMany()
            .HasForeignKey(r => r.ProjectType)
            .HasPrincipalKey(t => t.Code)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjectIntakeRecord>()
            .HasOne(r => r.CategoryRef)
            .WithMany()
            .HasForeignKey(r => r.CategoryCode)
            .HasPrincipalKey(c => c.Code)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjectIntakeRecord>()
            .HasOne(r => r.StatusRef)
            .WithMany()
            .HasForeignKey(r => r.ProjectStatusCode)
            .HasPrincipalKey(s => s.Code)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjectIntakeRecord>()
            .HasOne(r => r.ClockifyProject)
            .WithMany()
            .HasForeignKey(r => r.ClockifyRecordId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ProjectIntakeRecord>()
            .HasOne(r => r.Client)
            .WithMany()
            .HasForeignKey(r => r.ClientId)
            .OnDelete(DeleteBehavior.SetNull);

        // RBAC normalizado — Module/PermissionAction/Permission/ProfilePermission
        modelBuilder.Entity<Permission>()
            .HasOne(p => p.Module)
            .WithMany()
            .HasForeignKey(p => p.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProfilePermission>()
            .HasKey(pp => new { pp.ProfileId, pp.PermissionId });

        modelBuilder.Entity<ProfilePermission>()
            .HasOne(pp => pp.Profile)
            .WithMany()
            .HasForeignKey(pp => pp.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProfilePermission>()
            .HasOne(pp => pp.Permission)
            .WithMany()
            .HasForeignKey(pp => pp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProfilePermission>()
            .HasOne(pp => pp.Action)
            .WithMany()
            .HasForeignKey(pp => pp.ActionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}