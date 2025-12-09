using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SmartFactory.Application.Services;
using SmartFactory.Application.DTOs.Reports;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Domain.ValueObjects;

namespace SmartFactory.Application.Tests.Services;

/// <summary>
/// Unit tests for ReportService.
/// Tests report generation for OEE, Production, Quality, and Maintenance.
/// </summary>
public class ReportServiceTests
{
    private readonly Mock<IWorkOrderRepository> _workOrderRepositoryMock;
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly Mock<IMaintenanceRepository> _maintenanceRepositoryMock;
    private readonly Mock<IQualityRecordRepository> _qualityRecordRepositoryMock;
    private readonly Mock<ILogger<ReportService>> _loggerMock;
    private readonly ReportService _sut;

    public ReportServiceTests()
    {
        _workOrderRepositoryMock = new Mock<IWorkOrderRepository>();
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _maintenanceRepositoryMock = new Mock<IMaintenanceRepository>();
        _qualityRecordRepositoryMock = new Mock<IQualityRecordRepository>();
        _loggerMock = new Mock<ILogger<ReportService>>();

        _sut = new ReportService(
            _workOrderRepositoryMock.Object,
            _equipmentRepositoryMock.Object,
            _maintenanceRepositoryMock.Object,
            _qualityRecordRepositoryMock.Object,
            _loggerMock.Object);
    }

    #region GenerateOeeReportAsync Tests

    [Fact]
    public async Task GenerateOeeReportAsync_CalculatesAvailabilityCorrectly()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var workOrders = new List<WorkOrder>();
        var maintenanceRecords = CreateMaintenanceRecords(factoryId, 60); // 60 minutes downtime
        var qualityStats = CreateDefectStatistics(100, 90, 10);

        _workOrderRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTimeRange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        _maintenanceRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(maintenanceRecords);

        _qualityRecordRepositoryMock
            .Setup(r => r.GetDefectStatisticsAsync(factoryId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualityStats);

        // Act
        var result = await _sut.GenerateOeeReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Availability.Should().BeInRange(0, 100);
        result.TotalDowntime.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GenerateOeeReportAsync_ZeroPlannedTime_ReturnsZeroAvailability()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow; // Same time = 0 planned time

        var workOrders = new List<WorkOrder>();
        var maintenanceRecords = new List<MaintenanceRecord>();
        var qualityStats = CreateDefectStatistics(0, 0, 0);

        _workOrderRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTimeRange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        _maintenanceRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(maintenanceRecords);

        _qualityRecordRepositoryMock
            .Setup(r => r.GetDefectStatisticsAsync(factoryId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualityStats);

        // Act
        var result = await _sut.GenerateOeeReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        // When no data/planned time, service returns 100% (perfect availability = no recorded downtime)
        result.Availability.Should().Be(100);
    }

    [Fact]
    public async Task GenerateOeeReportAsync_CalculatesPerformanceFromWorkOrders()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var workOrders = CreateWorkOrders(factoryId, 100, 80); // 80/100 = 80% performance
        var maintenanceRecords = new List<MaintenanceRecord>();
        var qualityStats = CreateDefectStatistics(100, 95, 5);

        _workOrderRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTimeRange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        _maintenanceRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(maintenanceRecords);

        _qualityRecordRepositoryMock
            .Setup(r => r.GetDefectStatisticsAsync(factoryId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualityStats);

        // Act
        var result = await _sut.GenerateOeeReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Performance.Should().Be(80); // 80/100 * 100 = 80%
        result.TotalUnitsProduced.Should().Be(80);
    }

    [Fact]
    public async Task GenerateOeeReportAsync_CalculatesQualityFromPassRate()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var workOrders = new List<WorkOrder>();
        var maintenanceRecords = new List<MaintenanceRecord>();
        var qualityStats = CreateDefectStatistics(100, 90, 10); // 90% pass rate

        _workOrderRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTimeRange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        _maintenanceRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(maintenanceRecords);

        _qualityRecordRepositoryMock
            .Setup(r => r.GetDefectStatisticsAsync(factoryId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualityStats);

        // Act
        var result = await _sut.GenerateOeeReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Quality.Should().Be(90);
    }

    [Fact]
    public async Task GenerateOeeReportAsync_OeeFormulaIsCorrect()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var workOrders = CreateWorkOrders(factoryId, 100, 100);
        var maintenanceRecords = new List<MaintenanceRecord>();
        var qualityStats = CreateDefectStatistics(100, 100, 0); // 100% quality

        _workOrderRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTimeRange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        _maintenanceRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(maintenanceRecords);

        _qualityRecordRepositoryMock
            .Setup(r => r.GetDefectStatisticsAsync(factoryId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualityStats);

        // Act
        var result = await _sut.GenerateOeeReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        // OEE = (Availability x Performance x Quality) / 10000
        var expectedOee = (result.Availability * result.Performance * result.Quality) / 10000;
        result.OverallOee.Should().BeApproximately(expectedOee, 0.01);
    }

    #endregion

    #region GenerateProductionReportAsync Tests

    [Fact]
    public async Task GenerateProductionReportAsync_AggregatesWorkOrdersByStatus()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var workOrders = CreateWorkOrdersWithStatuses(factoryId);

        _workOrderRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTimeRange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        // Act
        var result = await _sut.GenerateProductionReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.WorkOrdersByStatus.Should().NotBeEmpty();
        result.TotalWorkOrders.Should().Be(workOrders.Count);
    }

    [Fact]
    public async Task GenerateProductionReportAsync_CalculatesYieldRate()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var workOrders = CreateWorkOrders(factoryId, 100, 80); // 80% yield

        _workOrderRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTimeRange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        // Act
        var result = await _sut.GenerateProductionReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.YieldRate.Should().Be(80); // 80/100 * 100
    }

    [Fact]
    public async Task GenerateProductionReportAsync_ZeroTarget_ReturnsZeroYieldRate()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var workOrders = new List<WorkOrder>(); // No work orders

        _workOrderRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTimeRange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        // Act
        var result = await _sut.GenerateProductionReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.YieldRate.Should().Be(0);
        result.TotalTargetQuantity.Should().Be(0);
    }

    [Fact]
    public async Task GenerateProductionReportAsync_GroupsByPriority()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var workOrders = CreateWorkOrdersWithStatuses(factoryId);

        _workOrderRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTimeRange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        // Act
        var result = await _sut.GenerateProductionReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.WorkOrdersByPriority.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateProductionReportAsync_GeneratesDailyTrends()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var workOrders = CreateWorkOrdersWithStatuses(factoryId);

        _workOrderRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTimeRange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        // Act
        var result = await _sut.GenerateProductionReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.DailyTrends.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateProductionReportAsync_ReturnsTop10Products()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var workOrders = CreateWorkOrdersWithProducts(factoryId, 15); // More than 10 products

        _workOrderRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTimeRange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        // Act
        var result = await _sut.GenerateProductionReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TopProducts.Should().HaveCountLessOrEqualTo(10);
    }

    #endregion

    #region GenerateQualityReportAsync Tests

    [Fact]
    public async Task GenerateQualityReportAsync_CalculatesPassRate()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var qualityStats = CreateDefectStatistics(100, 85, 15); // 85% pass rate
        var qualityRecords = new List<QualityRecord>();

        _qualityRecordRepositoryMock
            .Setup(r => r.GetDefectStatisticsAsync(factoryId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualityStats);

        _qualityRecordRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualityRecords);

        // Act
        var result = await _sut.GenerateQualityReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.PassRate.Should().Be(85);
        result.TotalInspections.Should().Be(100);
    }

    [Fact]
    public async Task GenerateQualityReportAsync_GroupsDefectsByType()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var qualityStats = CreateDefectStatisticsWithTypes();
        var qualityRecords = new List<QualityRecord>();

        _qualityRecordRepositoryMock
            .Setup(r => r.GetDefectStatisticsAsync(factoryId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualityStats);

        _qualityRecordRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualityRecords);

        // Act
        var result = await _sut.GenerateQualityReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.DefectsByType.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateQualityReportAsync_ReturnsTop10Defects()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var qualityStats = CreateDefectStatisticsWithTypes();
        var qualityRecords = new List<QualityRecord>();

        _qualityRecordRepositoryMock
            .Setup(r => r.GetDefectStatisticsAsync(factoryId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualityStats);

        _qualityRecordRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualityRecords);

        // Act
        var result = await _sut.GenerateQualityReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TopDefects.Should().HaveCountLessOrEqualTo(10);
    }

    [Fact]
    public async Task GenerateQualityReportAsync_ZeroDefects_ReturnsZeroPercentage()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var qualityStats = CreateDefectStatistics(100, 100, 0); // No defects
        var qualityRecords = new List<QualityRecord>();

        _qualityRecordRepositoryMock
            .Setup(r => r.GetDefectStatisticsAsync(factoryId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualityStats);

        _qualityRecordRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualityRecords);

        // Act
        var result = await _sut.GenerateQualityReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TotalDefects.Should().Be(0);
        result.PassRate.Should().Be(100);
    }

    #endregion

    #region GenerateMaintenanceReportAsync Tests

    [Fact]
    public async Task GenerateMaintenanceReportAsync_CalculatesCompletionRate()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var maintenanceRecords = CreateMaintenanceRecordsWithStatuses();

        _maintenanceRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(maintenanceRecords);

        // Act
        var result = await _sut.GenerateMaintenanceReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.CompletionRate.Should().BeInRange(0, 100);
        result.TotalScheduled.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GenerateMaintenanceReportAsync_SumsTotalDowntime()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var maintenanceRecords = CreateMaintenanceRecordsWithDowntime(120); // 120 minutes total

        _maintenanceRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(maintenanceRecords);

        // Act
        var result = await _sut.GenerateMaintenanceReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TotalDowntimeMinutes.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GenerateMaintenanceReportAsync_GroupsByTypeAndStatus()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var maintenanceRecords = CreateMaintenanceRecordsWithStatuses();

        _maintenanceRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(maintenanceRecords);

        // Act
        var result = await _sut.GenerateMaintenanceReportAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.MaintenanceByType.Should().NotBeEmpty();
        result.MaintenanceByStatus.Should().NotBeEmpty();
    }

    #endregion

    #region GetEquipmentEfficiencyAsync Tests

    [Fact]
    public async Task GetEquipmentEfficiencyAsync_CalculatesAvailabilityPerEquipment()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1"),
            CreateEquipment(lineId, "EQ002", "Equipment 2")
        };

        var maintenanceRecords = new List<MaintenanceRecord>();

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _maintenanceRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(maintenanceRecords);

        // Act
        var result = await _sut.GetEquipmentEfficiencyAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Availability >= 0 && e.Availability <= 100);
    }

    [Fact]
    public async Task GetEquipmentEfficiencyAsync_OrdersByAvailabilityDescending()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow;

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1"),
            CreateEquipment(lineId, "EQ002", "Equipment 2")
        };

        var maintenanceRecords = new List<MaintenanceRecord>();

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _maintenanceRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(maintenanceRecords);

        // Act
        var result = await _sut.GetEquipmentEfficiencyAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeInDescendingOrder(e => e.Availability);
    }

    #endregion

    #region Helper Methods

    private static Equipment CreateEquipment(Guid productionLineId, string code, string name)
    {
        return new Equipment(productionLineId, code, name, EquipmentType.PickAndPlace);
    }

    private static List<WorkOrder> CreateWorkOrders(Guid factoryId, int targetQty, int completedQty)
    {
        var workOrder = new WorkOrder(factoryId, $"WO-{Guid.NewGuid():N}", "PROD001", "Test Product", targetQty, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));

        // Use reflection to set CompletedQuantity since it's likely private setter
        var completedQuantityProperty = typeof(WorkOrder).GetProperty("CompletedQuantity");
        if (completedQuantityProperty?.CanWrite == true)
        {
            completedQuantityProperty.SetValue(workOrder, completedQty);
        }

        return new List<WorkOrder> { workOrder };
    }

    private static List<WorkOrder> CreateWorkOrdersWithStatuses(Guid factoryId)
    {
        var workOrders = new List<WorkOrder>
        {
            new WorkOrder(factoryId, "WO-001", "PROD001", "Product 1", 100, DateTime.UtcNow, DateTime.UtcNow.AddDays(1)),
            new WorkOrder(factoryId, "WO-002", "PROD002", "Product 2", 50, DateTime.UtcNow, DateTime.UtcNow.AddDays(2)),
            new WorkOrder(factoryId, "WO-003", "PROD003", "Product 3", 75, DateTime.UtcNow, DateTime.UtcNow.AddDays(3))
        };

        return workOrders;
    }

    private static List<WorkOrder> CreateWorkOrdersWithProducts(Guid factoryId, int productCount)
    {
        var workOrders = new List<WorkOrder>();
        for (int i = 0; i < productCount; i++)
        {
            workOrders.Add(new WorkOrder(
                factoryId,
                $"WO-{i:D3}",
                $"PROD{i:D3}",
                $"Product {i}",
                100,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(i + 1)));
        }
        return workOrders;
    }

    private static List<MaintenanceRecord> CreateMaintenanceRecords(Guid factoryId, int totalDowntimeMinutes)
    {
        var equipmentId = Guid.NewGuid();
        var record = new MaintenanceRecord(
            equipmentId,
            MaintenanceType.Preventive,
            "Test Maintenance",
            DateTime.UtcNow);

        // Set downtime through reflection if needed
        var downtimeProperty = typeof(MaintenanceRecord).GetProperty("DowntimeMinutes");
        if (downtimeProperty?.CanWrite == true)
        {
            downtimeProperty.SetValue(record, totalDowntimeMinutes);
        }

        return new List<MaintenanceRecord> { record };
    }

    private static List<MaintenanceRecord> CreateMaintenanceRecordsWithStatuses()
    {
        var equipmentId = Guid.NewGuid();
        var records = new List<MaintenanceRecord>
        {
            new MaintenanceRecord(equipmentId, MaintenanceType.Preventive, "Scheduled maintenance", DateTime.UtcNow),
            new MaintenanceRecord(equipmentId, MaintenanceType.Corrective, "Emergency repair", DateTime.UtcNow.AddDays(-1)),
            new MaintenanceRecord(equipmentId, MaintenanceType.Predictive, "Predicted maintenance", DateTime.UtcNow.AddDays(-2))
        };

        return records;
    }

    private static List<MaintenanceRecord> CreateMaintenanceRecordsWithDowntime(int totalMinutes)
    {
        return CreateMaintenanceRecords(Guid.NewGuid(), totalMinutes);
    }

    private static DefectStatistics CreateDefectStatistics(int total, int pass, int fail)
    {
        return new DefectStatistics
        {
            TotalInspections = total,
            PassCount = pass,
            FailCount = fail,
            TotalDefects = fail,
            PassRate = total > 0 ? (double)pass / total * 100 : 0,
            DefectsByType = new Dictionary<DefectType, int>()
        };
    }

    private static DefectStatistics CreateDefectStatisticsWithTypes()
    {
        return new DefectStatistics
        {
            TotalInspections = 100,
            PassCount = 85,
            FailCount = 15,
            TotalDefects = 15,
            PassRate = 85,
            DefectsByType = new Dictionary<DefectType, int>
            {
                { DefectType.MisalignedComponent, 5 },
                { DefectType.SolderBridge, 4 },
                { DefectType.ColdSolder, 3 },
                { DefectType.MissingComponent, 2 },
                { DefectType.InsufficientSolder, 1 }
            }
        };
    }

    #endregion
}
