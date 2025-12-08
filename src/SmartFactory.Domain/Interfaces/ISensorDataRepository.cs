using SmartFactory.Domain.Entities;

namespace SmartFactory.Domain.Interfaces;

/// <summary>
/// Repository interface for SensorData entity operations.
/// Optimized for high-volume time-series data.
/// </summary>
public interface ISensorDataRepository
{
    /// <summary>
    /// Gets sensor data for an equipment within a date range.
    /// </summary>
    Task<IEnumerable<SensorData>> GetByEquipmentAsync(Guid equipmentId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest sensor reading for a specific tag.
    /// </summary>
    Task<SensorData?> GetLatestByTagAsync(Guid equipmentId, string tagName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all latest sensor readings for an equipment.
    /// </summary>
    Task<IEnumerable<SensorData>> GetLatestByEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a single sensor data reading.
    /// </summary>
    Task AddAsync(SensorData sensorData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a batch of sensor data readings (optimized for bulk insert).
    /// </summary>
    Task AddBatchAsync(IEnumerable<SensorData> sensorData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets average value for a tag within a date range.
    /// </summary>
    Task<double?> GetAverageValueAsync(Guid equipmentId, string tagName, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets min/max/average statistics for a tag.
    /// </summary>
    Task<SensorDataStatistics?> GetStatisticsAsync(Guid equipmentId, string tagName, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes sensor data older than specified date (for data retention).
    /// </summary>
    Task<int> DeleteOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Sensor data statistics.
/// </summary>
public class SensorDataStatistics
{
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public double AverageValue { get; set; }
    public int DataPointCount { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
