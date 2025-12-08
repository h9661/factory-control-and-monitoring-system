using SmartFactory.Application.DTOs.Reports;

namespace SmartFactory.Application.Interfaces;

/// <summary>
/// Service interface for report generation.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Generates OEE report.
    /// </summary>
    Task<OeeReportDto> GenerateOeeReportAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates production report.
    /// </summary>
    Task<ProductionReportDto> GenerateProductionReportAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates quality report.
    /// </summary>
    Task<QualityReportDto> GenerateQualityReportAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates maintenance report.
    /// </summary>
    Task<MaintenanceReportDto> GenerateMaintenanceReportAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets equipment efficiency data.
    /// </summary>
    Task<IEnumerable<EquipmentEfficiencyDto>> GetEquipmentEfficiencyAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
