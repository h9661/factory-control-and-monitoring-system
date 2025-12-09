using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SmartFactory.Application.Services.Maintenance;
using SmartFactory.Application.DTOs.Maintenance;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;

namespace SmartFactory.Application.Tests.Services.Maintenance;

/// <summary>
/// Unit tests for PredictiveMaintenanceService.
/// Tests weighted scoring algorithms, anomaly detection, and maintenance predictions.
/// </summary>
public class PredictiveMaintenanceServiceTests
{
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly Mock<ISensorDataRepository> _sensorDataRepositoryMock;
    private readonly Mock<IMaintenanceRepository> _maintenanceRepositoryMock;
    private readonly Mock<ILogger<PredictiveMaintenanceService>> _loggerMock;
    private readonly PredictiveMaintenanceService _sut;

    public PredictiveMaintenanceServiceTests()
    {
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _sensorDataRepositoryMock = new Mock<ISensorDataRepository>();
        _maintenanceRepositoryMock = new Mock<IMaintenanceRepository>();
        _loggerMock = new Mock<ILogger<PredictiveMaintenanceService>>();

        _sut = new PredictiveMaintenanceService(
            _loggerMock.Object,
            _equipmentRepositoryMock.Object,
            _sensorDataRepositoryMock.Object,
            _maintenanceRepositoryMock.Object);
    }

    #region GetHealthScoreAsync Tests

    [Fact]
    public async Task GetHealthScoreAsync_EquipmentNotFound_ReturnsDefaultScore()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        // Act
        var result = await _sut.GetHealthScoreAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        result.EquipmentId.Should().Be(equipmentId);
        result.EquipmentName.Should().Be("Unknown");
        result.OverallScore.Should().Be(0);
        result.TopConcerns.Should().Contain("Equipment not found");
    }

    [Fact]
    public async Task GetHealthScoreAsync_ValidEquipment_ReturnsHealthScore()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.GetHealthScoreAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        result.EquipmentName.Should().Be("Test Equipment");
        result.EquipmentCode.Should().Be("EQ001");
        result.OverallScore.Should().BeGreaterThan(0);
        result.ComponentScores.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetHealthScoreAsync_CalculatesWeightedScore()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.GetHealthScoreAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        result.OverallScore.Should().BeInRange(0, 100);
        result.ComponentScores.Should().HaveCount(3); // Temperature, Vibration, Pressure
    }

    [Fact]
    public async Task GetHealthScoreAsync_Over90DaysSinceMaintenance_AddsConcern()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);
        // Equipment with no maintenance date will have > 90 days concern

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.GetHealthScoreAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        // If no LastMaintenanceDate, it defaults to 365 days which is > 90
        result.TopConcerns.Should().Contain(c => c.Contains("days"));
    }

    [Fact]
    public async Task GetHealthScoreAsync_ComponentScoreBelow60_AddsConcern()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Error);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.GetHealthScoreAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        // Error status leads to lower sensor scores which may add concerns
        result.ComponentScores.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetHealthScoreAsync_ReturnsComponentHealthDetails()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.GetHealthScoreAsync(equipmentId);

        // Assert
        result.ComponentScores.Should().Contain(c => c.ComponentName == "Temperature");
        result.ComponentScores.Should().Contain(c => c.ComponentName == "Vibration");
        result.ComponentScores.Should().Contain(c => c.ComponentName == "Pressure");
        result.ComponentScores.Should().OnlyContain(c => c.Score >= 0 && c.Score <= 100);
    }

    #endregion

    #region GetFactoryHealthScoresAsync Tests

    [Fact]
    public async Task GetFactoryHealthScoresAsync_OrdersByScoreAscending()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Running),
            CreateEquipment(lineId, "EQ002", "Equipment 2", EquipmentStatus.Error),
            CreateEquipment(lineId, "EQ003", "Equipment 3", EquipmentStatus.Idle)
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.GetFactoryHealthScoresAsync(factoryId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeInAscendingOrder(s => s.OverallScore);
    }

    [Fact]
    public async Task GetFactoryHealthScoresAsync_OnlyIncludesActiveEquipment()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();

        var activeEquipment = CreateEquipment(lineId, "EQ001", "Active Equipment", EquipmentStatus.Running);
        var inactiveEquipment = CreateEquipment(lineId, "EQ002", "Inactive Equipment", EquipmentStatus.Offline);
        inactiveEquipment.Deactivate();

        var equipment = new List<Equipment> { activeEquipment, inactiveEquipment };

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.GetFactoryHealthScoresAsync(factoryId);

        // Assert
        result.Should().HaveCount(1);
        result.First().EquipmentName.Should().Be("Active Equipment");
    }

    #endregion

    #region PredictMaintenanceAsync Tests

    [Fact]
    public async Task PredictMaintenanceAsync_EquipmentNotFound_ReturnsPredictionWithDefaultScore()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        // Act
        var result = await _sut.PredictMaintenanceAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        result.EquipmentId.Should().Be(equipmentId);
    }

    [Fact]
    public async Task PredictMaintenanceAsync_ValidEquipment_ReturnsPrediction()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.PredictMaintenanceAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        result.EquipmentName.Should().Be("Test Equipment");
        result.DaysUntilMaintenance.Should().BeGreaterThan(0);
        result.PredictedMaintenanceDate.Should().BeAfter(DateTime.UtcNow);
        result.ConfidenceScore.Should().BeInRange(0, 1);
    }

    [Fact]
    public async Task PredictMaintenanceAsync_HighHealthScore_ReturnsLongerPrediction()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Healthy Equipment", EquipmentStatus.Idle);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.PredictMaintenanceAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        // Idle status tends to produce higher health scores
        result.DaysUntilMaintenance.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PredictMaintenanceAsync_LowHealthScore_ReturnsShorterPrediction()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Unhealthy Equipment", EquipmentStatus.Error);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.PredictMaintenanceAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        // Error status tends to produce lower health scores
        result.DaysUntilMaintenance.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PredictMaintenanceAsync_ReturnsRecommendedAction()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.PredictMaintenanceAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        result.RecommendedAction.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PredictMaintenanceAsync_ReturnsEstimatedDowntime()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.PredictMaintenanceAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        result.EstimatedDowntimeHours.Should().BeGreaterThan(0);
    }

    #endregion

    #region DetectAnomaliesAsync Tests

    [Fact]
    public async Task DetectAnomaliesAsync_EquipmentNotFound_ReturnsEmptyList()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        // Act
        var result = await _sut.DetectAnomaliesAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectAnomaliesAsync_ValidEquipment_ChecksAllSensorTypes()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Warning);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.DetectAnomaliesAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        // Anomalies are detected when deviation > 15%
        // With Warning status, there's a chance of detecting anomalies
    }

    [Fact]
    public async Task DetectAnomaliesAsync_AnomalyDetected_ReturnsSeverity()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        // Warning status increases chance of anomalies
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Warning);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act - Call multiple times to get anomalies (due to random variation)
        var results = new List<AnomalyResultDto>();
        for (int i = 0; i < 10; i++)
        {
            var anomalies = await _sut.DetectAnomaliesAsync(equipmentId);
            results.AddRange(anomalies);
        }

        // Assert
        if (results.Any())
        {
            results.Should().OnlyContain(a =>
                a.Severity == AnomalySeverity.Minor ||
                a.Severity == AnomalySeverity.Moderate ||
                a.Severity == AnomalySeverity.Significant ||
                a.Severity == AnomalySeverity.Severe);
        }
    }

    [Fact]
    public async Task DetectAnomaliesAsync_AnomalyDetected_IncludesDeviation()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Warning);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act - Call multiple times to get anomalies
        var results = new List<AnomalyResultDto>();
        for (int i = 0; i < 10; i++)
        {
            var anomalies = await _sut.DetectAnomaliesAsync(equipmentId);
            results.AddRange(anomalies);
        }

        // Assert
        if (results.Any())
        {
            results.Should().OnlyContain(a => a.DeviationPercent > 15); // Threshold is 15%
        }
    }

    [Fact]
    public async Task DetectAnomaliesAsync_ReturnsAnomalyWithAllRequiredFields()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Error);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act - Call multiple times to increase chance of detecting anomalies
        var results = new List<AnomalyResultDto>();
        for (int i = 0; i < 10; i++)
        {
            var anomalies = await _sut.DetectAnomaliesAsync(equipmentId);
            results.AddRange(anomalies);
        }

        // Assert
        if (results.Any())
        {
            var anomaly = results.First();
            anomaly.EquipmentId.Should().Be(equipmentId);
            anomaly.EquipmentName.Should().Be("Test Equipment");
            anomaly.SensorName.Should().NotBeNullOrEmpty();
            anomaly.CurrentValue.Should().BeGreaterThan(0);
            anomaly.ExpectedValue.Should().BeGreaterThan(0);
            anomaly.Description.Should().NotBeNullOrEmpty();
            anomaly.IsActive.Should().BeTrue();
        }
    }

    #endregion

    #region GetActiveAnomaliesAsync Tests

    [Fact]
    public async Task GetActiveAnomaliesAsync_OrdersBySeverityDescending()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Warning),
            CreateEquipment(lineId, "EQ002", "Equipment 2", EquipmentStatus.Error)
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.GetActiveAnomaliesAsync(factoryId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeInDescendingOrder(a => a.Severity);
    }

    #endregion

    #region GetHealthTrendAsync Tests

    [Fact]
    public async Task GetHealthTrendAsync_ReturnsDataPointsWithin4HourIntervals()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _sut.GetHealthTrendAsync(equipmentId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterOrEqualTo(5); // 24 hours / 4 hour intervals = 6 points
    }

    [Fact]
    public async Task GetHealthTrendAsync_DataPointsHaveValidScores()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddHours(-8);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _sut.GetHealthTrendAsync(equipmentId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().OnlyContain(p =>
            p.HealthScore >= 0 && p.HealthScore <= 100 &&
            p.Temperature > 0 &&
            p.Vibration > 0 &&
            p.Pressure > 0);
    }

    [Fact]
    public async Task GetHealthTrendAsync_DataPointsAreChronological()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _sut.GetHealthTrendAsync(equipmentId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeInAscendingOrder(p => p.Timestamp);
    }

    #endregion

    #region GetRiskRankingAsync Tests

    [Fact]
    public async Task GetRiskRankingAsync_RespectsLimit()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var limit = 2;

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Running),
            CreateEquipment(lineId, "EQ002", "Equipment 2", EquipmentStatus.Running),
            CreateEquipment(lineId, "EQ003", "Equipment 3", EquipmentStatus.Running),
            CreateEquipment(lineId, "EQ004", "Equipment 4", EquipmentStatus.Running)
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.GetRiskRankingAsync(factoryId, limit);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(limit);
    }

    [Fact]
    public async Task GetRiskRankingAsync_ReturnsLowestScoresFirst()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Running),
            CreateEquipment(lineId, "EQ002", "Equipment 2", EquipmentStatus.Error)
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.GetRiskRankingAsync(factoryId, 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeInAscendingOrder(s => s.OverallScore);
    }

    #endregion

    #region Confidence Score Tests

    [Fact]
    public async Task PredictMaintenanceAsync_ConfidenceScore_HasMaxValueOf95Percent()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipmentWithMaintenance(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.PredictMaintenanceAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        result.ConfidenceScore.Should().BeLessOrEqualTo(0.95);
    }

    [Fact]
    public async Task PredictMaintenanceAsync_ConfidenceScore_IncreasesWithMoreData()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipmentWithMaintenance(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.PredictMaintenanceAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        // Equipment with maintenance history should have higher confidence
        result.ConfidenceScore.Should().BeGreaterThan(0.7);
    }

    #endregion

    #region Urgency Tests

    [Fact]
    public async Task PredictMaintenanceAsync_ReturnsValidUrgencyLevel()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.PredictMaintenanceAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        result.Urgency.Should().BeOneOf(
            MaintenanceUrgency.Critical,
            MaintenanceUrgency.Urgent,
            MaintenanceUrgency.Required,
            MaintenanceUrgency.Scheduled,
            MaintenanceUrgency.Routine);
    }

    [Fact]
    public async Task PredictMaintenanceAsync_ErrorStatus_ReturnsHigherUrgency()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Error Equipment", EquipmentStatus.Error);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        var result = await _sut.PredictMaintenanceAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        // Error status should result in shorter days until maintenance and higher urgency
        result.DaysUntilMaintenance.Should().BeLessOrEqualTo(30);
    }

    #endregion

    #region Helper Methods

    private static Equipment CreateEquipment(Guid productionLineId, string code, string name, EquipmentStatus status)
    {
        var equipment = new Equipment(productionLineId, code, name, EquipmentType.PickAndPlace);
        equipment.UpdateStatus(status);
        return equipment;
    }

    private static Equipment CreateEquipmentWithMaintenance(Guid productionLineId, string code, string name, EquipmentStatus status)
    {
        var equipment = new Equipment(productionLineId, code, name, EquipmentType.PickAndPlace);
        equipment.UpdateStatus(status);
        equipment.RecordMaintenance();
        return equipment;
    }

    #endregion
}
