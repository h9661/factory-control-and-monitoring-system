using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.DTOs.Equipment;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Application.Interfaces;

/// <summary>
/// Service interface for equipment operations.
/// </summary>
public interface IEquipmentService
{
    /// <summary>
    /// Gets a paginated list of equipment.
    /// </summary>
    Task<PagedResult<EquipmentDto>> GetEquipmentAsync(EquipmentFilterDto filter, PaginationDto pagination, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets equipment by ID.
    /// </summary>
    Task<EquipmentDetailDto?> GetEquipmentByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates new equipment.
    /// </summary>
    Task<EquipmentDto> CreateEquipmentAsync(EquipmentCreateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates existing equipment.
    /// </summary>
    Task UpdateEquipmentAsync(Guid id, EquipmentUpdateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates equipment status.
    /// </summary>
    Task UpdateEquipmentStatusAsync(Guid id, EquipmentStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes equipment.
    /// </summary>
    Task DeleteEquipmentAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets equipment status summary.
    /// </summary>
    Task<EquipmentStatusSummaryDto> GetStatusSummaryAsync(Guid? factoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets equipment due for maintenance.
    /// </summary>
    Task<IEnumerable<EquipmentDto>> GetEquipmentDueForMaintenanceAsync(Guid? factoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records equipment heartbeat.
    /// </summary>
    Task RecordHeartbeatAsync(Guid id, CancellationToken cancellationToken = default);
}
