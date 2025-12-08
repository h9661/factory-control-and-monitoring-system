using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.DTOs.Quality;
using SmartFactory.Application.Exceptions;
using SmartFactory.Application.Interfaces;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Domain.ValueObjects;

namespace SmartFactory.Application.Services;

/// <summary>
/// Service implementation for quality operations.
/// </summary>
public class QualityService : IQualityService
{
    private readonly IQualityRecordRepository _qualityRecordRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<QualityRecordCreateDto> _createValidator;
    private readonly ILogger<QualityService> _logger;

    public QualityService(
        IQualityRecordRepository qualityRecordRepository,
        IEquipmentRepository equipmentRepository,
        IWorkOrderRepository workOrderRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<QualityRecordCreateDto> createValidator,
        ILogger<QualityService> logger)
    {
        _qualityRecordRepository = qualityRecordRepository;
        _equipmentRepository = equipmentRepository;
        _workOrderRepository = workOrderRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _logger = logger;
    }

    public async Task<PagedResult<QualityRecordDto>> GetQualityRecordsAsync(QualityFilterDto filter, PaginationDto pagination, CancellationToken cancellationToken = default)
    {
        IEnumerable<QualityRecord> records;

        if (filter.EquipmentId.HasValue)
        {
            records = await _qualityRecordRepository.GetByEquipmentAsync(filter.EquipmentId.Value, cancellationToken);
        }
        else if (filter.WorkOrderId.HasValue)
        {
            records = await _qualityRecordRepository.GetByWorkOrderAsync(filter.WorkOrderId.Value, cancellationToken);
        }
        else if (filter.FactoryId.HasValue)
        {
            records = await _qualityRecordRepository.GetByFactoryAsync(filter.FactoryId.Value, cancellationToken);
        }
        else if (filter.DateFrom.HasValue && filter.DateTo.HasValue)
        {
            records = await _qualityRecordRepository.GetByDateRangeAsync(filter.DateFrom.Value, filter.DateTo.Value, cancellationToken);
        }
        else
        {
            records = await _qualityRecordRepository.GetAllAsync(cancellationToken);
        }

        var query = records.AsQueryable();

        if (filter.InspectionType.HasValue)
            query = query.Where(q => q.InspectionType == filter.InspectionType.Value);

        if (filter.Result.HasValue)
            query = query.Where(q => q.Result == filter.Result.Value);

        if (filter.DefectType.HasValue)
            query = query.Where(q => q.DefectType == filter.DefectType.Value);

        if (!string.IsNullOrEmpty(filter.SearchText))
            query = query.Where(q =>
                (q.InspectorName != null && q.InspectorName.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase)) ||
                (q.DefectDescription != null && q.DefectDescription.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase)));

        var totalCount = query.Count();
        var items = query
            .OrderByDescending(q => q.InspectedAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToList();

        return PagedResult<QualityRecordDto>.Create(
            _mapper.Map<IEnumerable<QualityRecordDto>>(items),
            totalCount,
            pagination.PageNumber,
            pagination.PageSize);
    }

    public async Task<QualityRecordDetailDto?> GetQualityRecordByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _qualityRecordRepository.GetWithDetailsAsync(id, cancellationToken);
        return record != null ? _mapper.Map<QualityRecordDetailDto>(record) : null;
    }

    public async Task<QualityRecordDto> RecordInspectionAsync(QualityRecordCreateDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw Exceptions.ValidationException.FromFluentValidation(validationResult);

        // Verify equipment exists
        var equipment = await _equipmentRepository.GetByIdAsync(dto.EquipmentId, cancellationToken);
        if (equipment == null)
            throw NotFoundException.For<Equipment>(dto.EquipmentId);

        // Verify work order if provided
        if (dto.WorkOrderId.HasValue)
        {
            var workOrder = await _workOrderRepository.GetByIdAsync(dto.WorkOrderId.Value, cancellationToken);
            if (workOrder == null)
                throw NotFoundException.For<WorkOrder>(dto.WorkOrderId.Value);
        }

        var record = new QualityRecord(dto.EquipmentId, dto.InspectionType, dto.Result, DateTime.UtcNow);

        if (dto.WorkOrderId.HasValue)
            record.LinkToWorkOrder(dto.WorkOrderId.Value);

        if (!string.IsNullOrEmpty(dto.InspectorId))
            record.SetInspector(dto.InspectorId, dto.InspectorName);

        if (dto.DefectType.HasValue)
            record.RecordDefect(dto.DefectType.Value, dto.DefectDescription, dto.DefectCount ?? 1);

        if (dto.SampleSize.HasValue)
            record.SetSampleInfo(dto.SampleSize.Value, dto.DefectCount ?? 0);

        if (!string.IsNullOrEmpty(dto.Notes))
            record.AddNotes(dto.Notes);

        await _qualityRecordRepository.AddAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recorded quality inspection for equipment {EquipmentId} with result {Result}",
            dto.EquipmentId, dto.Result);

        return _mapper.Map<QualityRecordDto>(record);
    }

    public async Task<DefectSummaryDto> GetDefectSummaryAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var statistics = await _qualityRecordRepository.GetDefectStatisticsAsync(factoryId, startDate, endDate, cancellationToken);

        return new DefectSummaryDto
        {
            TotalInspections = statistics.TotalInspections,
            PassCount = statistics.PassCount,
            FailCount = statistics.FailCount,
            TotalDefects = statistics.TotalDefects,
            PassRate = Math.Round(statistics.PassRate, 2),
            OverallDefectRate = statistics.TotalInspections > 0
                ? Math.Round((double)statistics.TotalDefects / statistics.TotalInspections * 100, 2)
                : 0,
            DefectsByType = statistics.DefectsByType.Select(kvp => new DefectTypeCountDto
            {
                DefectType = kvp.Key,
                DefectTypeName = kvp.Key.ToString(),
                Count = kvp.Value,
                Percentage = statistics.TotalDefects > 0
                    ? Math.Round((double)kvp.Value / statistics.TotalDefects * 100, 2)
                    : 0
            })
        };
    }

    public async Task<IEnumerable<QualityTrendDto>> GetQualityTrendsAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        IEnumerable<QualityRecord> records;

        if (factoryId.HasValue)
        {
            records = await _qualityRecordRepository.GetByFactoryAsync(factoryId.Value, cancellationToken);
            records = records.Where(r => r.InspectedAt >= startDate && r.InspectedAt <= endDate);
        }
        else
        {
            records = await _qualityRecordRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        }

        var trends = records
            .GroupBy(r => r.InspectedAt.Date)
            .Select(g => new QualityTrendDto
            {
                Date = g.Key,
                TotalInspections = g.Count(),
                PassCount = g.Count(r => r.Result == InspectionResult.Pass),
                FailCount = g.Count(r => r.Result == InspectionResult.Fail),
                DefectCount = g.Sum(r => r.DefectCount ?? 0),
                PassRate = g.Count() > 0
                    ? Math.Round((double)g.Count(r => r.Result == InspectionResult.Pass) / g.Count() * 100, 2)
                    : 0
            })
            .OrderBy(t => t.Date)
            .ToList();

        return trends;
    }

    public async Task<double> CalculateYieldRateAsync(Guid? factoryId, DateTime date, CancellationToken cancellationToken = default)
    {
        var startDate = date.Date;
        var endDate = date.Date.AddDays(1);

        IEnumerable<QualityRecord> records;

        if (factoryId.HasValue)
        {
            records = await _qualityRecordRepository.GetByFactoryAsync(factoryId.Value, cancellationToken);
            records = records.Where(r => r.InspectedAt >= startDate && r.InspectedAt < endDate);
        }
        else
        {
            records = await _qualityRecordRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        }

        var recordList = records.ToList();
        if (recordList.Count == 0)
            return 0;

        var passCount = recordList.Count(r => r.Result == InspectionResult.Pass);
        return Math.Round((double)passCount / recordList.Count * 100, 2);
    }
}
