using Microsoft.EntityFrameworkCore;

namespace FleetFuel.Data;

/// <summary>
/// Database context for FleetFuel application.
/// Uses SQLite for MVP, designed for migration to PostgreSQL.
/// </summary>
public class FleetFuelDbContext : DbContext
{
    public FleetFuelDbContext(DbContextOptions<FleetFuelDbContext> options)
        : base(options)
    {
    }

    // DbSets for entities
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<Trip> Trips { get; set; } = null!;
    public DbSet<Receipt> Receipts { get; set; } = null!;
    public DbSet<YearSummary> YearSummaries { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<UserSubscription> UserSubscriptions { get; set; } = null!;
    public DbSet<SyncOperation> SyncOperations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FleetFuelDbContext).Assembly);

        // Soft delete query filter for all entities
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Vehicle>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Trip>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Receipt>().HasQueryFilter(e => !e.IsDeleted);
    }
}