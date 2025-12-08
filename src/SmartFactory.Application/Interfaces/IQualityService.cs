using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.DTOs.Quality;

namespace SmartFactory.Application.Interfaces;

/// <summary>
/// Service interface for quality operations.
/// </summary>
public interface IQualityService
{
    /// <summary>
    /// Gets a paginated list of quality records.
    /// </summary>
    Task<PagedResult<QualityRecordDto>> GetQualityRecordsAsync(QualityFilterDto filter, PaginationDto pagination, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quality record by ID.
    /// </summary>
    Task<QualityRecordDetailDto?> GetQualityRecordByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records an inspection.
    /// </summary>
    Task<QualityRecordDto> RecordInspectionAsync(QualityRecordCreateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets defect summary.
    /// </summary>
    Task<DefectSummaryDto> GetDefectSummaryAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quality trends.
    /// </summary>
    Task<IEnumerable<QualityTrendDto>> GetQualityTrendsAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates yield rate for a date.
    /// </summary>
    Task<double> CalculateYieldRateAsync(Guid? factoryId, DateTime date, CancellationToken cancellationToken = default);
}
