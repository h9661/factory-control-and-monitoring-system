using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SmartFactory.Application.DTOs.Alarm;
using SmartFactory.Application.Services;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Domain.ValueObjects;

namespace SmartFactory.Application.Tests.Services;

/// <summary>
/// Unit tests for AlarmService.
/// </summary>
public class AlarmServiceTests
{
    private readonly Mock<IAlarmRepository> _alarmRepositoryMock;
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<AlarmCreateDto>> _createValidatorMock;
    private readonly Mock<IValidator<AlarmAcknowledgeDto>> _acknowledgeValidatorMock;
    private readonly Mock<IValidator<AlarmResolveDto>> _resolveValidatorMock;
    private readonly Mock<ILogger<AlarmService>> _loggerMock;
    private readonly AlarmService _sut;

    public AlarmServiceTests()
    {
        _alarmRepositoryMock = new Mock<IAlarmRepository>();
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _createValidatorMock = new Mock<IValidator<AlarmCreateDto>>();
        _acknowledgeValidatorMock = new Mock<IValidator<AlarmAcknowledgeDto>>();
        _resolveValidatorMock = new Mock<IValidator<AlarmResolveDto>>();
        _loggerMock = new Mock<ILogger<AlarmService>>();

        // Setup default valid validation result
        var validResult = new ValidationResult();
        _createValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<AlarmCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validResult);
        _acknowledgeValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<AlarmAcknowledgeDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validResult);
        _resolveValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<AlarmResolveDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validResult);

        _sut = new AlarmService(
            _alarmRepositoryMock.Object,
            _equipmentRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _createValidatorMock.Object,
            _acknowledgeValidatorMock.Object,
            _resolveValidatorMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAlarmSummaryAsync_ReturnsCorrectSummary()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var expectedSummary = new AlarmSummary
        {
            TotalActive = 10,
            CriticalCount = 2,
            ErrorCount = 3,
            WarningCount = 3,
            InformationCount = 2,
            AcknowledgedCount = 4,
            UnacknowledgedCount = 6
        };

        _alarmRepositoryMock
            .Setup(r => r.GetAlarmSummaryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _sut.GetAlarmSummaryAsync(factoryId);

        // Assert
        result.Should().NotBeNull();
        result.TotalActive.Should().Be(10);
        result.CriticalCount.Should().Be(2);
        result.ErrorCount.Should().Be(3);
        result.WarningCount.Should().Be(3);
        result.InformationCount.Should().Be(2);
        result.AcknowledgedCount.Should().Be(4);
        result.UnacknowledgedCount.Should().Be(6);
    }

    [Fact]
    public async Task GetAlarmSummaryAsync_WithNoFactory_ReturnsAllAlarmsSummary()
    {
        // Arrange
        var expectedSummary = new AlarmSummary { TotalActive = 5 };

        _alarmRepositoryMock
            .Setup(r => r.GetAlarmSummaryAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _sut.GetAlarmSummaryAsync(null);

        // Assert
        result.Should().NotBeNull();
        result.TotalActive.Should().Be(5);
    }

    [Fact]
    public async Task GetActiveAlarmsAsync_ReturnsActiveAlarms()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();

        var alarms = new List<Alarm>
        {
            CreateAlarm(equipmentId, "ALM001", AlarmSeverity.Critical),
            CreateAlarm(equipmentId, "ALM002", AlarmSeverity.Warning)
        };

        var expectedDtos = new List<AlarmDto>
        {
            new() { Id = alarms[0].Id, AlarmCode = "ALM001", Severity = AlarmSeverity.Critical },
            new() { Id = alarms[1].Id, AlarmCode = "ALM002", Severity = AlarmSeverity.Warning }
        };

        _alarmRepositoryMock
            .Setup(r => r.GetActiveAlarmsAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alarms);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<AlarmDto>>(alarms))
            .Returns(expectedDtos);

        // Act
        var result = await _sut.GetActiveAlarmsAsync(factoryId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().AlarmCode.Should().Be("ALM001");
        result.First().Severity.Should().Be(AlarmSeverity.Critical);
    }

    [Fact]
    public async Task AcknowledgeAlarmAsync_ValidAlarm_UpdatesStatus()
    {
        // Arrange
        var alarmId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var acknowledgeDto = new AlarmAcknowledgeDto { UserId = "TestUser" };

        var existingAlarm = CreateAlarm(equipmentId, "ALM001", AlarmSeverity.Warning);

        _alarmRepositoryMock
            .Setup(r => r.GetByIdAsync(alarmId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAlarm);

        // Act
        await _sut.AcknowledgeAlarmAsync(alarmId, acknowledgeDto);

        // Assert
        existingAlarm.Status.Should().Be(AlarmStatus.Acknowledged);
        existingAlarm.AcknowledgedAt.Should().NotBeNull();
        existingAlarm.AcknowledgedBy.Should().Be("TestUser");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AcknowledgeAlarmAsync_AlarmNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var alarmId = Guid.NewGuid();
        var acknowledgeDto = new AlarmAcknowledgeDto { UserId = "TestUser" };

        _alarmRepositoryMock
            .Setup(r => r.GetByIdAsync(alarmId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Alarm?)null);

        // Act & Assert
        await Assert.ThrowsAsync<SmartFactory.Application.Exceptions.NotFoundException>(
            () => _sut.AcknowledgeAlarmAsync(alarmId, acknowledgeDto));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResolveAlarmAsync_ValidAlarm_UpdatesStatus()
    {
        // Arrange
        var alarmId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var resolveDto = new AlarmResolveDto
        {
            UserId = "TestUser",
            ResolutionNotes = "Issue resolved by replacing sensor"
        };

        var existingAlarm = CreateAlarm(equipmentId, "ALM001", AlarmSeverity.Warning);

        _alarmRepositoryMock
            .Setup(r => r.GetByIdAsync(alarmId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAlarm);

        // Act
        await _sut.ResolveAlarmAsync(alarmId, resolveDto);

        // Assert
        existingAlarm.Status.Should().Be(AlarmStatus.Resolved);
        existingAlarm.ResolvedAt.Should().NotBeNull();
        existingAlarm.ResolvedBy.Should().Be("TestUser");
        existingAlarm.ResolutionNotes.Should().Be("Issue resolved by replacing sensor");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRecentAlarmsAsync_ReturnsLimitedResults()
    {
        // Arrange
        var count = 5;
        var equipmentId = Guid.NewGuid();
        var alarms = Enumerable.Range(1, count).Select(i =>
            CreateAlarm(equipmentId, $"ALM{i:D3}", AlarmSeverity.Warning)).ToList();

        var expectedDtos = alarms.Select(a => new AlarmDto
        {
            Id = a.Id,
            AlarmCode = a.AlarmCode
        }).ToList();

        _alarmRepositoryMock
            .Setup(r => r.GetRecentAlarmsAsync(count, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alarms);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<AlarmDto>>(alarms))
            .Returns(expectedDtos);

        // Act
        var result = await _sut.GetRecentAlarmsAsync(count, null);

        // Assert
        result.Should().HaveCount(count);
    }

    [Fact]
    public async Task GetActiveAlarmCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var expectedCount = 15;

        _alarmRepositoryMock
            .Setup(r => r.GetActiveAlarmCountAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _sut.GetActiveAlarmCountAsync(factoryId);

        // Assert
        result.Should().Be(expectedCount);
    }

    /// <summary>
    /// Helper method to create Alarm entities for testing.
    /// </summary>
    private static Alarm CreateAlarm(Guid equipmentId, string alarmCode, AlarmSeverity severity)
    {
        return new Alarm(
            equipmentId,
            alarmCode,
            severity,
            $"Test message for {alarmCode}",
            DateTime.UtcNow);
    }
}
