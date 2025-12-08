using Microsoft.EntityFrameworkCore;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Infrastructure.Data;

namespace SmartFactory.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for QualityRecord entity.
/// </summary>
public class QualityRecordRepository : RepositoryBase<QualityRecord>, IQualityRecordRepository
{
    public QualityRecordRepository(SmartFactoryDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<QualityRecord>> GetByEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(q => q.Equipment)
            .Include(q => q.WorkOrder)
            .Where(q => q.EquipmentId == equipmentId)
            .OrderByDescending(q => q.InspectedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<QualityRecord>> GetByWorkOrderAsync(Guid workOrderId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(q => q.Equipment)
            .Where(q => q.WorkOrderId == workOrderId)
            .OrderByDescending(q => q.InspectedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<QualityRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(q => q.Equipment)
            .Include(q => q.WorkOrder)
            .Where(q => q.InspectedAt >= startDate && q.InspectedAt <= endDate)
            .OrderByDescending(q => q.InspectedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<QualityRecord>> GetByResultAsync(InspectionResult result, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(q => q.Equipment)
            .Where(q => q.Result == result)
            .OrderByDescending(q => q.InspectedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<QualityRecord>> GetByFactoryAsync(Guid factoryId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(q => q.Equipment)
                .ThenInclude(e => e.ProductionLine)
            .Include(q => q.WorkOrder)
            .Where(q => q.Equipment.ProductionLine.FactoryId == factoryId)
            .OrderByDescending(q => q.InspectedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<QualityRecord?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(q => q.Equipment)
                .ThenInclude(e => e.ProductionLine)
            .Include(q => q.WorkOrder)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    public async Task<DefectStatistics> GetDefectStatisticsAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(q => q.Equipment)
                .ThenInclude(e => e.ProductionLine)
            .Where(q => q.InspectedAt >= startDate && q.InspectedAt <= endDate);

        if (factoryId.HasValue)
        {
            query = query.Where(q => q.Equipment.ProductionLine.FactoryId == factoryId.Value);
        }

        var records = await query.ToListAsync(cancellationToken);

        var stats = new DefectStatistics
        {
            TotalInspections = records.Count,
            PassCount = records.Count(r => r.Result == InspectionResult.Pass),
            FailCount = records.Count(r => r.Result == InspectionResult.Fail),
            TotalDefects = records.Where(r => r.DefectCount.HasValue).Sum(r => r.DefectCount!.Value),
            DefectsByType = records
                .Where(r => r.DefectType.HasValue)
                .GroupBy(r => r.DefectType!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.DefectCount ?? 1))
        };

        stats.PassRate = stats.TotalInspections > 0
            ? (double)stats.PassCount / stats.TotalInspections * 100
            : 0;

        return stats;
    }
}
