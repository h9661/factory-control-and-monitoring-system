using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.DTOs.WorkOrder;
using SmartFactory.Application.Exceptions;
using SmartFactory.Application.Services;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Domain.ValueObjects;

namespace SmartFactory.Application.Tests.Services;

/// <summary>
/// Unit tests for WorkOrderService.
/// Tests CRUD operations, state management, and progress reporting.
/// </summary>
public class WorkOrderServiceTests
{
    private readonly Mock<IWorkOrderRepository> _workOrderRepositoryMock;
    private readonly Mock<IFactoryRepository> _factoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<WorkOrderCreateDto>> _createValidatorMock;
    private readonly Mock<IValidator<WorkOrderUpdateDto>> _updateValidatorMock;
    private readonly Mock<IValidator<WorkOrderProgressDto>> _progressValidatorMock;
    private readonly Mock<ILogger<WorkOrderService>> _loggerMock;
    private readonly WorkOrderService _sut;

    public WorkOrderServiceTests()
    {
        _workOrderRepositoryMock = new Mock<IWorkOrderRepository>();
        _factoryRepositoryMock = new Mock<IFactoryRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _createValidatorMock = new Mock<IValidator<WorkOrderCreateDto>>();
        _updateValidatorMock = new Mock<IValidator<WorkOrderUpdateDto>>();
        _progressValidatorMock = new Mock<IValidator<WorkOrderProgressDto>>();
        _loggerMock = new Mock<ILogger<WorkOrderService>>();

        // Setup default valid validation results
        var validResult = new ValidationResult();
        _createValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<WorkOrderCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validResult);
        _updateValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<WorkOrderUpdateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validResult);
        _progressValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<WorkOrderProgressDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validResult);

        _sut = new WorkOrderService(
            _workOrderRepositoryMock.Object,
            _factoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            _progressValidatorMock.Object,
            _loggerMock.Object);
    }

    #region GetWorkOrdersAsync Tests

    [Fact]
    public async Task GetWorkOrdersAsync_FilterByStatus_ReturnsFiltered()
    {
        // Arrange
        var filter = new WorkOrderFilterDto { Status = WorkOrderStatus.InProgress };
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };

        var workOrders = new List<WorkOrder>
        {
            CreateWorkOrder(Guid.NewGuid(), "WO-001", 100)
        };

        var expectedDtos = new List<WorkOrderDto>
        {
            new() { OrderNumber = "WO-001" }
        };

        _workOrderRepositoryMock
            .Setup(r => r.GetByStatusAsync(WorkOrderStatus.InProgress, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<WorkOrderDto>>(It.IsAny<IEnumerable<WorkOrder>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _sut.GetWorkOrdersAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        _workOrderRepositoryMock.Verify(r => r.GetByStatusAsync(WorkOrderStatus.InProgress, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetWorkOrdersAsync_FilterByDateRange_ReturnsFiltered()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var filter = new WorkOrderFilterDto { DateFrom = startDate, DateTo = endDate };
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };

        var workOrders = new List<WorkOrder>
        {
            CreateWorkOrder(Guid.NewGuid(), "WO-001", 100)
        };

        _workOrderRepositoryMock
            .Setup(r => r.GetByDateRangeAsync(It.IsAny<DateTimeRange>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<WorkOrderDto>>(It.IsAny<IEnumerable<WorkOrder>>()))
            .Returns(new List<WorkOrderDto> { new() { OrderNumber = "WO-001" } });

        // Act
        var result = await _sut.GetWorkOrdersAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        _workOrderRepositoryMock.Verify(r => r.GetByDateRangeAsync(It.IsAny<DateTimeRange>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetWorkOrdersAsync_SearchText_MatchesOrderOrProduct()
    {
        // Arrange
        var filter = new WorkOrderFilterDto { SearchText = "WO-001" };
        var pagination = new PaginationDto { PageNumber = 1, PageSize = 10 };

        var workOrders = new List<WorkOrder>
        {
            CreateWorkOrder(Guid.NewGuid(), "WO-001", 100),
            CreateWorkOrder(Guid.NewGuid(), "WO-002", 100)
        };

        _workOrderRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<WorkOrderDto>>(It.IsAny<IEnumerable<WorkOrder>>()))
            .Returns((IEnumerable<WorkOrder> wo) => wo.Select(w => new WorkOrderDto { OrderNumber = w.OrderNumber }));

        // Act
        var result = await _sut.GetWorkOrdersAsync(filter, pagination);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
    }

    #endregion

    #region GetWorkOrderByIdAsync Tests

    [Fact]
    public async Task GetWorkOrderByIdAsync_WorkOrderExists_ReturnsWorkOrder()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();
        var workOrder = CreateWorkOrder(Guid.NewGuid(), "WO-001", 100);
        var expectedDto = new WorkOrderDetailDto { OrderNumber = "WO-001" };

        _workOrderRepositoryMock
            .Setup(r => r.GetWithStepsAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrder);

        _mapperMock
            .Setup(m => m.Map<WorkOrderDetailDto>(workOrder))
            .Returns(expectedDto);

        // Act
        var result = await _sut.GetWorkOrderByIdAsync(workOrderId);

        // Assert
        result.Should().NotBeNull();
        result!.OrderNumber.Should().Be("WO-001");
    }

    [Fact]
    public async Task GetWorkOrderByIdAsync_WorkOrderNotFound_ReturnsNull()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();

        _workOrderRepositoryMock
            .Setup(r => r.GetWithStepsAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder?)null);

        // Act
        var result = await _sut.GetWorkOrderByIdAsync(workOrderId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateWorkOrderAsync Tests

    [Fact]
    public async Task CreateWorkOrderAsync_ValidDto_CreatesWorkOrder()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var createDto = new WorkOrderCreateDto
        {
            FactoryId = factoryId,
            OrderNumber = "WO-NEW",
            ProductCode = "PROD001",
            ProductName = "Test Product",
            TargetQuantity = 100,
            ScheduledStart = DateTime.UtcNow,
            ScheduledEnd = DateTime.UtcNow.AddDays(1),
            Priority = WorkOrderPriority.High
        };

        var factory = new Factory("Test Factory", "Test Location");
        var expectedDto = new WorkOrderDto { OrderNumber = "WO-NEW" };

        _factoryRepositoryMock
            .Setup(r => r.GetByIdAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(factory);

        _workOrderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(createDto.OrderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder?)null);

        _mapperMock
            .Setup(m => m.Map<WorkOrderDto>(It.IsAny<WorkOrder>()))
            .Returns(expectedDto);

        // Act
        var result = await _sut.CreateWorkOrderAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.OrderNumber.Should().Be("WO-NEW");
        _workOrderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateWorkOrderAsync_DuplicateOrderNumber_ThrowsDuplicateException()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var createDto = new WorkOrderCreateDto
        {
            FactoryId = factoryId,
            OrderNumber = "WO-EXISTING",
            ProductCode = "PROD001",
            ProductName = "Test Product",
            TargetQuantity = 100,
            ScheduledStart = DateTime.UtcNow,
            ScheduledEnd = DateTime.UtcNow.AddDays(1)
        };

        var factory = new Factory("Test Factory", "Test Location");
        var existingWorkOrder = CreateWorkOrder(factoryId, "WO-EXISTING", 100);

        _factoryRepositoryMock
            .Setup(r => r.GetByIdAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(factory);

        _workOrderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync(createDto.OrderNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingWorkOrder);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateEntityException>(
            () => _sut.CreateWorkOrderAsync(createDto));
    }

    [Fact]
    public async Task CreateWorkOrderAsync_FactoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var createDto = new WorkOrderCreateDto
        {
            FactoryId = factoryId,
            OrderNumber = "WO-001",
            ProductCode = "PROD001",
            ProductName = "Test Product",
            TargetQuantity = 100,
            ScheduledStart = DateTime.UtcNow,
            ScheduledEnd = DateTime.UtcNow.AddDays(1)
        };

        _factoryRepositoryMock
            .Setup(r => r.GetByIdAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Factory?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.CreateWorkOrderAsync(createDto));
    }

    #endregion

    #region StartWorkOrderAsync Tests

    [Fact]
    public async Task StartWorkOrderAsync_ValidWorkOrder_StartsOrder()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();
        var workOrder = CreateWorkOrder(Guid.NewGuid(), "WO-001", 100);

        _workOrderRepositoryMock
            .Setup(r => r.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrder);

        // Act
        await _sut.StartWorkOrderAsync(workOrderId);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartWorkOrderAsync_WorkOrderNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();

        _workOrderRepositoryMock
            .Setup(r => r.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.StartWorkOrderAsync(workOrderId));
    }

    #endregion

    #region PauseWorkOrderAsync Tests

    [Fact]
    public async Task PauseWorkOrderAsync_WorkOrderNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();

        _workOrderRepositoryMock
            .Setup(r => r.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.PauseWorkOrderAsync(workOrderId));
    }

    #endregion

    #region ResumeWorkOrderAsync Tests

    [Fact]
    public async Task ResumeWorkOrderAsync_WorkOrderNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();

        _workOrderRepositoryMock
            .Setup(r => r.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.ResumeWorkOrderAsync(workOrderId));
    }

    #endregion

    #region CompleteWorkOrderAsync Tests

    [Fact]
    public async Task CompleteWorkOrderAsync_WorkOrderNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();

        _workOrderRepositoryMock
            .Setup(r => r.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.CompleteWorkOrderAsync(workOrderId));
    }

    #endregion

    #region CancelWorkOrderAsync Tests

    [Fact]
    public async Task CancelWorkOrderAsync_ValidOrder_CancelsWithReason()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();
        var reason = "Customer request";
        var workOrder = CreateWorkOrder(Guid.NewGuid(), "WO-001", 100);

        _workOrderRepositoryMock
            .Setup(r => r.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrder);

        // Act
        await _sut.CancelWorkOrderAsync(workOrderId, reason);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelWorkOrderAsync_WorkOrderNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();

        _workOrderRepositoryMock
            .Setup(r => r.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.CancelWorkOrderAsync(workOrderId, "reason"));
    }

    #endregion

    #region ReportProgressAsync Tests

    [Fact]
    public async Task ReportProgressAsync_ValidProgress_UpdatesQuantities()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();
        var progressDto = new WorkOrderProgressDto
        {
            CompletedQuantity = 50,
            DefectQuantity = 2
        };
        var workOrder = CreateWorkOrder(Guid.NewGuid(), "WO-001", 100);
        workOrder.Start(); // Need to start before reporting progress

        _workOrderRepositoryMock
            .Setup(r => r.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrder);

        // Act
        await _sut.ReportProgressAsync(workOrderId, progressDto);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReportProgressAsync_WorkOrderNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();
        var progressDto = new WorkOrderProgressDto
        {
            CompletedQuantity = 50,
            DefectQuantity = 2
        };

        _workOrderRepositoryMock
            .Setup(r => r.GetByIdAsync(workOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkOrder?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.ReportProgressAsync(workOrderId, progressDto));
    }

    #endregion

    #region GetProductionSummaryAsync Tests

    [Fact]
    public async Task GetProductionSummaryAsync_ReturnsCorrectSummary()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var summary = new ProductionSummary
        {
            Date = date,
            TotalWorkOrders = 10,
            CompletedWorkOrders = 5,
            InProgressWorkOrders = 3,
            TargetUnits = 1000,
            CompletedUnits = 500,
            DefectUnits = 10
        };

        _workOrderRepositoryMock
            .Setup(r => r.GetProductionSummaryAsync(factoryId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        // Act
        var result = await _sut.GetProductionSummaryAsync(factoryId, date);

        // Assert
        result.Should().NotBeNull();
        result.TotalWorkOrders.Should().Be(10);
        result.CompletedWorkOrders.Should().Be(5);
        result.TotalTargetUnits.Should().Be(1000);
    }

    #endregion

    #region GetActiveWorkOrdersAsync Tests

    [Fact]
    public async Task GetActiveWorkOrdersAsync_ReturnsActiveWorkOrders()
    {
        // Arrange
        var factoryId = Guid.NewGuid();
        var workOrders = new List<WorkOrder>
        {
            CreateWorkOrder(factoryId, "WO-001", 100),
            CreateWorkOrder(factoryId, "WO-002", 200)
        };

        var expectedDtos = new List<WorkOrderDto>
        {
            new() { OrderNumber = "WO-001" },
            new() { OrderNumber = "WO-002" }
        };

        _workOrderRepositoryMock
            .Setup(r => r.GetActiveWorkOrdersAsync(factoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<WorkOrderDto>>(workOrders))
            .Returns(expectedDtos);

        // Act
        var result = await _sut.GetActiveWorkOrdersAsync(factoryId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    #endregion

    #region Helper Methods

    private static WorkOrder CreateWorkOrder(Guid factoryId, string orderNumber, int targetQuantity)
    {
        return new WorkOrder(
            factoryId,
            orderNumber,
            "PROD001",
            "Test Product",
            targetQuantity,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1));
    }

    #endregion
}
