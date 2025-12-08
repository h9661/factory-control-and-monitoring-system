using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.DTOs.Equipment;
using SmartFactory.Application.Exceptions;
using SmartFactory.Application.Interfaces;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;

namespace SmartFactory.Application.Services;

/// <summary>
/// Service implementation for equipment operations.
/// </summary>
public class EquipmentService : IEquipmentService
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IProductionLineRepository _productionLineRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<EquipmentCreateDto> _createValidator;
    private readonly IValidator<EquipmentUpdateDto> _updateValidator;
    private readonly ILogger<EquipmentService> _logger;

    public EquipmentService(
        IEquipmentRepository equipmentRepository,
        IProductionLineRepository productionLineRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<EquipmentCreateDto> createValidator,
        IValidator<EquipmentUpdateDto> updateValidator,
        ILogger<EquipmentService> logger)
    {
        _equipmentRepository = equipmentRepository;
        _productionLineRepository = productionLineRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<EquipmentDto>> GetEquipmentAsync(EquipmentFilterDto filter, PaginationDto pagination, CancellationToken cancellationToken = default)
    {
        IEnumerable<Equipment> equipment;

        if (filter.Status.HasValue)
        {
            equipment = await _equipmentRepository.GetByStatusAsync(filter.Status.Value, cancellationToken);
        }
        else if (filter.ProductionLineId.HasValue)
        {
            equipment = await _equipmentRepository.GetByProductionLineAsync(filter.ProductionLineId.Value, cancellationToken);
        }
        else if (filter.FactoryId.HasValue)
        {
            equipment = await _equipmentRepository.GetByFactoryAsync(filter.FactoryId.Value, cancellationToken);
        }
        else
        {
            equipment = await _equipmentRepository.GetAllAsync(cancellationToken);
        }

        var query = equipment.AsQueryable();

        if (filter.Type.HasValue)
            query = query.Where(e => e.Type == filter.Type.Value);

        if (filter.IsActive.HasValue)
            query = query.Where(e => e.IsActive == filter.IsActive.Value);

        if (!string.IsNullOrEmpty(filter.SearchText))
            query = query.Where(e =>
                e.Code.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase) ||
                e.Name.Contains(filter.SearchText, StringComparison.OrdinalIgnoreCase));

        var totalCount = query.Count();
        var items = query
            .OrderBy(e => e.Code)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToList();

        return PagedResult<EquipmentDto>.Create(
            _mapper.Map<IEnumerable<EquipmentDto>>(items),
            totalCount,
            pagination.PageNumber,
            pagination.PageSize);
    }

    public async Task<EquipmentDetailDto?> GetEquipmentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetWithDetailsAsync(id, cancellationToken);
        return equipment != null ? _mapper.Map<EquipmentDetailDto>(equipment) : null;
    }

    public async Task<EquipmentDto> CreateEquipmentAsync(EquipmentCreateDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw Exceptions.ValidationException.FromFluentValidation(validationResult);

        // Verify production line exists
        var productionLine = await _productionLineRepository.GetByIdAsync(dto.ProductionLineId, cancellationToken);
        if (productionLine == null)
            throw NotFoundException.For<ProductionLine>(dto.ProductionLineId);

        var equipment = new Equipment(dto.ProductionLineId, dto.Code, dto.Name, dto.Type);

        if (!string.IsNullOrEmpty(dto.Description))
            equipment.Update(dto.Name, dto.Description, dto.Type);

        if (!string.IsNullOrEmpty(dto.OpcNodeId))
            equipment.SetOpcConfiguration(dto.OpcNodeId);

        if (!string.IsNullOrEmpty(dto.IpAddress))
            equipment.SetNetworkConfiguration(dto.IpAddress);

        if (!string.IsNullOrEmpty(dto.Manufacturer) || !string.IsNullOrEmpty(dto.Model))
            equipment.SetManufacturerInfo(dto.Manufacturer, dto.Model, dto.SerialNumber);

        if (dto.InstallationDate.HasValue)
            equipment.SetInstallationDate(dto.InstallationDate);

        if (dto.MaintenanceIntervalDays.HasValue)
            equipment.SetMaintenanceSchedule(dto.MaintenanceIntervalDays);

        await _equipmentRepository.AddAsync(equipment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created equipment {EquipmentCode} for production line {ProductionLineId}",
            dto.Code, dto.ProductionLineId);

        return _mapper.Map<EquipmentDto>(equipment);
    }

    public async Task UpdateEquipmentAsync(Guid id, EquipmentUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw Exceptions.ValidationException.FromFluentValidation(validationResult);

        var equipment = await _equipmentRepository.GetByIdAsync(id, cancellationToken);
        if (equipment == null)
            throw NotFoundException.For<Equipment>(id);

        equipment.Update(dto.Name, dto.Description, dto.Type);

        equipment.SetOpcConfiguration(dto.OpcNodeId);
        equipment.SetNetworkConfiguration(dto.IpAddress);
        equipment.SetManufacturerInfo(dto.Manufacturer, dto.Model, dto.SerialNumber);
        equipment.SetInstallationDate(dto.InstallationDate);
        equipment.SetMaintenanceSchedule(dto.MaintenanceIntervalDays);

        if (dto.IsActive)
            equipment.Activate();
        else
            equipment.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated equipment {EquipmentId}", id);
    }

    public async Task UpdateEquipmentStatusAsync(Guid id, EquipmentStatus status, CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id, cancellationToken);
        if (equipment == null)
            throw NotFoundException.For<Equipment>(id);

        equipment.UpdateStatus(status);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated equipment {EquipmentId} status to {Status}", id, status);
    }

    public async Task DeleteEquipmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id, cancellationToken);
        if (equipment == null)
            throw NotFoundException.For<Equipment>(id);

        // Soft delete
        equipment.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deactivated equipment {EquipmentId}", id);
    }

    public async Task<EquipmentStatusSummaryDto> GetStatusSummaryAsync(Guid? factoryId, CancellationToken cancellationToken = default)
    {
        var summary = await _equipmentRepository.GetStatusSummaryAsync(factoryId, cancellationToken);
        return new EquipmentStatusSummaryDto
        {
            TotalCount = summary.TotalCount,
            RunningCount = summary.RunningCount,
            IdleCount = summary.IdleCount,
            WarningCount = summary.WarningCount,
            ErrorCount = summary.ErrorCount,
            MaintenanceCount = summary.MaintenanceCount,
            OfflineCount = summary.OfflineCount
        };
    }

    public async Task<IEnumerable<EquipmentDto>> GetEquipmentDueForMaintenanceAsync(Guid? factoryId, CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetEquipmentDueForMaintenanceAsync(cancellationToken);

        if (factoryId.HasValue)
        {
            // Filter by factory through production lines
            var productionLines = await _productionLineRepository.GetByFactoryAsync(factoryId.Value, cancellationToken);
            var lineIds = productionLines.Select(pl => pl.Id).ToHashSet();
            equipment = equipment.Where(e => lineIds.Contains(e.ProductionLineId));
        }

        return _mapper.Map<IEnumerable<EquipmentDto>>(equipment);
    }

    public async Task RecordHeartbeatAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(id, cancellationToken);
        if (equipment == null)
            throw NotFoundException.For<Equipment>(id);

        equipment.RecordHeartbeat();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
