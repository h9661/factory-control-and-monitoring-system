using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.DTOs.WorkOrder;
using SmartFactory.Application.Exceptions;
using SmartFactory.Application.Interfaces;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Domain.ValueObjects;

namespace SmartFactory.Application.Services;

/// <summary>
/// Service implementation for work order operations.
/// </summary>
public class WorkOrderService : IWorkOrderService
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IFactoryRepository _factoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<WorkOrderCreateDto> _createValidator;
    private readonly IValidator<WorkOrderUpdateDto> _updateValidator;
    private readonly IValidator<WorkOrderProgressDto> _progressValidator;
    private readonly ILogger<WorkOrderService> _logger;

    public WorkOrderService(
        IWorkOrderRepository workOrderRepository,
        IFactoryRepository factoryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<WorkOrderCreateDto> createValidator,
        IValidator<WorkOrderUpdateDto> updateValidator,
        IValidator<WorkOrderProgressDto> progressValidator,
        ILogger<WorkOrderService> logger)
    {
        _workOrderRepository = workOrderRepository;
        _factoryRepository = factoryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _progressValidator = progressValidator;
        _logger = logger;
    }

    public async Task<PagedResult<WorkOrderDto>> GetWorkOrdersAsync(WorkOrderFilterDto filter, PaginationDto pagination, CancellationToken cancellationToken = default)
    {
        IEnumerable<WorkOrder> workOrders;

        if (filter.Status.HasValue)
        {
            workOrders = await _workOrderRepository.GetByStatusAsync(filter.Status.Value, cancellationToken);
        }
        else if (filter.DateFrom.HasValue && filter.DateTo.HasValue)
        {
            workOrders = await _workOrderRepository.GetByDateRangeAsync(
                new DateTimeRange(filter.DateFrom.Value, filter.DateTo.Value), cancellationToken);
        }
        else if (filter.FactoryId.HasValue)
        {
            workOrders = await _workOrderRepository.GetByFactoryAsync(filter.FactoryId.Value, cancellationToken);
        }
        else
        {
            workOrders = await _workOrderRepository.GetAllAsync(cancellationToken);
        }

        var query = workOrders.AsQueryable();

        if (filter.Priority.HasValue)
            query = query.Where(w => w.Priority == filter.Priority.Value);

        if (!string.IsNullOrEmpty(filter.SearchText))
            query = query.Where(w =>
                w.OrderNumber.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase) ||
                w.ProductCode.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase) ||
                w.ProductName.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase));

        var totalCount = query.Count();
        var items = query
            .OrderByDescending(w => w.CreatedAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToList();

        return PagedResult<WorkOrderDto>.Create(
            _mapper.Map<IEnumerable<WorkOrderDto>>(items),
            totalCount,
            pagination.PageNumber,
            pagination.PageSize);
    }

    public async Task<WorkOrderDetailDto?> GetWorkOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workOrder = await _workOrderRepository.GetWithStepsAsync(id, cancellationToken);
        return workOrder != null ? _mapper.Map<WorkOrderDetailDto>(workOrder) : null;
    }

    public async Task<WorkOrderDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var workOrder = await _workOrderRepository.GetByOrderNumberAsync(orderNumber, cancellationToken);
        return workOrder != null ? _mapper.Map<WorkOrderDto>(workOrder) : null;
    }

    public async Task<WorkOrderDto> CreateWorkOrderAsync(WorkOrderCreateDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw Exceptions.ValidationException.FromFluentValidation(validationResult);

        // Verify factory exists
        var factory = await _factoryRepository.GetByIdAsync(dto.FactoryId, cancellationToken);
        if (factory == null)
            throw NotFoundException.For<Factory>(dto.FactoryId);

        // Check for duplicate order number
        var existing = await _workOrderRepository.GetByOrderNumberAsync(dto.OrderNumber, cancellationToken);
        if (existing != null)
            throw DuplicateEntityException.For<WorkOrder>("OrderNumber", dto.OrderNumber);

        var workOrder = new WorkOrder(
            dto.FactoryId,
            dto.OrderNumber,
            dto.ProductCode,
            dto.ProductName,
            dto.TargetQuantity,
            dto.ScheduledStart,
            dto.ScheduledEnd);

        workOrder.SetPriority(dto.Priority);

        if (!string.IsNullOrEmpty(dto.CustomerName) || !string.IsNullOrEmpty(dto.CustomerOrderRef))
            workOrder.SetCustomerInfo(dto.CustomerName, dto.CustomerOrderRef);

        if (!string.IsNullOrEmpty(dto.Notes))
            workOrder.AddNotes(dto.Notes);

        await _workOrderRepository.AddAsync(workOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created work order {OrderNumber} for factory {FactoryId}", dto.OrderNumber, dto.FactoryId);

        return _mapper.Map<WorkOrderDto>(workOrder);
    }

    public async Task UpdateWorkOrderAsync(Guid id, WorkOrderUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw Exceptions.ValidationException.FromFluentValidation(validationResult);

        var workOrder = await _workOrderRepository.GetByIdAsync(id, cancellationToken);
        if (workOrder == null)
            throw NotFoundException.For<WorkOrder>(id);

        workOrder.UpdateSchedule(dto.ScheduledStart, dto.ScheduledEnd);
        workOrder.SetPriority(dto.Priority);
        workOrder.SetCustomerInfo(dto.CustomerName, dto.CustomerOrderRef);

        if (!string.IsNullOrEmpty(dto.Notes))
            workOrder.AddNotes(dto.Notes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated work order {WorkOrderId}", id);
    }

    public async Task StartWorkOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workOrder = await _workOrderRepository.GetByIdAsync(id, cancellationToken);
        if (workOrder == null)
            throw NotFoundException.For<WorkOrder>(id);

        try
        {
            workOrder.Start();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Started work order {WorkOrderId}", id);
        }
        catch (InvalidOperationException ex)
        {
            throw new OperationNotAllowedException("Start", ex.Message);
        }
    }

    public async Task PauseWorkOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workOrder = await _workOrderRepository.GetByIdAsync(id, cancellationToken);
        if (workOrder == null)
            throw NotFoundException.For<WorkOrder>(id);

        try
        {
            workOrder.Pause();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Paused work order {WorkOrderId}", id);
        }
        catch (InvalidOperationException ex)
        {
            throw new OperationNotAllowedException("Pause", ex.Message);
        }
    }

    public async Task ResumeWorkOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workOrder = await _workOrderRepository.GetByIdAsync(id, cancellationToken);
        if (workOrder == null)
            throw NotFoundException.For<WorkOrder>(id);

        try
        {
            workOrder.Resume();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Resumed work order {WorkOrderId}", id);
        }
        catch (InvalidOperationException ex)
        {
            throw new OperationNotAllowedException("Resume", ex.Message);
        }
    }

    public async Task CompleteWorkOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workOrder = await _workOrderRepository.GetByIdAsync(id, cancellationToken);
        if (workOrder == null)
            throw NotFoundException.For<WorkOrder>(id);

        try
        {
            workOrder.Complete();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Completed work order {WorkOrderId}", id);
        }
        catch (InvalidOperationException ex)
        {
            throw new OperationNotAllowedException("Complete", ex.Message);
        }
    }

    public async Task CancelWorkOrderAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        var workOrder = await _workOrderRepository.GetByIdAsync(id, cancellationToken);
        if (workOrder == null)
            throw NotFoundException.For<WorkOrder>(id);

        try
        {
            workOrder.Cancel(reason);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Cancelled work order {WorkOrderId}: {Reason}", id, reason);
        }
        catch (InvalidOperationException ex)
        {
            throw new OperationNotAllowedException("Cancel", ex.Message);
        }
    }

    public async Task ReportProgressAsync(Guid id, WorkOrderProgressDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _progressValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw Exceptions.ValidationException.FromFluentValidation(validationResult);

        var workOrder = await _workOrderRepository.GetByIdAsync(id, cancellationToken);
        if (workOrder == null)
            throw NotFoundException.For<WorkOrder>(id);

        workOrder.ReportProgress(dto.CompletedQuantity, dto.DefectQuantity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reported progress for work order {WorkOrderId}: {Completed}/{Defects}",
            id, dto.CompletedQuantity, dto.DefectQuantity);
    }

    public async Task<ProductionSummaryDto> GetProductionSummaryAsync(Guid? factoryId, DateTime date, CancellationToken cancellationToken = default)
    {
        var summary = await _workOrderRepository.GetProductionSummaryAsync(factoryId, date, cancellationToken);
        return new ProductionSummaryDto
        {
            Date = date,
            TotalWorkOrders = summary.TotalWorkOrders,
            CompletedWorkOrders = summary.CompletedWorkOrders,
            InProgressWorkOrders = summary.InProgressWorkOrders,
            TotalTargetUnits = summary.TargetUnits,
            TotalCompletedUnits = summary.CompletedUnits,
            TotalDefectUnits = summary.DefectUnits,
            YieldRate = summary.YieldRate
        };
    }

    public async Task<IEnumerable<WorkOrderDto>> GetActiveWorkOrdersAsync(Guid? factoryId, CancellationToken cancellationToken = default)
    {
        var workOrders = await _workOrderRepository.GetActiveWorkOrdersAsync(factoryId, cancellationToken);
        return _mapper.Map<IEnumerable<WorkOrderDto>>(workOrders);
    }
}
