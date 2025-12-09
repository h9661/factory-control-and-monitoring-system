using SmartFactory.Application.DTOs.Maintenance;

namespace SmartFactory.Application.Services.Maintenance;

/// <summary>
/// Service interface for predictive maintenance calculations and anomaly detection.
/// </summary>
public interface IPredictiveMaintenanceService
{
    /// <summary>
    /// Calculates the health score for a specific equipment.
    /// </summary>
    /// <param name="equipmentId">Equipment ID to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Health score result.</returns>
    Task<HealthScoreDto> GetHealthScoreAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets health scores for all equipment in a factory.
    /// </summary>
    /// <param name="factoryId">Factory ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of health scores ordered by risk.</returns>
    Task<List<HealthScoreDto>> GetFactoryHealthScoresAsync(
        Guid factoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Predicts maintenance requirements for equipment.
    /// </summary>
    /// <param name="equipmentId">Equipment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Maintenance prediction result.</returns>
    Task<MaintenancePredictionDto> PredictMaintenanceAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets maintenance predictions for all equipment in a factory.
    /// </summary>
    /// <param name="factoryId">Factory ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of maintenance predictions.</returns>
    Task<List<MaintenancePredictionDto>> GetFactoryMaintenancePredictionsAsync(
        Guid factoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detects anomalies for a specific equipment.
    /// </summary>
    /// <param name="equipmentId">Equipment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of detected anomalies.</returns>
    Task<List<AnomalyResultDto>> DetectAnomaliesAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active anomalies in a factory.
    /// </summary>
    /// <param name="factoryId">Factory ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active anomalies.</returns>
    Task<List<AnomalyResultDto>> GetActiveAnomaliesAsync(
        Guid factoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets health trend data for charting.
    /// </summary>
    /// <param name="equipmentId">Equipment ID.</param>
    /// <param name="startDate">Start of trend period.</param>
    /// <param name="endDate">End of trend period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of health trend points.</returns>
    Task<List<HealthTrendPointDto>> GetHealthTrendAsync(
        Guid equipmentId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets equipment ranking by risk level.
    /// </summary>
    /// <param name="factoryId">Factory ID.</param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of equipment ordered by risk (highest first).</returns>
    Task<List<HealthScoreDto>> GetRiskRankingAsync(
        Guid factoryId,
        int limit = 10,
        CancellationToken cancellationToken = default);
}
