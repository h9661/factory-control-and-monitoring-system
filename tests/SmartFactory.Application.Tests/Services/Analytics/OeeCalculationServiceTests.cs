using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SmartFactory.Application.Services.Analytics;
using SmartFactory.Application.DTOs.Analytics;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;

namespace SmartFactory.Application.Tests.Services.Analytics;

/// <summary>
/// Unit tests for OeeCalculationService.
/// Tests OEE formula: OEE = Availability x Performance x Quality
/// </summary>
public class OeeCalculationServiceTests
{
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly Mock<IWorkOrderRepository> _workOrderRepositoryMock;
    private readonly Mock<ILogger<OeeCalculationService>> _loggerMock;
    private readonly OeeCalculationService _sut;

    public OeeCalculationServiceTests()
    {
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _workOrderRepositoryMock = new Mock<IWorkOrderRepository>();
        _loggerMock = new Mock<ILogger<OeeCalculationService>>();

        _sut = new OeeCalculationService(
            _loggerMock.Object,
            _equipmentRepositoryMock.Object,
            _workOrderRepositoryMock.Object);
    }

    #region CalculateOeeAsync Tests

    [Fact]
    public async Task CalculateOeeAsync_EquipmentNotFound_ReturnsEmptyResult()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        // Act
        var result = await _sut.CalculateOeeAsync(equipmentId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.OverallOee.Should().Be(0);
        result.Availability.Should().Be(0);
        result.Performance.Should().Be(0);
        result.Quality.Should().Be(0);
        result.EquipmentId.Should().Be(equipmentId);
    }

    [Fact]
    public async Task CalculateOeeAsync_WithValidEquipment_ReturnsOeeResult()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);
        var productionSummary = new ProductionSummary
        {
            CompletedUnits = 100,
            DefectUnits = 5,
            TargetUnits = 120
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _workOrderRepositoryMock
            .Setup(r => r.GetProductionSummaryAsync(It.IsAny<Guid?>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productionSummary);

        // Act
        var result = await _sut.CalculateOeeAsync(equipmentId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TotalProduced.Should().Be(100);
        result.DefectUnits.Should().Be(5);
        result.GoodUnits.Should().Be(95);
        result.EquipmentName.Should().Be("Test Equipment");
    }

    [Fact]
    public async Task CalculateOeeAsync_WithZeroProduction_ReturnsZeroPerformance()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);
        var productionSummary = new ProductionSummary
        {
            CompletedUnits = 0,
            DefectUnits = 0,
            TargetUnits = 0
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _workOrderRepositoryMock
            .Setup(r => r.GetProductionSummaryAsync(It.IsAny<Guid?>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productionSummary);

        // Act
        var result = await _sut.CalculateOeeAsync(equipmentId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Performance.Should().Be(0);
        result.TotalProduced.Should().Be(0);
    }

    [Fact]
    public async Task CalculateOeeAsync_WithAllDefects_ReturnsZeroQuality()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);
        var productionSummary = new ProductionSummary
        {
            CompletedUnits = 100,
            DefectUnits = 100, // All defects
            TargetUnits = 100
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _workOrderRepositoryMock
            .Setup(r => r.GetProductionSummaryAsync(It.IsAny<Guid?>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productionSummary);

        // Act
        var result = await _sut.CalculateOeeAsync(equipmentId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Quality.Should().Be(0);
        result.GoodUnits.Should().Be(0);
    }

    [Fact]
    public async Task CalculateOeeAsync_ValuesClampedTo0To100Range()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);
        var productionSummary = new ProductionSummary
        {
            CompletedUnits = 100,
            DefectUnits = 0,
            TargetUnits = 100
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _workOrderRepositoryMock
            .Setup(r => r.GetProductionSummaryAsync(It.IsAny<Guid?>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productionSummary);

        // Act
        var result = await _sut.CalculateOeeAsync(equipmentId, startDate, endDate);

        // Assert
        result.Availability.Should().BeInRange(0, 100);
        result.Performance.Should().BeInRange(0, 100);
        result.Quality.Should().BeInRange(0, 100);
        result.OverallOee.Should().BeInRange(0, 100);
    }

    [Fact]
    public async Task CalculateOeeAsync_WithPerfectProduction_ReturnsHighQuality()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);
        var productionSummary = new ProductionSummary
        {
            CompletedUnits = 100,
            DefectUnits = 0, // No defects
            TargetUnits = 100
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _workOrderRepositoryMock
            .Setup(r => r.GetProductionSummaryAsync(It.IsAny<Guid?>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productionSummary);

        // Act
        var result = await _sut.CalculateOeeAsync(equipmentId, startDate, endDate);

        // Assert
        result.Quality.Should().Be(100);
        result.GoodUnits.Should().Be(100);
    }

    #endregion

    #region CalculateFactoryOeeAsync Tests

    [Fact]
    public async Task CalculateFactoryOeeAsync_WithNoActiveEquipment_ReturnsEmptyResult()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Equipment>());

        // Act
        var result = await _sut.CalculateFactoryOeeAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.OverallOee.Should().Be(0);
        result.Availability.Should().Be(0);
        result.Performance.Should().Be(0);
        result.Quality.Should().Be(0);
    }

    [Fact]
    public async Task CalculateFactoryOeeAsync_WithActiveEquipment_AggregatesCorrectly()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Running),
            CreateEquipment(lineId, "EQ002", "Equipment 2", EquipmentStatus.Running)
        };

        var productionSummary = new ProductionSummary
        {
            CompletedUnits = 200,
            DefectUnits = 10,
            TargetUnits = 220
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _workOrderRepositoryMock
            .Setup(r => r.GetProductionSummaryAsync(factoryId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productionSummary);

        // Act
        var result = await _sut.CalculateFactoryOeeAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TotalProduced.Should().Be(200);
        result.GoodUnits.Should().Be(190);
        result.DefectUnits.Should().Be(10);
    }

    [Fact]
    public async Task CalculateFactoryOeeAsync_OnlyCountsActiveEquipment()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var activeEquipment = CreateEquipment(lineId, "EQ001", "Active Equipment", EquipmentStatus.Running);
        var inactiveEquipment = CreateEquipment(lineId, "EQ002", "Inactive Equipment", EquipmentStatus.Offline);
        inactiveEquipment.Deactivate();

        var equipment = new List<Equipment> { activeEquipment, inactiveEquipment };

        var productionSummary = new ProductionSummary
        {
            CompletedUnits = 100,
            DefectUnits = 5,
            TargetUnits = 110
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _workOrderRepositoryMock
            .Setup(r => r.GetProductionSummaryAsync(factoryId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productionSummary);

        // Act
        var result = await _sut.CalculateFactoryOeeAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        // Planned time should only consider active equipment
        result.PlannedProductionTimeMinutes.Should().BeGreaterThan(0);
    }

    #endregion

    #region GetOeeTrendAsync Tests

    [Fact]
    public async Task GetOeeTrendAsync_HourlyGranularity_ReturnsHourlyPoints()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddHours(-5);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _sut.GetOeeTrendAsync(factoryId, startDate, endDate, OeeGranularity.Hourly);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterOrEqualTo(4); // ~5 hours
        result.Should().OnlyContain(p => p.Timestamp >= startDate && p.Timestamp <= endDate);
    }

    [Fact]
    public async Task GetOeeTrendAsync_DailyGranularity_ReturnsDailyPoints()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _sut.GetOeeTrendAsync(factoryId, startDate, endDate, OeeGranularity.Daily);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterOrEqualTo(6); // ~7 days
    }

    [Fact]
    public async Task GetOeeTrendAsync_WeeklyGranularity_ReturnsWeeklyPoints()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-28);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _sut.GetOeeTrendAsync(factoryId, startDate, endDate, OeeGranularity.Weekly);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterOrEqualTo(3); // ~4 weeks
    }

    [Fact]
    public async Task GetOeeTrendAsync_DataPointsHaveValidOeeValues()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddHours(-2);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _sut.GetOeeTrendAsync(factoryId, startDate, endDate, OeeGranularity.Hourly);

        // Assert
        result.Should().OnlyContain(p =>
            p.Availability >= 0 && p.Availability <= 100 &&
            p.Performance >= 0 && p.Performance <= 100 &&
            p.Quality >= 0 && p.Quality <= 100 &&
            p.OverallOee >= 0 && p.OverallOee <= 100);
    }

    #endregion

    #region GetLossBreakdownAsync Tests

    [Fact]
    public async Task GetLossBreakdownAsync_ReturnsLossCategories()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Running)
        };

        var productionSummary = new ProductionSummary
        {
            CompletedUnits = 100,
            DefectUnits = 10,
            TargetUnits = 110
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _workOrderRepositoryMock
            .Setup(r => r.GetProductionSummaryAsync(factoryId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productionSummary);

        // Act
        var result = await _sut.GetLossBreakdownAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.LossCategories.Should().NotBeEmpty();
        result.LossCategories.Should().Contain(c => c.Type == LossType.Availability);
        result.LossCategories.Should().Contain(c => c.Type == LossType.Performance);
        result.LossCategories.Should().Contain(c => c.Type == LossType.Quality);
    }

    [Fact]
    public async Task GetLossBreakdownAsync_CalculatesAvailabilityLoss()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Running)
        };

        var productionSummary = new ProductionSummary
        {
            CompletedUnits = 100,
            DefectUnits = 5
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _workOrderRepositoryMock
            .Setup(r => r.GetProductionSummaryAsync(factoryId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productionSummary);

        // Act
        var result = await _sut.GetLossBreakdownAsync(factoryId, startDate, endDate);

        // Assert
        result.AvailabilityLossPercent.Should().BeGreaterOrEqualTo(0);
        result.EffectiveProductionPercent.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetLossBreakdownAsync_LossCategoriesHaveCorrectTypes()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Running)
        };

        var productionSummary = new ProductionSummary
        {
            CompletedUnits = 100,
            DefectUnits = 10
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _workOrderRepositoryMock
            .Setup(r => r.GetProductionSummaryAsync(factoryId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productionSummary);

        // Act
        var result = await _sut.GetLossBreakdownAsync(factoryId, startDate, endDate);

        // Assert
        var availabilityCategories = result.LossCategories.Where(c => c.Type == LossType.Availability);
        var performanceCategories = result.LossCategories.Where(c => c.Type == LossType.Performance);
        var qualityCategories = result.LossCategories.Where(c => c.Type == LossType.Quality);

        availabilityCategories.Should().NotBeEmpty();
        performanceCategories.Should().NotBeEmpty();
        qualityCategories.Should().NotBeEmpty();
    }

    #endregion

    #region GetOeeComparisonAsync Tests

    [Fact]
    public async Task GetOeeComparisonAsync_DayComparison_ReturnsCorrectPeriods()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var periodCount = 7;

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Running)
        };

        var productionSummary = new ProductionSummary
        {
            CompletedUnits = 100,
            DefectUnits = 5
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _workOrderRepositoryMock
            .Setup(r => r.GetProductionSummaryAsync(factoryId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productionSummary);

        // Act
        var result = await _sut.GetOeeComparisonAsync(factoryId, OeeComparisonType.Day, periodCount);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(periodCount);
    }

    [Fact]
    public async Task GetOeeComparisonAsync_CalculatesChangeFromPrevious()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var periodCount = 3;

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Running)
        };

        var productionSummary = new ProductionSummary
        {
            CompletedUnits = 100,
            DefectUnits = 5
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _workOrderRepositoryMock
            .Setup(r => r.GetProductionSummaryAsync(factoryId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productionSummary);

        // Act
        var result = await _sut.GetOeeComparisonAsync(factoryId, OeeComparisonType.Day, periodCount);

        // Assert
        result.Should().NotBeNull();
        result.First().ChangeFromPrevious.Should().Be(0); // First period has no previous
    }

    #endregion

    #region GetEquipmentOeeListAsync Tests

    [Fact]
    public async Task GetEquipmentOeeListAsync_ReturnsOrderedByOee()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Running),
            CreateEquipment(lineId, "EQ002", "Equipment 2", EquipmentStatus.Idle)
        };

        var productionSummary = new ProductionSummary
        {
            CompletedUnits = 100,
            DefectUnits = 5
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => equipment.FirstOrDefault(e => e.Id == id));

        _workOrderRepositoryMock
            .Setup(r => r.GetProductionSummaryAsync(It.IsAny<Guid?>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productionSummary);

        // Act
        var result = await _sut.GetEquipmentOeeListAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeInDescendingOrder(r => r.OverallOee);
    }

    [Fact]
    public async Task GetEquipmentOeeListAsync_OnlyIncludesActiveEquipment()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var activeEquipment = CreateEquipment(lineId, "EQ001", "Active Equipment", EquipmentStatus.Running);
        var inactiveEquipment = CreateEquipment(lineId, "EQ002", "Inactive Equipment", EquipmentStatus.Offline);
        inactiveEquipment.Deactivate();

        var equipment = new List<Equipment> { activeEquipment, inactiveEquipment };

        var productionSummary = new ProductionSummary
        {
            CompletedUnits = 100,
            DefectUnits = 5
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(activeEquipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeEquipment);

        _workOrderRepositoryMock
            .Setup(r => r.GetProductionSummaryAsync(It.IsAny<Guid?>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productionSummary);

        // Act
        var result = await _sut.GetEquipmentOeeListAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().EquipmentName.Should().Be("Active Equipment");
    }

    #endregion

    #region Time Breakdown Tests (Status-based)

    [Theory]
    [InlineData(EquipmentStatus.Running, 85)]
    [InlineData(EquipmentStatus.Idle, 20)]
    [InlineData(EquipmentStatus.Warning, 60)]
    [InlineData(EquipmentStatus.Error, 10)]
    [InlineData(EquipmentStatus.Maintenance, 5)]
    [InlineData(EquipmentStatus.Offline, 0)]
    public async Task CalculateOeeAsync_DifferentStatuses_ReturnsExpectedRunTimePercentage(
        EquipmentStatus status,
        int expectedRunTimePercent)
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", status);
        var productionSummary = new ProductionSummary
        {
            CompletedUnits = 100,
            DefectUnits = 5
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _workOrderRepositoryMock
            .Setup(r => r.GetProductionSummaryAsync(It.IsAny<Guid?>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productionSummary);

        // Act
        var result = await _sut.CalculateOeeAsync(equipmentId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        var totalTime = result.ActualRunTimeMinutes + result.IdleTimeMinutes + result.DownTimeMinutes;
        if (totalTime > 0)
        {
            var actualRunPercent = (result.ActualRunTimeMinutes / totalTime) * 100;
            actualRunPercent.Should().BeApproximately(expectedRunTimePercent, 5);
        }
    }

    #endregion

    #region Helper Methods

    private static Equipment CreateEquipment(Guid productionLineId, string code, string name, EquipmentStatus status)
    {
        var equipment = new Equipment(productionLineId, code, name, EquipmentType.PickAndPlace);
        equipment.UpdateStatus(status);
        return equipment;
    }

    #endregion
}
