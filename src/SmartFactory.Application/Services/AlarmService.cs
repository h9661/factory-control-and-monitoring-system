using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.DTOs.Alarm;
using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.Exceptions;
using SmartFactory.Application.Interfaces;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Domain.ValueObjects;

namespace SmartFactory.Application.Services;

/// <summary>
/// Service implementation for alarm operations.
/// </summary>
public class AlarmService : IAlarmService
{
    private readonly IAlarmRepository _alarmRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<AlarmCreateDto> _createValidator;
    private readonly IValidator<AlarmAcknowledgeDto> _acknowledgeValidator;
    private readonly IValidator<AlarmResolveDto> _resolveValidator;
    private readonly ILogger<AlarmService> _logger;

    public AlarmService(
        IAlarmRepository alarmRepository,
        IEquipmentRepository equipmentRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<AlarmCreateDto> createValidator,
        IValidator<AlarmAcknowledgeDto> acknowledgeValidator,
        IValidator<AlarmResolveDto> resolveValidator,
        ILogger<AlarmService> logger)
    {
        _alarmRepository = alarmRepository;
        _equipmentRepository = equipmentRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _acknowledgeValidator = acknowledgeValidator;
        _resolveValidator = resolveValidator;
        _logger = logger;
    }

    public async Task<PagedResult<AlarmDto>> GetAlarmsAsync(AlarmFilterDto filter, PaginationDto pagination, CancellationToken cancellationToken = default)
    {
        var alarms = filter.ActiveOnly == true
            ? await _alarmRepository.GetActiveAlarmsAsync(filter.FactoryId, cancellationToken)
            : await _alarmRepository.GetByDateRangeAsync(
                new DateTimeRange(
                    filter.DateFrom ?? DateTime.UtcNow.AddDays(-30),
                    filter.DateTo ?? DateTime.UtcNow),
                cancellationToken);

        // Apply additional filters
        var query = alarms.AsQueryable();

        if (filter.EquipmentId.HasValue)
            query = query.Where(a => a.EquipmentId == filter.EquipmentId.Value);

        if (filter.Severity.HasValue)
            query = query.Where(a => a.Severity == filter.Severity.Value);

        if (filter.Status.HasValue)
            query = query.Where(a => a.Status == filter.Status.Value);

        if (!string.IsNullOrEmpty(filter.SearchText))
            query = query.Where(a =>
                a.AlarmCode.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase) ||
                a.Message.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase));

        var totalCount = query.Count();
        var items = query
            .OrderByDescending(a => a.OccurredAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToList();

        return PagedResult<AlarmDto>.Create(
            _mapper.Map<IEnumerable<AlarmDto>>(items),
            totalCount,
            pagination.PageNumber,
            pagination.PageSize);
    }

    public async Task<AlarmDetailDto?> GetAlarmByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var alarm = await _alarmRepository.GetByIdAsync(id, cancellationToken);
        return alarm != null ? _mapper.Map<AlarmDetailDto>(alarm) : null;
    }

    public async Task<IEnumerable<AlarmDto>> GetActiveAlarmsAsync(Guid? factoryId, CancellationToken cancellationToken = default)
    {
        var alarms = await _alarmRepository.GetActiveAlarmsAsync(factoryId, cancellationToken);
        return _mapper.Map<IEnumerable<AlarmDto>>(alarms);
    }

    public async Task<IEnumerable<AlarmDto>> GetRecentAlarmsAsync(int count, Guid? factoryId, CancellationToken cancellationToken = default)
    {
        var alarms = await _alarmRepository.GetRecentAlarmsAsync(count, factoryId, cancellationToken);
        return _mapper.Map<IEnumerable<AlarmDto>>(alarms);
    }

    public async Task<AlarmDto> CreateAlarmAsync(AlarmCreateDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw Exceptions.ValidationException.FromFluentValidation(validationResult);

        // Verify equipment exists
        var equipment = await _equipmentRepository.GetByIdAsync(dto.EquipmentId, cancellationToken);
        if (equipment == null)
            throw NotFoundException.For<Equipment>(dto.EquipmentId);

        var alarm = new Alarm(dto.EquipmentId, dto.AlarmCode, dto.Severity, dto.Message, DateTime.UtcNow);
        if (!string.IsNullOrEmpty(dto.Description))
            alarm.SetDescription(dto.Description);

        await _alarmRepository.AddAsync(alarm, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created alarm {AlarmCode} for equipment {EquipmentId} with severity {Severity}",
            dto.AlarmCode, dto.EquipmentId, dto.Severity);

        return _mapper.Map<AlarmDto>(alarm);
    }

    public async Task AcknowledgeAlarmAsync(Guid id, AlarmAcknowledgeDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _acknowledgeValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw Exceptions.ValidationException.FromFluentValidation(validationResult);

        var alarm = await _alarmRepository.GetByIdAsync(id, cancellationToken);
        if (alarm == null)
            throw NotFoundException.For<Alarm>(id);

        try
        {
            alarm.Acknowledge(dto.UserId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Alarm {AlarmId} acknowledged by user {UserId}", id, dto.UserId);
        }
        catch (InvalidOperationException ex)
        {
            throw new OperationNotAllowedException("Acknowledge", ex.Message);
        }
    }

    public async Task AcknowledgeAlarmsAsync(IEnumerable<Guid> ids, string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new Exceptions.ValidationException("UserId", "User ID is required.");

        foreach (var id in ids)
        {
            var alarm = await _alarmRepository.GetByIdAsync(id, cancellationToken);
            if (alarm != null && alarm.IsActive)
            {
                alarm.Acknowledge(userId);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Acknowledged {Count} alarms by user {UserId}", ids.Count(), userId);
    }

    public async Task ResolveAlarmAsync(Guid id, AlarmResolveDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _resolveValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw Exceptions.ValidationException.FromFluentValidation(validationResult);

        var alarm = await _alarmRepository.GetByIdAsync(id, cancellationToken);
        if (alarm == null)
            throw NotFoundException.For<Alarm>(id);

        alarm.Resolve(dto.UserId, dto.ResolutionNotes);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Alarm {AlarmId} resolved by user {UserId}", id, dto.UserId);
    }

    public async Task<AlarmSummaryDto> GetAlarmSummaryAsync(Guid? factoryId, CancellationToken cancellationToken = default)
    {
        var summary = await _alarmRepository.GetAlarmSummaryAsync(factoryId, cancellationToken);
        return new AlarmSummaryDto
        {
            TotalActive = summary.TotalActive,
            CriticalCount = summary.CriticalCount,
            ErrorCount = summary.ErrorCount,
            WarningCount = summary.WarningCount,
            InformationCount = summary.InformationCount,
            UnacknowledgedCount = summary.UnacknowledgedCount,
            AcknowledgedCount = summary.AcknowledgedCount
        };
    }

    public async Task<int> GetActiveAlarmCountAsync(Guid? factoryId, CancellationToken cancellationToken = default)
    {
        return await _alarmRepository.GetActiveAlarmCountAsync(factoryId, cancellationToken);
    }
}
