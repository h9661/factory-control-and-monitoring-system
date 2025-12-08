using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.DTOs.Maintenance;
using SmartFactory.Application.Exceptions;
using SmartFactory.Application.Interfaces;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Domain.ValueObjects;

namespace SmartFactory.Application.Services;

/// <summary>
/// Service implementation for maintenance operations.
/// </summary>
public class MaintenanceService : IMaintenanceService
{
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<MaintenanceCreateDto> _createValidator;
    private readonly IValidator<MaintenanceCompleteDto> _completeValidator;
    private readonly IValidator<MaintenanceRescheduleDto> _rescheduleValidator;
    private readonly ILogger<MaintenanceService> _logger;

    public MaintenanceService(
        IMaintenanceRepository maintenanceRepository,
        IEquipmentRepository equipmentRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<MaintenanceCreateDto> createValidator,
        IValidator<MaintenanceCompleteDto> completeValidator,
        IValidator<MaintenanceRescheduleDto> rescheduleValidator,
        ILogger<MaintenanceService> logger)
    {
        _maintenanceRepository = maintenanceRepository;
        _equipmentRepository = equipmentRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _completeValidator = completeValidator;
        _rescheduleValidator = rescheduleValidator;
        _logger = logger;
    }

    public async Task<PagedResult<MaintenanceRecordDto>> GetMaintenanceRecordsAsync(MaintenanceFilterDto filter, PaginationDto pagination, CancellationToken cancellationToken = default)
    {
        IEnumerable<MaintenanceRecord> records;

        if (filter.Status.HasValue)
        {
            records = await _maintenanceRepository.GetByStatusAsync(filter.Status.Value, cancellationToken);
        }
        else if (filter.EquipmentId.HasValue)
        {
            records = await _maintenanceRepository.GetByEquipmentAsync(filter.EquipmentId.Value, cancellationToken);
        }
        else if (filter.FactoryId.HasValue)
        {
            records = await _maintenanceRepository.GetByFactoryAsync(filter.FactoryId.Value, cancellationToken);
        }
        else if (filter.DateFrom.HasValue && filter.DateTo.HasValue)
        {
            records = await _maintenanceRepository.GetByDateRangeAsync(filter.DateFrom.Value, filter.DateTo.Value, cancellationToken);
        }
        else
        {
            records = await _maintenanceRepository.GetAllAsync(cancellationToken);
        }

        var query = records.AsQueryable();

        if (filter.Type.HasValue)
            query = query.Where(m => m.Type == filter.Type.Value);

        if (!string.IsNullOrEmpty(filter.SearchText))
            query = query.Where(m =>
                m.Title.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase) ||
                (m.TechnicianName != null && m.TechnicianName.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase)));

        var totalCount = query.Count();
        var items = query
            .OrderByDescending(m => m.ScheduledDate)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToList();

        return PagedResult<MaintenanceRecordDto>.Create(
            _mapper.Map<IEnumerable<MaintenanceRecordDto>>(items),
            totalCount,
            pagination.PageNumber,
            pagination.PageSize);
    }

    public async Task<MaintenanceRecordDetailDto?> GetMaintenanceRecordByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _maintenanceRepository.GetWithEquipmentAsync(id, cancellationToken);
        return record != null ? _mapper.Map<MaintenanceRecordDetailDto>(record) : null;
    }

    public async Task<MaintenanceRecordDto> ScheduleMaintenanceAsync(MaintenanceCreateDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw Exceptions.ValidationException.FromFluentValidation(validationResult);

        // Verify equipment exists
        var equipment = await _equipmentRepository.GetByIdAsync(dto.EquipmentId, cancellationToken);
        if (equipment == null)
            throw NotFoundException.For<Equipment>(dto.EquipmentId);

        var record = new MaintenanceRecord(dto.EquipmentId, dto.Type, dto.Title, dto.ScheduledDate);

        if (!string.IsNullOrEmpty(dto.Description))
            record.SetDescription(dto.Description);

        if (!string.IsNullOrEmpty(dto.TechnicianId))
            record.AssignTechnician(dto.TechnicianId, dto.TechnicianName);

        if (dto.EstimatedCost.HasValue)
            record.SetEstimatedCost(dto.EstimatedCost.Value);

        await _maintenanceRepository.AddAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Scheduled maintenance {MaintenanceTitle} for equipment {EquipmentId} on {ScheduledDate}",
            dto.Title, dto.EquipmentId, dto.ScheduledDate);

        return _mapper.Map<MaintenanceRecordDto>(record);
    }

    public async Task StartMaintenanceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _maintenanceRepository.GetByIdAsync(id, cancellationToken);
        if (record == null)
            throw NotFoundException.For<MaintenanceRecord>(id);

        try
        {
            record.Start();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Started maintenance {MaintenanceId}", id);
        }
        catch (InvalidOperationException ex)
        {
            throw new OperationNotAllowedException("Start", ex.Message);
        }
    }

    public async Task CompleteMaintenanceAsync(Guid id, MaintenanceCompleteDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _completeValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw Exceptions.ValidationException.FromFluentValidation(validationResult);

        var record = await _maintenanceRepository.GetByIdAsync(id, cancellationToken);
        if (record == null)
            throw NotFoundException.For<MaintenanceRecord>(id);

        try
        {
            record.Complete(dto.ActualCost, dto.DowntimeMinutes, dto.Notes);

            if (!string.IsNullOrEmpty(dto.PartsUsed))
                record.RecordPartsUsed(dto.PartsUsed);

            // Update equipment's last maintenance date
            var equipment = await _equipmentRepository.GetByIdAsync(record.EquipmentId, cancellationToken);
            equipment?.RecordMaintenance();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Completed maintenance {MaintenanceId}", id);
        }
        catch (InvalidOperationException ex)
        {
            throw new OperationNotAllowedException("Complete", ex.Message);
        }
    }

    public async Task CancelMaintenanceAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        var record = await _maintenanceRepository.GetByIdAsync(id, cancellationToken);
        if (record == null)
            throw NotFoundException.For<MaintenanceRecord>(id);

        try
        {
            record.Cancel(reason);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Cancelled maintenance {MaintenanceId}: {Reason}", id, reason);
        }
        catch (InvalidOperationException ex)
        {
            throw new OperationNotAllowedException("Cancel", ex.Message);
        }
    }

    public async Task RescheduleMaintenanceAsync(Guid id, MaintenanceRescheduleDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _rescheduleValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw Exceptions.ValidationException.FromFluentValidation(validationResult);

        var record = await _maintenanceRepository.GetByIdAsync(id, cancellationToken);
        if (record == null)
            throw NotFoundException.For<MaintenanceRecord>(id);

        try
        {
            record.Reschedule(dto.NewScheduledDate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Rescheduled maintenance {MaintenanceId} to {NewDate}", id, dto.NewScheduledDate);
        }
        catch (InvalidOperationException ex)
        {
            throw new OperationNotAllowedException("Reschedule", ex.Message);
        }
    }

    public async Task<IEnumerable<MaintenanceDueAlertDto>> GetOverdueMaintenanceAsync(Guid? factoryId, CancellationToken cancellationToken = default)
    {
        var records = await _maintenanceRepository.GetOverdueAsync(cancellationToken);

        if (factoryId.HasValue)
        {
            var factoryRecords = await _maintenanceRepository.GetByFactoryAsync(factoryId.Value, cancellationToken);
            var factoryRecordIds = factoryRecords.Select(r => r.Id).ToHashSet();
            records = records.Where(r => factoryRecordIds.Contains(r.Id));
        }

        return records.Select(r => new MaintenanceDueAlertDto
        {
            MaintenanceRecordId = r.Id,
            EquipmentId = r.EquipmentId,
            EquipmentCode = r.Equipment?.Code ?? string.Empty,
            EquipmentName = r.Equipment?.Name ?? string.Empty,
            ProductionLineName = r.Equipment?.ProductionLine?.Name ?? string.Empty,
            Title = r.Title,
            Type = r.Type.ToString(),
            ScheduledDate = r.ScheduledDate,
            DueDate = r.ScheduledDate,
            DaysOverdue = (int)(DateTime.UtcNow - r.ScheduledDate).TotalDays,
            IsOverdue = DateTime.UtcNow > r.ScheduledDate,
            Severity = CalculateOverdueSeverity(r.ScheduledDate)
        });
    }

    public async Task<IEnumerable<MaintenanceDueAlertDto>> GetUpcomingMaintenanceAsync(int days, Guid? factoryId, CancellationToken cancellationToken = default)
    {
        var records = await _maintenanceRepository.GetUpcomingAsync(days, cancellationToken);

        if (factoryId.HasValue)
        {
            var factoryRecords = await _maintenanceRepository.GetByFactoryAsync(factoryId.Value, cancellationToken);
            var factoryRecordIds = factoryRecords.Select(r => r.Id).ToHashSet();
            records = records.Where(r => factoryRecordIds.Contains(r.Id));
        }

        return records.Select(r => new MaintenanceDueAlertDto
        {
            MaintenanceRecordId = r.Id,
            EquipmentId = r.EquipmentId,
            EquipmentCode = r.Equipment?.Code ?? string.Empty,
            EquipmentName = r.Equipment?.Name ?? string.Empty,
            ProductionLineName = r.Equipment?.ProductionLine?.Name ?? string.Empty,
            Title = r.Title,
            Type = r.Type.ToString(),
            ScheduledDate = r.ScheduledDate,
            DueDate = r.ScheduledDate,
            DaysOverdue = 0,
            IsOverdue = false,
            Severity = "Info"
        });
    }

    public async Task<MaintenanceSummaryDto> GetMaintenanceSummaryAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        IEnumerable<MaintenanceRecord> records;

        if (factoryId.HasValue)
        {
            records = await _maintenanceRepository.GetByFactoryAsync(factoryId.Value, cancellationToken);
            records = records.Where(r => r.ScheduledDate >= startDate && r.ScheduledDate <= endDate);
        }
        else
        {
            records = await _maintenanceRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        }

        var recordList = records.ToList();
        var completedRecords = recordList.Where(r => r.Status == MaintenanceStatus.Completed).ToList();

        return new MaintenanceSummaryDto
        {
            TotalScheduled = recordList.Count,
            TotalCompleted = completedRecords.Count,
            TotalOverdue = recordList.Count(r => r.Status == MaintenanceStatus.Overdue),
            TotalCancelled = recordList.Count(r => r.Status == MaintenanceStatus.Cancelled),
            TotalInProgress = recordList.Count(r => r.Status == MaintenanceStatus.InProgress),
            TotalDowntimeMinutes = completedRecords.Sum(r => r.DowntimeMinutes ?? 0),
            TotalActualCost = completedRecords.Sum(r => r.ActualCost ?? 0),
            PreventiveCount = recordList.Count(r => r.Type == MaintenanceType.Preventive),
            CorrectiveCount = recordList.Count(r => r.Type == MaintenanceType.Corrective),
            PredictiveCount = recordList.Count(r => r.Type == MaintenanceType.Predictive),
            CompletionRate = recordList.Count > 0
                ? Math.Round((double)completedRecords.Count / recordList.Count * 100, 2)
                : 0
        };
    }

    private static string CalculateOverdueSeverity(DateTime scheduledDate)
    {
        var daysOverdue = (DateTime.UtcNow - scheduledDate).TotalDays;
        return daysOverdue switch
        {
            > 14 => "Critical",
            > 7 => "High",
            > 3 => "Medium",
            _ => "Low"
        };
    }
}
