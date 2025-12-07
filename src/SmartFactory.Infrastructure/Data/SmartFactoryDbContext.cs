using Microsoft.EntityFrameworkCore;
using SmartFactory.Domain.Entities;

namespace SmartFactory.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for Smart Factory System.
/// </summary>
public class SmartFactoryDbContext : DbContext
{
    public SmartFactoryDbContext(DbContextOptions<SmartFactoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Factory> Factories => Set<Factory>();
    public DbSet<ProductionLine> ProductionLines => Set<ProductionLine>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<SensorData> SensorData => Set<SensorData>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderStep> WorkOrderSteps => Set<WorkOrderStep>();
    public DbSet<QualityRecord> QualityRecords => Set<QualityRecord>();
    public DbSet<Alarm> Alarms => Set<Alarm>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartFactoryDbContext).Assembly);

        // Global query filter for soft delete on Factory
        modelBuilder.Entity<Factory>().HasQueryFilter(f => !f.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Domain.Common.BaseEntity entity)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }
    }
}
