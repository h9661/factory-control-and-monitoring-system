using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.DTOs.Maintenance;
using SmartFactory.Application.Exceptions;
using SmartFactory.Application.Services;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using Xunit;

namespace SmartFactory.Application.Tests.Services;

/// <summary>
/// Unit tests for MaintenanceService covering scheduling, state management, and summary operations.
/// </summary>
public class MaintenanceServiceTests
{
    private readonly Mock<IMaintenanceRepository> _maintenanceRepositoryMock;
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<MaintenanceCreateDto>> _createValidatorMock;
    private readonly Mock<IValidator<MaintenanceCompleteDto>> _completeValidatorMock;
    private readonly Mock<IValidator<MaintenanceRescheduleDto>> _rescheduleValidatorMock;
    private readonly Mock<ILogger<MaintenanceService>> _loggerMock;
    private readonly MaintenanceService _sut;

    public MaintenanceServiceTests()
    {
        _maintenanceRepositoryMock = new Mock<IMaintenanceRepository>();
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _createValidatorMock = new Mock<IValidator<MaintenanceCreateDto>>();
        _completeValidatorMock = new Mock<IValidator<MaintenanceCompleteDto>>();
        _rescheduleValidatorMock = new Mock<IValidator<MaintenanceRescheduleDto>>();
        _loggerMock = new Mock<ILogger<MaintenanceService>>();

        _sut = new MaintenanceService(
            _maintenanceRepositoryMock.Object,
            _equipmentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _createValidatorMock.Object,
            _completeValidatorMock.Object,
            _rescheduleValidatorMock.Object,
            _loggerMock.Object);
    }

    #region GetMaintenanceRecordsAsync Tests

    [Fact]
    public async Task GetMaintenanceRecordsAsync_WithStatusFilter_ReturnsFilteredRecords()
    {
        // Arrange
        var filter = new MaintenanceFilterDto { Status = MaintenanceStatus.Scheduled };
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };
        var records = CreateTestMaintenanceRecords(5);
        var dtos = records.Select(r => CreateMaintenanceRecordDto(r)).ToList();

        _maintenanceRepositoryMock.Setup(x => x.GetByStatusAsync(MaintenanceStatus.Scheduled, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);
        _mapperMock.Setup(x => x.Map<IEnumerable<MaintenanceRecordDto>>(It.IsAny<IEnumerable<MaintenanceRecord>>()))
            .Returns(dtos);

        // Act
        var result = await _sut.GetMaintenanceRecordsAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(5);
        _maintenanceRepositoryMock.Verify(x => x.GetByStatusAsync(MaintenanceStatus.Scheduled, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMaintenanceRecordsAsync_WithEquipmentIdFilter_ReturnsFilteredRecords()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var filter = new MaintenanceFilterDto { EquipmentId = equipmentId };
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };
        var records = CreateTestMaintenanceRecords(3);
        var dtos = records.Select(r => CreateMaintenanceRecordDto(r)).ToList();

        _maintenanceRepositoryMock.Setup(x => x.GetByEquipmentAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);
        _mapperMock.Setup(x => x.Map<IEnumerable<MaintenanceRecordDto>>(It.IsAny<IEnumerable<MaintenanceRecord>>()))
            .Returns(dtos);

        // Act
        var result = await _sut.GetMaintenanceRecordsAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        _maintenanceRepositoryMock.Verify(x => x.GetByEquipmentAsync(equipmentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMaintenanceRecordsAsync_WithDateRangeFilter_ReturnsFilteredRecords()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var filter = new MaintenanceFilterDto { DateFrom = startDate, DateTo = endDate };
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };
        var records = CreateTestMaintenanceRecords(4);
        var dtos = records.Select(r => CreateMaintenanceRecordDto(r)).ToList();

        _maintenanceRepositoryMock.Setup(x => x.GetByDateRangeAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);
        _mapperMock.Setup(x => x.Map<IEnumerable<MaintenanceRecordDto>>(It.IsAny<IEnumerable<MaintenanceRecord>>()))
            .Returns(dtos);

        // Act
        var result = await _sut.GetMaintenanceRecordsAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(4);
        _maintenanceRepositoryMock.Verify(x => x.GetByDateRangeAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMaintenanceRecordsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var filter = new MaintenanceFilterDto();
        var pagination = new PaginationDto { PageNumber = 2, PageSize = 5 };
        var records = CreateTestMaintenanceRecords(12);
        var dtos = records.Select(r => CreateMaintenanceRecordDto(r)).ToList();

        _maintenanceRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);
        _mapperMock.Setup(x => x.Map<IEnumerable<MaintenanceRecordDto>>(It.IsAny<IEnumerable<MaintenanceRecord>>()))
            .Returns(dtos.Skip(5).Take(5));

        // Act
        var result = await _sut.GetMaintenanceRecordsAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(12);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
    }

    #endregion

    #region GetMaintenanceRecordByIdAsync Tests

    [Fact]
    public async Task GetMaintenanceRecordByIdAsync_RecordExists_ReturnsDetailDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var record = CreateTestMaintenanceRecord(id);
        var dto = CreateMaintenanceRecordDetailDto(record);

        _maintenanceRepositoryMock.Setup(x => x.GetWithEquipmentAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _mapperMock.Setup(x => x.Map<MaintenanceRecordDetailDto>(record))
            .Returns(dto);

        // Act
        var result = await _sut.GetMaintenanceRecordByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetMaintenanceRecordByIdAsync_RecordNotFound_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _maintenanceRepositoryMock.Setup(x => x.GetWithEquipmentAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MaintenanceRecord?)null);

        // Act
        var result = await _sut.GetMaintenanceRecordByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ScheduleMaintenanceAsync Tests

    [Fact]
    public async Task ScheduleMaintenanceAsync_WithValidDto_CreatesAndReturnsRecord()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var dto = new MaintenanceCreateDto
        {
            EquipmentId = equipmentId,
            Type = MaintenanceType.Preventive,
            Title = "Scheduled Maintenance",
            ScheduledDate = DateTime.UtcNow.AddDays(7),
            Description = "Regular maintenance",
            TechnicianId = "TECH001",
            TechnicianName = "John Doe",
            EstimatedCost = 500
        };
        var equipment = CreateTestEquipment(equipmentId);
        var resultDto = new MaintenanceRecordDto { Id = Guid.NewGuid(), Title = dto.Title };

        _createValidatorMock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _equipmentRepositoryMock.Setup(x => x.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);
        _mapperMock.Setup(x => x.Map<MaintenanceRecordDto>(It.IsAny<MaintenanceRecord>()))
            .Returns(resultDto);

        // Act
        var result = await _sut.ScheduleMaintenanceAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(dto.Title);
        _maintenanceRepositoryMock.Verify(x => x.AddAsync(It.IsAny<MaintenanceRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ScheduleMaintenanceAsync_WithValidationFailure_ThrowsValidationException()
    {
        // Arrange
        var dto = new MaintenanceCreateDto
        {
            EquipmentId = Guid.NewGuid(),
            Type = MaintenanceType.Preventive,
            Title = "",
            ScheduledDate = DateTime.UtcNow.AddDays(-1)
        };
        var validationResult = new ValidationResult(new[] { new ValidationFailure("Title", "Title is required") });
        _createValidatorMock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var act = () => _sut.ScheduleMaintenanceAsync(dto);

        // Assert
        await act.Should().ThrowAsync<Exceptions.ValidationException>();
    }

    [Fact]
    public async Task ScheduleMaintenanceAsync_EquipmentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var dto = new MaintenanceCreateDto
        {
            EquipmentId = equipmentId,
            Type = MaintenanceType.Preventive,
            Title = "Test Maintenance",
            ScheduledDate = DateTime.UtcNow.AddDays(7)
        };

        _createValidatorMock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _equipmentRepositoryMock.Setup(x => x.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        // Act
        var act = () => _sut.ScheduleMaintenanceAsync(dto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region StartMaintenanceAsync Tests

    [Fact]
    public async Task StartMaintenanceAsync_RecordExists_StartsMaintenanceSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var record = CreateTestMaintenanceRecord(id);

        _maintenanceRepositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        // Act
        await _sut.StartMaintenanceAsync(id);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartMaintenanceAsync_RecordNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _maintenanceRepositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MaintenanceRecord?)null);

        // Act
        var act = () => _sut.StartMaintenanceAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region CompleteMaintenanceAsync Tests

    [Fact]
    public async Task CompleteMaintenanceAsync_WithValidData_CompletesSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var record = CreateTestMaintenanceRecord(id, equipmentId);
        record.Start();
        var equipment = CreateTestEquipment(equipmentId);
        var dto = new MaintenanceCompleteDto
        {
            ActualCost = 450,
            DowntimeMinutes = 120,
            Notes = "Completed successfully",
            PartsUsed = "Filter, Oil"
        };

        _completeValidatorMock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _maintenanceRepositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _equipmentRepositoryMock.Setup(x => x.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        await _sut.CompleteMaintenanceAsync(id, dto);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteMaintenanceAsync_RecordNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new MaintenanceCompleteDto { ActualCost = 100, DowntimeMinutes = 60 };

        _completeValidatorMock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _maintenanceRepositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MaintenanceRecord?)null);

        // Act
        var act = () => _sut.CompleteMaintenanceAsync(id, dto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CompleteMaintenanceAsync_WithValidationFailure_ThrowsValidationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new MaintenanceCompleteDto { ActualCost = -100, DowntimeMinutes = -60 };
        var validationResult = new ValidationResult(new[] { new ValidationFailure("ActualCost", "Must be positive") });

        _completeValidatorMock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var act = () => _sut.CompleteMaintenanceAsync(id, dto);

        // Assert
        await act.Should().ThrowAsync<Exceptions.ValidationException>();
    }

    #endregion

    #region CancelMaintenanceAsync Tests

    [Fact]
    public async Task CancelMaintenanceAsync_RecordExists_CancelsSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var record = CreateTestMaintenanceRecord(id);
        var reason = "Equipment no longer in use";

        _maintenanceRepositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        // Act
        await _sut.CancelMaintenanceAsync(id, reason);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelMaintenanceAsync_RecordNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _maintenanceRepositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MaintenanceRecord?)null);

        // Act
        var act = () => _sut.CancelMaintenanceAsync(id, "Reason");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region RescheduleMaintenanceAsync Tests

    [Fact]
    public async Task RescheduleMaintenanceAsync_WithValidData_ReschedulesSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var record = CreateTestMaintenanceRecord(id);
        var dto = new MaintenanceRescheduleDto { NewScheduledDate = DateTime.UtcNow.AddDays(14) };

        _rescheduleValidatorMock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _maintenanceRepositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        // Act
        await _sut.RescheduleMaintenanceAsync(id, dto);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RescheduleMaintenanceAsync_RecordNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new MaintenanceRescheduleDto { NewScheduledDate = DateTime.UtcNow.AddDays(14) };

        _rescheduleValidatorMock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _maintenanceRepositoryMock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MaintenanceRecord?)null);

        // Act
        var act = () => _sut.RescheduleMaintenanceAsync(id, dto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetOverdueMaintenanceAsync Tests

    [Fact]
    public async Task GetOverdueMaintenanceAsync_WithOverdueRecords_ReturnsAlerts()
    {
        // Arrange
        var records = CreateOverdueMaintenanceRecords(3);
        _maintenanceRepositoryMock.Setup(x => x.GetOverdueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        // Act
        var result = await _sut.GetOverdueMaintenanceAsync(null);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.All(r => r.IsOverdue).Should().BeTrue();
    }

    [Fact]
    public async Task GetOverdueMaintenanceAsync_WithFactoryFilter_ReturnsFilteredAlerts()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var overdueRecords = CreateOverdueMaintenanceRecords(5);
        var factoryRecords = overdueRecords.Take(2).ToList();

        _maintenanceRepositoryMock.Setup(x => x.GetOverdueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(overdueRecords);
        _maintenanceRepositoryMock.Setup(x => x.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(factoryRecords);

        // Act
        var result = await _sut.GetOverdueMaintenanceAsync(factoryId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetMaintenanceSummaryAsync Tests

    [Fact]
    public async Task GetMaintenanceSummaryAsync_WithRecords_ReturnsCorrectSummary()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var records = CreateMixedStatusMaintenanceRecords();

        _maintenanceRepositoryMock.Setup(x => x.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        // Act
        var result = await _sut.GetMaintenanceSummaryAsync(factoryId, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TotalScheduled.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetMaintenanceSummaryAsync_WithNoRecords_ReturnsZeroValues()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        _maintenanceRepositoryMock.Setup(x => x.GetByDateRangeAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<MaintenanceRecord>());

        // Act
        var result = await _sut.GetMaintenanceSummaryAsync(null, startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.TotalScheduled.Should().Be(0);
        result.CompletionRate.Should().Be(0);
    }

    #endregion

    #region Helper Methods

    private static List<MaintenanceRecord> CreateTestMaintenanceRecords(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => CreateTestMaintenanceRecord(Guid.NewGuid()))
            .ToList();
    }

    private static MaintenanceRecord CreateTestMaintenanceRecord(Guid id, Guid? equipmentId = null)
    {
        var record = new MaintenanceRecord(
            equipmentId ?? Guid.NewGuid(),
            MaintenanceType.Preventive,
            $"Maintenance {id}",
            DateTime.UtcNow.AddDays(7));

        var idProperty = typeof(MaintenanceRecord).GetProperty("Id");
        idProperty?.SetValue(record, id);

        return record;
    }

    private static List<MaintenanceRecord> CreateOverdueMaintenanceRecords(int count)
    {
        var records = new List<MaintenanceRecord>();
        for (int i = 0; i < count; i++)
        {
            var record = new MaintenanceRecord(
                Guid.NewGuid(),
                MaintenanceType.Preventive,
                $"Overdue Maintenance {i}",
                DateTime.UtcNow.AddDays(-i - 1));
            records.Add(record);
        }
        return records;
    }

    private static List<MaintenanceRecord> CreateMixedStatusMaintenanceRecords()
    {
        var records = new List<MaintenanceRecord>();
        var scheduledDate = DateTime.UtcNow;

        for (int i = 0; i < 10; i++)
        {
            var record = new MaintenanceRecord(
                Guid.NewGuid(),
                i % 3 == 0 ? MaintenanceType.Preventive : (i % 3 == 1 ? MaintenanceType.Corrective : MaintenanceType.Predictive),
                $"Maintenance {i}",
                scheduledDate.AddDays(-i));
            records.Add(record);
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

    private static MaintenanceRecordDto CreateMaintenanceRecordDto(MaintenanceRecord record)
    {
        return new MaintenanceRecordDto
        {
            Id = record.Id,
            EquipmentId = record.EquipmentId,
            Type = record.Type,
            Title = record.Title,
            Status = record.Status,
            ScheduledDate = record.ScheduledDate
        };
    }

    private static MaintenanceRecordDetailDto CreateMaintenanceRecordDetailDto(MaintenanceRecord record)
    {
        return new MaintenanceRecordDetailDto
        {
            Id = record.Id,
            EquipmentId = record.EquipmentId,
            Type = record.Type,
            Title = record.Title,
            Status = record.Status,
            ScheduledDate = record.ScheduledDate
        };
    }

    #endregion
}
