using Microsoft.EntityFrameworkCore;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Domain.ValueObjects;
using SmartFactory.Infrastructure.Data;

namespace SmartFactory.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Alarm entity.
/// </summary>
public class AlarmRepository : RepositoryBase<Alarm>, IAlarmRepository
{
    public AlarmRepository(SmartFactoryDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Alarm>> GetActiveAlarmsAsync(Guid? factoryId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(a => a.Equipment)
                .ThenInclude(e => e.ProductionLine)
            .Where(a => a.Status != AlarmStatus.Resolved);

        if (factoryId.HasValue)
        {
            query = query.Where(a => a.Equipment.ProductionLine.FactoryId == factoryId.Value);
        }

        return await query
            .OrderByDescending(a => a.Severity)
            .ThenByDescending(a => a.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Alarm>> GetByEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.EquipmentId == equipmentId)
            .OrderByDescending(a => a.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Alarm>> GetBySeverityAsync(AlarmSeverity severity, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.Severity == severity && a.Status != AlarmStatus.Resolved)
            .OrderByDescending(a => a.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Alarm>> GetByDateRangeAsync(DateTimeRange dateRange, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.OccurredAt >= dateRange.Start && a.OccurredAt <= dateRange.End)
            .OrderByDescending(a => a.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Alarm>> GetRecentAlarmsAsync(int count, Guid? factoryId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(a => a.Equipment)
                .ThenInclude(e => e.ProductionLine)
            .AsQueryable();

        if (factoryId.HasValue)
        {
            query = query.Where(a => a.Equipment.ProductionLine.FactoryId == factoryId.Value);
        }

        return await query
            .OrderByDescending(a => a.OccurredAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetActiveAlarmCountAsync(Guid? factoryId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(a => a.Status != AlarmStatus.Resolved);

        if (factoryId.HasValue)
        {
            query = query.Where(a => a.Equipment.ProductionLine.FactoryId == factoryId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<AlarmSummary> GetAlarmSummaryAsync(Guid? factoryId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(a => a.Status != AlarmStatus.Resolved);

        if (factoryId.HasValue)
        {
            query = query.Where(a => a.Equipment.ProductionLine.FactoryId == factoryId.Value);
        }

        // Use database-level aggregation instead of loading all records into memory
        var summary = await query
            .GroupBy(a => 1)
            .Select(g => new AlarmSummary
            {
                TotalActive = g.Count(),
                CriticalCount = g.Count(a => a.Severity == AlarmSeverity.Critical),
                ErrorCount = g.Count(a => a.Severity == AlarmSeverity.Error),
                WarningCount = g.Count(a => a.Severity == AlarmSeverity.Warning),
                InformationCount = g.Count(a => a.Severity == AlarmSeverity.Information),
                AcknowledgedCount = g.Count(a => a.Status == AlarmStatus.Acknowledged),
                UnacknowledgedCount = g.Count(a => a.Status == AlarmStatus.Active)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return summary ?? new AlarmSummary();
    }
}
