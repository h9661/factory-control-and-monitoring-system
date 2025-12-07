using Microsoft.EntityFrameworkCore;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Infrastructure.Data;

namespace SmartFactory.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Equipment entity.
/// </summary>
public class EquipmentRepository : RepositoryBase<Equipment>, IEquipmentRepository
{
    public EquipmentRepository(SmartFactoryDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Equipment>> GetByProductionLineAsync(Guid lineId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.ProductionLineId == lineId && e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Equipment>> GetByFactoryAsync(Guid factoryId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.ProductionLine)
            .Where(e => e.ProductionLine.FactoryId == factoryId && e.IsActive)
            .OrderBy(e => e.ProductionLine.Sequence)
            .ThenBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Equipment>> GetByStatusAsync(EquipmentStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.Status == status && e.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Equipment?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.ProductionLine)
                .ThenInclude(pl => pl.Factory)
            .Include(e => e.Alarms.Where(a => a.Status != AlarmStatus.Resolved).OrderByDescending(a => a.OccurredAt).Take(10))
            .Include(e => e.MaintenanceRecords.OrderByDescending(m => m.ScheduledDate).Take(5))
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Equipment>> GetEquipmentDueForMaintenanceAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(e => e.IsActive &&
                        e.MaintenanceIntervalDays.HasValue &&
                        e.LastMaintenanceDate.HasValue &&
                        e.LastMaintenanceDate.Value.AddDays(e.MaintenanceIntervalDays.Value) <= now)
            .Include(e => e.ProductionLine)
            .ToListAsync(cancellationToken);
    }

    public async Task<EquipmentStatusSummary> GetStatusSummaryAsync(Guid? factoryId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(e => e.IsActive);

        if (factoryId.HasValue)
        {
            query = query.Where(e => e.ProductionLine.FactoryId == factoryId.Value);
        }

        var counts = await query
            .GroupBy(e => e.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new EquipmentStatusSummary
        {
            TotalCount = counts.Sum(c => c.Count),
            RunningCount = counts.FirstOrDefault(c => c.Status == EquipmentStatus.Running)?.Count ?? 0,
            IdleCount = counts.FirstOrDefault(c => c.Status == EquipmentStatus.Idle)?.Count ?? 0,
            WarningCount = counts.FirstOrDefault(c => c.Status == EquipmentStatus.Warning)?.Count ?? 0,
            ErrorCount = counts.FirstOrDefault(c => c.Status == EquipmentStatus.Error)?.Count ?? 0,
            MaintenanceCount = counts.FirstOrDefault(c => c.Status == EquipmentStatus.Maintenance)?.Count ?? 0,
            OfflineCount = counts.FirstOrDefault(c => c.Status == EquipmentStatus.Offline)?.Count ?? 0
        };
    }
}
