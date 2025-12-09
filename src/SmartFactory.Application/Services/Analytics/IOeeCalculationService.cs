using SmartFactory.Application.DTOs.Analytics;

namespace SmartFactory.Application.Services.Analytics;

/// <summary>
/// Service interface for OEE (Overall Equipment Effectiveness) calculations.
/// Provides methods for calculating and analyzing manufacturing efficiency metrics.
/// </summary>
public interface IOeeCalculationService
{
    /// <summary>
    /// Calculates OEE for a specific equipment over a date range.
    /// </summary>
    /// <param name="equipmentId">The equipment ID to calculate OEE for.</param>
    /// <param name="startDate">Start of the calculation period.</param>
    /// <param name="endDate">End of the calculation period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>OEE calculation result.</returns>
    Task<OeeResultDto> CalculateOeeAsync(
        Guid equipmentId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates OEE for an entire factory over a date range.
    /// </summary>
    /// <param name="factoryId">The factory ID to calculate OEE for.</param>
    /// <param name="startDate">Start of the calculation period.</param>
    /// <param name="endDate">End of the calculation period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Factory-wide OEE calculation result.</returns>
    Task<OeeResultDto> CalculateFactoryOeeAsync(
        Guid factoryId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets OEE trend data over a time period with specified granularity.
    /// </summary>
    /// <param name="factoryId">The factory ID to get trend for.</param>
    /// <param name="startDate">Start of the trend period.</param>
    /// <param name="endDate">End of the trend period.</param>
    /// <param name="granularity">Time granularity for data points.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of OEE data points for trending.</returns>
    Task<List<OeeDataPointDto>> GetOeeTrendAsync(
        Guid factoryId,
        DateTime startDate,
        DateTime endDate,
        OeeGranularity granularity = OeeGranularity.Hourly,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets OEE breakdown by loss categories.
    /// </summary>
    /// <param name="factoryId">The factory ID to analyze.</param>
    /// <param name="startDate">Start of the analysis period.</param>
    /// <param name="endDate">End of the analysis period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Loss breakdown analysis.</returns>
    Task<OeeLossBreakdownDto> GetLossBreakdownAsync(
        Guid factoryId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compares OEE across different time periods (shifts, days, weeks).
    /// </summary>
    /// <param name="factoryId">The factory ID to compare.</param>
    /// <param name="comparisonType">Type of comparison (shift, day, week).</param>
    /// <param name="periodCount">Number of periods to compare.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of comparison data.</returns>
    Task<List<OeeComparisonDto>> GetOeeComparisonAsync(
        Guid factoryId,
        OeeComparisonType comparisonType,
        int periodCount = 7,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets OEE results for all equipment in a factory.
    /// </summary>
    /// <param name="factoryId">The factory ID.</param>
    /// <param name="startDate">Start of the calculation period.</param>
    /// <param name="endDate">End of the calculation period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of OEE results per equipment.</returns>
    Task<List<OeeResultDto>> GetEquipmentOeeListAsync(
        Guid factoryId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Time granularity for OEE trend data.
/// </summary>
public enum OeeGranularity
{
    Hourly,
    Shift,
    Daily,
    Weekly,
    Monthly
}

/// <summary>
/// Type of OEE comparison.
/// </summary>
public enum OeeComparisonType
{
    Shift,
    Day,
    Week,
    Month
}
