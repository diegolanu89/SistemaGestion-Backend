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
    }
}