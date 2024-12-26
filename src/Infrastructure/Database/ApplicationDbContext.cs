using Application.Abstractions.Data;
using Domain.Entities;
using Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Terminal> Terminals { get; set; } = null!;
    public DbSet<Operator> Operators { get; set; } = null!;
    public DbSet<OperatorTerminal> OperatorTerminalSessions { get; set; } = null!;
    public DbSet<Job> Jobs { get; set; } = null!;
    public DbSet<JobInProgress> JobsInProgress { get; set; } = null!;
    public DbSet<JobItem> JobItems { get; set; } = null!;
    // public DbSet<ProductStock> ProductStocks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.ApplyConfiguration(new TerminalConfiguration());
        modelBuilder.ApplyConfiguration(new OperatorConfiguration());
        modelBuilder.ApplyConfiguration(new OperatorTerminalConfiguration());
        modelBuilder.ApplyConfiguration(new JobConfiguration());
        modelBuilder.ApplyConfiguration(new JobInProgressConfiguration());
        modelBuilder.ApplyConfiguration(new JobItemConfiguration());
        // modelBuilder.ApplyConfiguration(new ProductStockConfiguration());
    }
}
