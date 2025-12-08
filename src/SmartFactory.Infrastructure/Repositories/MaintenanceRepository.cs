using Microsoft.EntityFrameworkCore;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Infrastructure.Data;

namespace SmartFactory.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for MaintenanceRecord entity.
/// </summary>
public class MaintenanceRepository : RepositoryBase<MaintenanceRecord>, IMaintenanceRepository
{
    public MaintenanceRepository(SmartFactoryDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetByEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.Equipment)
            .Where(m => m.EquipmentId == equipmentId)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetByStatusAsync(MaintenanceStatus status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.Equipment)
            .Where(m => m.Status == status)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetOverdueAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Include(m => m.Equipment)
                .ThenInclude(e => e.ProductionLine)
            .Where(m => m.Status == MaintenanceStatus.Scheduled && m.ScheduledDate < now)
            .OrderBy(m => m.ScheduledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetUpcomingAsync(int days, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(days);
        return await DbSet
            .Include(m => m.Equipment)
                .ThenInclude(e => e.ProductionLine)
            .Where(m => m.Status == MaintenanceStatus.Scheduled && m.ScheduledDate >= now && m.ScheduledDate <= cutoff)
            .OrderBy(m => m.ScheduledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.Equipment)
            .Where(m => m.ScheduledDate >= startDate && m.ScheduledDate <= endDate)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetByFactoryAsync(Guid factoryId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.Equipment)
                .ThenInclude(e => e.ProductionLine)
            .Where(m => m.Equipment.ProductionLine.FactoryId == factoryId)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<MaintenanceRecord?> GetWithEquipmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(m => m.Equipment)
                .ThenInclude(e => e.ProductionLine)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }
}
