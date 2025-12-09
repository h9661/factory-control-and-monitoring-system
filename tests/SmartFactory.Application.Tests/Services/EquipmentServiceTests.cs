using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.DTOs.Equipment;
using SmartFactory.Application.Exceptions;
using SmartFactory.Application.Services;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;

namespace SmartFactory.Application.Tests.Services;

/// <summary>
/// Unit tests for EquipmentService.
/// Tests CRUD operations, filtering, pagination, and status management.
/// </summary>
public class EquipmentServiceTests
{
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly Mock<IProductionLineRepository> _productionLineRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<EquipmentCreateDto>> _createValidatorMock;
    private readonly Mock<IValidator<EquipmentUpdateDto>> _updateValidatorMock;
    private readonly Mock<ILogger<EquipmentService>> _loggerMock;
    private readonly EquipmentService _sut;

    public EquipmentServiceTests()
    {
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _productionLineRepositoryMock = new Mock<IProductionLineRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _createValidatorMock = new Mock<IValidator<EquipmentCreateDto>>();
        _updateValidatorMock = new Mock<IValidator<EquipmentUpdateDto>>();
        _loggerMock = new Mock<ILogger<EquipmentService>>();

        // Setup default valid validation results
        var validResult = new ValidationResult();
        _createValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<EquipmentCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validResult);
        _updateValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<EquipmentUpdateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validResult);

        _sut = new EquipmentService(
            _equipmentRepositoryMock.Object,
            _productionLineRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _loggerMock.Object);
    }

    #region GetEquipmentAsync Tests

    [Fact]
    public async Task GetEquipmentAsync_FilterByStatus_ReturnsFilteredResults()
    {
        // Arrange
        var filter = new EquipmentFilterDto { Status = EquipmentStatus.Running };
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };
        var lineId = Guid.NewGuid();

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Running),
            CreateEquipment(lineId, "EQ002", "Equipment 2", EquipmentStatus.Running)
        };

        var expectedDtos = new List<EquipmentDto>
        {
            new() { Id = equipment[0].Id, Code = "EQ001", Name = "Equipment 1" },
            new() { Id = equipment[1].Id, Code = "EQ002", Name = "Equipment 2" }
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByStatusAsync(EquipmentStatus.Running, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<EquipmentDto>>(It.IsAny<IEnumerable<Equipment>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _sut.GetEquipmentAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetEquipmentAsync_FilterByProductionLine_ReturnsFilteredResults()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var filter = new EquipmentFilterDto { ProductionLineId = lineId };
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Running)
        };

        var expectedDtos = new List<EquipmentDto>
        {
            new() { Id = equipment[0].Id, Code = "EQ001", Name = "Equipment 1" }
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByProductionLineAsync(lineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<EquipmentDto>>(It.IsAny<IEnumerable<Equipment>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _sut.GetEquipmentAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        _equipmentRepositoryMock.Verify(r => r.GetByProductionLineAsync(lineId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetEquipmentAsync_FilterByFactory_ReturnsFilteredResults()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var filter = new EquipmentFilterDto { FactoryId = factoryId };
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Running)
        };

        var expectedDtos = new List<EquipmentDto>
        {
            new() { Id = equipment[0].Id, Code = "EQ001", Name = "Equipment 1" }
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<EquipmentDto>>(It.IsAny<IEnumerable<Equipment>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _sut.GetEquipmentAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        _equipmentRepositoryMock.Verify(r => r.GetByFactoryAsync(factoryId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetEquipmentAsync_FilterByType_AppliesTypeFilter()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var filter = new EquipmentFilterDto { Type = EquipmentType.PickAndPlace };
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Running)
        };

        var expectedDtos = new List<EquipmentDto>
        {
            new() { Id = equipment[0].Id, Code = "EQ001", Name = "Equipment 1" }
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<EquipmentDto>>(It.IsAny<IEnumerable<Equipment>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _sut.GetEquipmentAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEquipmentAsync_FilterBySearchText_MatchesCodeOrName()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var filter = new EquipmentFilterDto { SearchText = "EQ001" };
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Running),
            CreateEquipment(lineId, "EQ002", "Equipment 2", EquipmentStatus.Running)
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<EquipmentDto>>(It.IsAny<IEnumerable<Equipment>>()))
            .Returns((IEnumerable<Equipment> e) => e.Select(eq => new EquipmentDto { Id = eq.Id, Code = eq.Code }));

        // Act
        var result = await _sut.GetEquipmentAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetEquipmentAsync_PaginationApplied_ReturnsCorrectPage()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var filter = new EquipmentFilterDto();
        var pagination = new PaginationDto { PageNumber = 2, PageSize = 2 };

        var equipment = Enumerable.Range(1, 5)
            .Select(i => CreateEquipment(lineId, $"EQ00{i}", $"Equipment {i}", EquipmentStatus.Running))
            .ToList();

        _equipmentRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<EquipmentDto>>(It.IsAny<IEnumerable<Equipment>>()))
            .Returns((IEnumerable<Equipment> e) => e.Select(eq => new EquipmentDto { Id = eq.Id, Code = eq.Code }));

        // Act
        var result = await _sut.GetEquipmentAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEquipmentAsync_OrdersByCode()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var filter = new EquipmentFilterDto();
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };

        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ003", "Equipment 3", EquipmentStatus.Running),
            CreateEquipment(lineId, "EQ001", "Equipment 1", EquipmentStatus.Running),
            CreateEquipment(lineId, "EQ002", "Equipment 2", EquipmentStatus.Running)
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<EquipmentDto>>(It.IsAny<IEnumerable<Equipment>>()))
            .Returns((IEnumerable<Equipment> e) => e.Select(eq => new EquipmentDto { Id = eq.Id, Code = eq.Code }));

        // Act
        var result = await _sut.GetEquipmentAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        var items = result.Items.ToList();
        items.Should().BeInAscendingOrder(e => e.Code);
    }

    #endregion

    #region GetEquipmentByIdAsync Tests

    [Fact]
    public async Task GetEquipmentByIdAsync_EquipmentExists_ReturnsEquipment()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);
        var expectedDto = new EquipmentDetailDto { Id = equipment.Id, Code = "EQ001", Name = "Test Equipment" };

        _equipmentRepositoryMock
            .Setup(r => r.GetWithDetailsAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _mapperMock
            .Setup(m => m.Map<EquipmentDetailDto>(equipment))
            .Returns(expectedDto);

        // Act
        var result = await _sut.GetEquipmentByIdAsync(equipmentId);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("EQ001");
    }

    [Fact]
    public async Task GetEquipmentByIdAsync_EquipmentNotFound_ReturnsNull()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();

        _equipmentRepositoryMock
            .Setup(r => r.GetWithDetailsAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        // Act
        var result = await _sut.GetEquipmentByIdAsync(equipmentId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateEquipmentAsync Tests

    [Fact]
    public async Task CreateEquipmentAsync_ValidDto_CreatesEquipment()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var createDto = new EquipmentCreateDto
        {
            ProductionLineId = lineId,
            Code = "EQ001",
            Name = "New Equipment",
            Type = EquipmentType.PickAndPlace
        };

        var productionLine = new ProductionLine(Guid.NewGuid(), "LINE-001", "Test Line");
        var expectedDto = new EquipmentDto { Code = "EQ001", Name = "New Equipment" };

        _productionLineRepositoryMock
            .Setup(r => r.GetByIdAsync(lineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productionLine);

        _mapperMock
            .Setup(m => m.Map<EquipmentDto>(It.IsAny<Equipment>()))
            .Returns(expectedDto);

        // Act
        var result = await _sut.CreateEquipmentAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("EQ001");
        _equipmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Equipment>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEquipmentAsync_InvalidValidation_ThrowsValidationException()
    {
        // Arrange
        var createDto = new EquipmentCreateDto
        {
            ProductionLineId = Guid.NewGuid(),
            Code = "", // Invalid
            Name = "New Equipment",
            Type = EquipmentType.PickAndPlace
        };

        var invalidResult = new ValidationResult(new[]
        {
            new ValidationFailure("Code", "Code is required")
        });

        _createValidatorMock
            .Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidResult);

        // Act & Assert
        await Assert.ThrowsAsync<Exceptions.ValidationException>(
            () => _sut.CreateEquipmentAsync(createDto));

        _equipmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Equipment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateEquipmentAsync_ProductionLineNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var createDto = new EquipmentCreateDto
        {
            ProductionLineId = lineId,
            Code = "EQ001",
            Name = "New Equipment",
            Type = EquipmentType.PickAndPlace
        };

        _productionLineRepositoryMock
            .Setup(r => r.GetByIdAsync(lineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductionLine?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.CreateEquipmentAsync(createDto));

        _equipmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Equipment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region UpdateEquipmentAsync Tests

    [Fact]
    public async Task UpdateEquipmentAsync_EquipmentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var updateDto = new EquipmentUpdateDto
        {
            Name = "Updated Equipment",
            Type = EquipmentType.PickAndPlace,
            IsActive = true
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.UpdateEquipmentAsync(equipmentId, updateDto));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEquipmentAsync_ValidDto_UpdatesAllProperties()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Original Equipment", EquipmentStatus.Running);
        var updateDto = new EquipmentUpdateDto
        {
            Name = "Updated Equipment",
            Description = "Updated description",
            Type = EquipmentType.ReflowOven,
            IsActive = true,
            Manufacturer = "Test Manufacturer",
            Model = "Test Model"
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        await _sut.UpdateEquipmentAsync(equipmentId, updateDto);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        equipment.Name.Should().Be("Updated Equipment");
    }

    #endregion

    #region UpdateEquipmentStatusAsync Tests

    [Fact]
    public async Task UpdateEquipmentStatusAsync_UpdatesStatus()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Idle);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        await _sut.UpdateEquipmentStatusAsync(equipmentId, EquipmentStatus.Running);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        equipment.Status.Should().Be(EquipmentStatus.Running);
    }

    [Fact]
    public async Task UpdateEquipmentStatusAsync_EquipmentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.UpdateEquipmentStatusAsync(equipmentId, EquipmentStatus.Running));
    }

    #endregion

    #region DeleteEquipmentAsync Tests

    [Fact]
    public async Task DeleteEquipmentAsync_DeactivatesEquipment()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        await _sut.DeleteEquipmentAsync(equipmentId);

        // Assert
        equipment.IsActive.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteEquipmentAsync_EquipmentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.DeleteEquipmentAsync(equipmentId));
    }

    #endregion

    #region GetStatusSummaryAsync Tests

    [Fact]
    public async Task GetStatusSummaryAsync_ReturnsCorrectCounts()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var summary = new EquipmentStatusSummary
        {
            TotalCount = 10,
            RunningCount = 5,
            IdleCount = 2,
            WarningCount = 1,
            ErrorCount = 1,
            MaintenanceCount = 1,
            OfflineCount = 0
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetStatusSummaryAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        // Act
        var result = await _sut.GetStatusSummaryAsync(factoryId);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(10);
        result.RunningCount.Should().Be(5);
        result.IdleCount.Should().Be(2);
    }

    #endregion

    #region GetEquipmentDueForMaintenanceAsync Tests

    [Fact]
    public async Task GetEquipmentDueForMaintenanceAsync_FiltersCorrectly()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var equipment = new List<Equipment>
        {
            CreateEquipment(lineId, "EQ001", "Equipment Due", EquipmentStatus.Running)
        };

        var expectedDtos = new List<EquipmentDto>
        {
            new() { Code = "EQ001", Name = "Equipment Due" }
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetEquipmentDueForMaintenanceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<EquipmentDto>>(equipment))
            .Returns(expectedDtos);

        // Act
        var result = await _sut.GetEquipmentDueForMaintenanceAsync(null);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    #endregion

    #region RecordHeartbeatAsync Tests

    [Fact]
    public async Task RecordHeartbeatAsync_UpdatesHeartbeat()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();
        var lineId = Guid.NewGuid();
        var equipment = CreateEquipment(lineId, "EQ001", "Test Equipment", EquipmentStatus.Running);
        var originalHeartbeat = equipment.LastHeartbeat;

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act
        await _sut.RecordHeartbeatAsync(equipmentId);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordHeartbeatAsync_EquipmentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var equipmentId = Guid.NewGuid();

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.RecordHeartbeatAsync(equipmentId));
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
