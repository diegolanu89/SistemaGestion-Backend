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
}