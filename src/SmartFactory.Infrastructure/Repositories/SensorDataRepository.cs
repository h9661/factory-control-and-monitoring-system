using Microsoft.EntityFrameworkCore;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Infrastructure.Data;

namespace SmartFactory.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for SensorData entity.
/// Optimized for high-volume time-series operations.
/// </summary>
public class SensorDataRepository : ISensorDataRepository
{
    private readonly SmartFactoryDbContext _context;

    public SensorDataRepository(SmartFactoryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SensorData>> GetByEquipmentAsync(Guid equipmentId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.SensorData
            .Where(s => s.EquipmentId == equipmentId && s.Timestamp >= startDate && s.Timestamp <= endDate)
            .OrderBy(s => s.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<SensorData?> GetLatestByTagAsync(Guid equipmentId, string tagName, CancellationToken cancellationToken = default)
    {
        return await _context.SensorData
            .Where(s => s.EquipmentId == equipmentId && s.TagName == tagName)
            .OrderByDescending(s => s.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<SensorData>> GetLatestByEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        // Get the latest reading for each tag
        var latestByTag = await _context.SensorData
            .Where(s => s.EquipmentId == equipmentId)
            .GroupBy(s => s.TagName)
            .Select(g => g.OrderByDescending(s => s.Timestamp).First())
            .ToListAsync(cancellationToken);

        return latestByTag;
    }

    public async Task AddAsync(SensorData sensorData, CancellationToken cancellationToken = default)
    {
        await _context.SensorData.AddAsync(sensorData, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddBatchAsync(IEnumerable<SensorData> sensorData, CancellationToken cancellationToken = default)
    {
        await _context.SensorData.AddRangeAsync(sensorData, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<double?> GetAverageValueAsync(Guid equipmentId, string tagName, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var values = await _context.SensorData
            .Where(s => s.EquipmentId == equipmentId && s.TagName == tagName && s.Timestamp >= startDate && s.Timestamp <= endDate)
            .Select(s => s.Value)
            .ToListAsync(cancellationToken);

        return values.Count > 0 ? values.Average() : null;
    }

    public async Task<SensorDataStatistics?> GetStatisticsAsync(Guid equipmentId, string tagName, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var data = await _context.SensorData
            .Where(s => s.EquipmentId == equipmentId && s.TagName == tagName && s.Timestamp >= startDate && s.Timestamp <= endDate)
            .ToListAsync(cancellationToken);

        if (!data.Any())
            return null;

        return new SensorDataStatistics
        {
            MinValue = data.Min(s => s.Value),
            MaxValue = data.Max(s => s.Value),
            AverageValue = data.Average(s => s.Value),
            DataPointCount = data.Count,
            StartTime = data.Min(s => s.Timestamp),
            EndTime = data.Max(s => s.Timestamp)
        };
    }

    public async Task<int> DeleteOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        var oldData = _context.SensorData.Where(s => s.Timestamp < cutoffDate);
        var count = await oldData.CountAsync(cancellationToken);
        _context.SensorData.RemoveRange(oldData);
        await _context.SaveChangesAsync(cancellationToken);
        return count;
    }
}
