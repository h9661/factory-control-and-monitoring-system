using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.DTOs.Quality;
using SmartFactory.Application.Exceptions;
using SmartFactory.Application.Services;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using Xunit;

namespace SmartFactory.Application.Tests.Services;

/// <summary>
/// Unit tests for QualityService covering inspection recording, defect summaries, and quality trends.
/// </summary>
public class QualityServiceTests
{
    private readonly Mock<IQualityRecordRepository> _qualityRecordRepositoryMock;
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly Mock<IWorkOrderRepository> _workOrderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<QualityRecordCreateDto>> _createValidatorMock;
    private readonly Mock<ILogger<QualityService>> _loggerMock;
    private readonly QualityService _sut;

    public QualityServiceTests()
    {
        _qualityRecordRepositoryMock = new Mock<IQualityRecordRepository>();
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _workOrderRepositoryMock = new Mock<IWorkOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _createValidatorMock = new Mock<IValidator<QualityRecordCreateDto>>();
        _loggerMock = new Mock<ILogger<QualityService>>();

        _sut = new QualityService(
            _qualityRecordRepositoryMock.Object,
            _equipmentRepositoryMock.Object,
            _workOrderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _createValidatorMock.Object,
            _loggerMock.Object);
    }

    #region GetQualityRecordsAsync Tests

    [Fact]
    public async Task GetQualityRecordsAsync_WithEquipmentIdFilter_ReturnsFilteredRecords()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var filter = new QualityFilterDto { EquipmentId = equipmentId };
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };
        var records = CreateTestQualityRecords(5);
        var dtos = records.Select(r => CreateQualityRecordDto(r)).ToList();

        _qualityRecordRepositoryMock.Setup(x => x.GetByEquipmentAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);
        _mapperMock.Setup(x => x.Map<IEnumerable<QualityRecordDto>>(It.IsAny<IEnumerable<QualityRecord>>()))
            .Returns(dtos);

        // Act
        var result = await _sut.GetQualityRecordsAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(5);
        _qualityRecordRepositoryMock.Verify(x => x.GetByEquipmentAsync(equipmentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetQualityRecordsAsync_WithWorkOrderIdFilter_ReturnsFilteredRecords()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();
        var filter = new QualityFilterDto { WorkOrderId = workOrderId };
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };
        var records = CreateTestQualityRecords(3);
        var dtos = records.Select(r => CreateQualityRecordDto(r)).ToList();

        _qualityRecordRepositoryMock.Setup(x => x.GetByWorkOrderAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);
        _mapperMock.Setup(x => x.Map<IEnumerable<QualityRecordDto>>(It.IsAny<IEnumerable<QualityRecord>>()))
            .Returns(dtos);

        // Act
        var result = await _sut.GetQualityRecordsAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        _qualityRecordRepositoryMock.Verify(x => x.GetByWorkOrderAsync(workOrderId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetQualityRecordsAsync_WithDateRangeFilter_ReturnsFilteredRecords()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var filter = new QualityFilterDto { DateFrom = startDate, DateTo = endDate };
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };
        var records = CreateTestQualityRecords(4);
        var dtos = records.Select(r => CreateQualityRecordDto(r)).ToList();

        _qualityRecordRepositoryMock.Setup(x => x.GetByDateRangeAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);
        _mapperMock.Setup(x => x.Map<IEnumerable<QualityRecordDto>>(It.IsAny<IEnumerable<QualityRecord>>()))
            .Returns(dtos);

        // Act
        var result = await _sut.GetQualityRecordsAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(4);
        _qualityRecordRepositoryMock.Verify(x => x.GetByDateRangeAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetQualityRecordsAsync_WithNoFilter_ReturnsAllRecords()
    {
        // Arrange
        var filter = new QualityFilterDto();
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };
        var records = CreateTestQualityRecords(8);
        var dtos = records.Select(r => CreateQualityRecordDto(r)).ToList();

        _qualityRecordRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);
        _mapperMock.Setup(x => x.Map<IEnumerable<QualityRecordDto>>(It.IsAny<IEnumerable<QualityRecord>>()))
            .Returns(dtos);

        // Act
        var result = await _sut.GetQualityRecordsAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(8);
        _qualityRecordRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetQualityRecordByIdAsync Tests

    [Fact]
    public async Task GetQualityRecordByIdAsync_RecordExists_ReturnsDetailDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var record = CreateTestQualityRecord(id);
        var dto = CreateQualityRecordDetailDto(record);

        _qualityRecordRepositoryMock.Setup(x => x.GetWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _mapperMock.Setup(x => x.Map<QualityRecordDetailDto>(record))
            .Returns(dto);

        // Act
        var result = await _sut.GetQualityRecordByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetQualityRecordByIdAsync_RecordNotFound_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _qualityRecordRepositoryMock.Setup(x => x.GetWithDetailsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((QualityRecord?)null);

        // Act
        var result = await _sut.GetQualityRecordByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region RecordInspectionAsync Tests

    [Fact]
    public async Task RecordInspectionAsync_WithValidDto_CreatesAndReturnsRecord()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var dto = new QualityRecordCreateDto
        {
            EquipmentId = equipmentId,
            InspectionType = InspectionType.Visual,
            Result = InspectionResult.Pass,
            InspectorId = "INS001",
            InspectorName = "Jane Smith",
            SampleSize = 100,
            Notes = "All samples passed"
        };
        var equipment = CreateTestEquipment(equipmentId);
        var resultDto = new QualityRecordDto { Id = Guid.NewGuid(), Result = InspectionResult.Pass };

        _createValidatorMock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _equipmentRepositoryMock.Setup(x => x.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);
        _mapperMock.Setup(x => x.Map<QualityRecordDto>(It.IsAny<QualityRecord>()))
            .Returns(resultDto);

        // Act
        var result = await _sut.RecordInspectionAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be(InspectionResult.Pass);
        _qualityRecordRepositoryMock.Verify(x => x.AddAsync(It.IsAny<QualityRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordInspectionAsync_WithValidationFailure_ThrowsValidationException()
    {
        // Arrange
        var dto = new QualityRecordCreateDto
        {
            EquipmentId = Guid.Empty,
            InspectionType = InspectionType.Visual,
            Result = InspectionResult.Pass
        };
        var validationResult = new ValidationResult(new[] { new ValidationFailure("EquipmentId", "EquipmentId is required") });
        _createValidatorMock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var act = () => _sut.RecordInspectionAsync(dto);

        // Assert
        await act.Should().ThrowAsync<Exceptions.ValidationException>();
    }

    [Fact]
    public async Task RecordInspectionAsync_EquipmentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var dto = new QualityRecordCreateDto
        {
            EquipmentId = equipmentId,
            InspectionType = InspectionType.Visual,
            Result = InspectionResult.Pass
        };

        _createValidatorMock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _equipmentRepositoryMock.Setup(x => x.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        // Act
        var act = () => _sut.RecordInspectionAsync(dto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RecordInspectionAsync_WithWorkOrderNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var workOrderId = Guid.NewGuid();
        var equipment = CreateTestEquipment(equipmentId);
        var dto = new QualityRecordCreateDto
        {
            EquipmentId = equipmentId,
            WorkOrderId = workOrderId,
            InspectionType = InspectionType.Visual,
            Result = InspectionResult.Pass
        };

        _createValidatorMock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _equipmentRepositoryMock.Setup(x => x.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);
        _workOrderRepositoryMock.Setup(x => x.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder?)null);

        // Act
        var act = () => _sut.RecordInspectionAsync(dto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RecordInspectionAsync_WithDefect_RecordsDefectInformation()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var dto = new QualityRecordCreateDto
        {
            EquipmentId = equipmentId,
            InspectionType = InspectionType.Visual,
            Result = InspectionResult.Fail,
            DefectType = DefectType.SolderBridge,
            DefectDescription = "Surface scratches found",
            DefectCount = 5,
            SampleSize = 100
        };
        var equipment = CreateTestEquipment(equipmentId);
        var resultDto = new QualityRecordDto { Id = Guid.NewGuid(), Result = InspectionResult.Fail };

        _createValidatorMock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _equipmentRepositoryMock.Setup(x => x.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);
        _mapperMock.Setup(x => x.Map<QualityRecordDto>(It.IsAny<QualityRecord>()))
            .Returns(resultDto);

        // Act
        var result = await _sut.RecordInspectionAsync(dto);

        // Assert
        result.Should().NotBeNull();
        _qualityRecordRepositoryMock.Verify(x => x.AddAsync(It.IsAny<QualityRecord>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetDefectSummaryAsync Tests

    [Fact]
    public async Task GetDefectSummaryAsync_WithRecords_ReturnsCorrectSummary()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var statistics = new DefectStatistics
        {
            TotalInspections = 100,
            PassCount = 85,
            FailCount = 15,
            TotalDefects = 20,
            PassRate = 85.0,
            DefectsByType = new Dictionary<DefectType, int>
            {
                { DefectType.SolderBridge, 8 },
                { DefectType.MisalignedComponent, 7 },
                { DefectType.MissingComponent, 5 }
            }
        };

        _qualityRecordRepositoryMock.Setup(x => x.GetDefectStatisticsAsync(factoryId, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(statistics);

        // Act
        var result = await _sut.GetDefectSummaryAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TotalInspections.Should().Be(100);
        result.PassCount.Should().Be(85);
        result.FailCount.Should().Be(15);
        result.PassRate.Should().Be(85.0);
        result.TotalDefects.Should().Be(20);
        result.OverallDefectRate.Should().Be(20.0);
    }

    [Fact]
    public async Task GetDefectSummaryAsync_WithNoInspections_ReturnsZeroDefectRate()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var statistics = new DefectStatistics
        {
            TotalInspections = 0,
            PassCount = 0,
            FailCount = 0,
            TotalDefects = 0,
            PassRate = 0,
            DefectsByType = new Dictionary<DefectType, int>()
        };

        _qualityRecordRepositoryMock.Setup(x => x.GetDefectStatisticsAsync(null, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(statistics);

        // Act
        var result = await _sut.GetDefectSummaryAsync(null, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TotalInspections.Should().Be(0);
        result.OverallDefectRate.Should().Be(0);
    }

    #endregion

    #region GetQualityTrendsAsync Tests

    [Fact]
    public async Task GetQualityTrendsAsync_WithRecords_ReturnsDailyTrends()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var records = CreateTestQualityRecordsForTrends(21);

        _qualityRecordRepositoryMock.Setup(x => x.GetByDateRangeAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        // Act
        var result = await _sut.GetQualityTrendsAsync(null, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetQualityTrendsAsync_WithFactoryFilter_ReturnsFilteredTrends()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var records = CreateTestQualityRecordsForTrends(14);

        _qualityRecordRepositoryMock.Setup(x => x.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        // Act
        var result = await _sut.GetQualityTrendsAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        _qualityRecordRepositoryMock.Verify(x => x.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region CalculateYieldRateAsync Tests

    [Fact]
    public async Task CalculateYieldRateAsync_WithRecords_ReturnsCorrectRate()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var records = CreateTestQualityRecordsWithResults(passCount: 85, failCount: 15);

        _qualityRecordRepositoryMock.Setup(x => x.GetByDateRangeAsync(
            date, date.AddDays(1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        // Act
        var result = await _sut.CalculateYieldRateAsync(null, date);

        // Assert
        result.Should().Be(85.0);
    }

    [Fact]
    public async Task CalculateYieldRateAsync_WithNoRecords_ReturnsZero()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;

        _qualityRecordRepositoryMock.Setup(x => x.GetByDateRangeAsync(
            date, date.AddDays(1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<QualityRecord>());

        // Act
        var result = await _sut.CalculateYieldRateAsync(null, date);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task CalculateYieldRateAsync_WithFactoryFilter_ReturnsFilteredRate()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;
        var records = CreateTestQualityRecordsWithResults(passCount: 90, failCount: 10);

        _qualityRecordRepositoryMock.Setup(x => x.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        // Act
        var result = await _sut.CalculateYieldRateAsync(factoryId, date);

        // Assert
        result.Should().Be(90.0);
        _qualityRecordRepositoryMock.Verify(x => x.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static List<QualityRecord> CreateTestQualityRecords(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => CreateTestQualityRecord(Guid.NewGuid()))
            .ToList();
    }

    private static QualityRecord CreateTestQualityRecord(Guid id)
    {
        var record = new QualityRecord(
            Guid.NewGuid(),
            InspectionType.Visual,
            InspectionResult.Pass,
            DateTime.UtcNow);

        var idProperty = typeof(QualityRecord).GetProperty("Id");
        idProperty?.SetValue(record, id);

        return record;
    }

    private static List<QualityRecord> CreateTestQualityRecordsForTrends(int count)
    {
        var records = new List<QualityRecord>();
        var baseDate = DateTime.UtcNow.AddDays(-7);

        for (int i = 0; i < count; i++)
        {
            var record = new QualityRecord(
                Guid.NewGuid(),
                InspectionType.Visual,
                i % 5 == 0 ? InspectionResult.Fail : InspectionResult.Pass,
                baseDate.AddDays(i % 7));
            records.Add(record);
        }

        return records;
    }

    private static List<QualityRecord> CreateTestQualityRecordsWithResults(int passCount, int failCount)
    {
        var records = new List<QualityRecord>();
        var now = DateTime.UtcNow;

        for (int i = 0; i < passCount; i++)
        {
            records.Add(new QualityRecord(Guid.NewGuid(), InspectionType.Visual, InspectionResult.Pass, now));
        }

        for (int i = 0; i < failCount; i++)
        {
            records.Add(new QualityRecord(Guid.NewGuid(), InspectionType.Visual, InspectionResult.Fail, now));
        }

        return records;
    }

    private static Equipment CreateTestEquipment(Guid id)
    {
        var equipment = new Equipment(
            Guid.NewGuid(),
            "EQ001",
            "Test Equipment",
            EquipmentType.SMTMachine);

        var idProperty = typeof(Equipment).GetProperty("Id");
        idProperty?.SetValue(equipment, id);

        return equipment;
    }

    private static QualityRecordDto CreateQualityRecordDto(QualityRecord record)
    {
        return new QualityRecordDto
        {
            Id = record.Id,
            EquipmentId = record.EquipmentId,
            InspectionType = record.InspectionType,
            Result = record.Result,
            InspectedAt = record.InspectedAt
        };
    }

    private static QualityRecordDetailDto CreateQualityRecordDetailDto(QualityRecord record)
    {
        return new QualityRecordDetailDto
        {
            Id = record.Id,
            EquipmentId = record.EquipmentId,
            InspectionType = record.InspectionType,
            Result = record.Result,
            InspectedAt = record.InspectedAt
        };
    }

    #endregion
}
