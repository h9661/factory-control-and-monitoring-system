using Microsoft.EntityFrameworkCore;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Domain.ValueObjects;
using SmartFactory.Infrastructure.Data;

namespace SmartFactory.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for WorkOrder entity.
/// </summary>
public class WorkOrderRepository : RepositoryBase<WorkOrder>, IWorkOrderRepository
{
    public WorkOrderRepository(SmartFactoryDbContext context) : base(context)
    {
    }

    public async Task<WorkOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(w => w.Factory)
            .FirstOrDefaultAsync(w => w.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<IEnumerable<WorkOrder>> GetByFactoryAsync(Guid factoryId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(w => w.FactoryId == factoryId)
            .OrderByDescending(w => w.ScheduledStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkOrder>> GetByStatusAsync(WorkOrderStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(w => w.Status == status)
            .OrderByDescending(w => w.ScheduledStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkOrder>> GetByDateRangeAsync(DateTimeRange dateRange, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(w => w.ScheduledStart >= dateRange.Start && w.ScheduledStart <= dateRange.End)
            .OrderByDescending(w => w.ScheduledStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkOrder>> GetActiveWorkOrdersAsync(Guid? factoryId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(w => w.Factory)
            .Where(w => w.Status == WorkOrderStatus.InProgress || w.Status == WorkOrderStatus.Scheduled);

        if (factoryId.HasValue)
        {
            query = query.Where(w => w.FactoryId == factoryId.Value);
        }

        return await query
            .OrderBy(w => w.Priority)
            .ThenBy(w => w.ScheduledStart)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkOrder?> GetWithStepsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(w => w.Factory)
            .Include(w => w.Steps)
                .ThenInclude(s => s.Equipment)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<ProductionSummary> GetProductionSummaryAsync(Guid? factoryId, DateTime date, CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        var query = DbSet.Where(w => w.ScheduledStart <= endOfDay && w.ScheduledEnd >= startOfDay);

        if (factoryId.HasValue)
        {
            query = query.Where(w => w.FactoryId == factoryId.Value);
        }

        var workOrders = await query.ToListAsync(cancellationToken);

        return new ProductionSummary
        {
            Date = date,
            TotalWorkOrders = workOrders.Count,
            CompletedWorkOrders = workOrders.Count(w => w.Status == WorkOrderStatus.Completed),
            InProgressWorkOrders = workOrders.Count(w => w.Status == WorkOrderStatus.InProgress),
            TargetUnits = workOrders.Sum(w => w.TargetQuantity),
            CompletedUnits = workOrders.Sum(w => w.CompletedQuantity),
            DefectUnits = workOrders.Sum(w => w.DefectQuantity)
        };
    }
}
