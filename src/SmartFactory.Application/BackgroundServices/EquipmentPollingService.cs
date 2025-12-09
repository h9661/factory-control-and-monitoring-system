using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartFactory.Application.Events;
using SmartFactory.Application.Interfaces;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Application.BackgroundServices;

/// <summary>
/// Background service that polls equipment status and publishes status change events.
/// </summary>
public class EquipmentPollingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger<EquipmentPollingService> _logger;
    private readonly PollingOptions _options;
    private readonly ConcurrentDictionary<Guid, EquipmentStatus> _lastKnownStatus = new();

    public EquipmentPollingService(
        IServiceProvider serviceProvider,
        IEventAggregator eventAggregator,
        IOptions<PollingOptions> options,
        ILogger<EquipmentPollingService> logger)
    {
        _serviceProvider = serviceProvider;
        _eventAggregator = eventAggregator;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Equipment Polling Service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollEquipmentStatusAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling equipment status");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.EquipmentPollingIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Equipment Polling Service stopped");
    }

    private async Task PollEquipmentStatusAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var equipmentService = scope.ServiceProvider.GetRequiredService<IEquipmentService>();

        // Get all equipment using the paginated API
        var filter = new DTOs.Equipment.EquipmentFilterDto();
        var pagination = new DTOs.Common.PaginationDto { PageNumber = 1, PageSize = 1000 };
        var result = await equipmentService.GetEquipmentAsync(filter, pagination, cancellationToken);

        foreach (var equip in result.Items)
        {
            var previousStatus = _lastKnownStatus.GetOrAdd(equip.Id, equip.Status);

            if (previousStatus != equip.Status)
            {
                _logger.LogInformation(
                    "Equipment {EquipmentCode} status changed from {PreviousStatus} to {CurrentStatus}",
                    equip.Code, previousStatus, equip.Status);

                await _eventAggregator.PublishAsync(new EquipmentStatusUpdatedEvent
                {
                    EquipmentId = equip.Id,
                    EquipmentCode = equip.Code,
                    EquipmentName = equip.Name,
                    PreviousStatus = previousStatus,
                    CurrentStatus = equip.Status,
                    ProductionLineId = equip.ProductionLineId,
                    ProductionLineName = equip.ProductionLineName
                }, cancellationToken);
            }

            _lastKnownStatus[equip.Id] = equip.Status;
        }
    }
}

/// <summary>
/// Configuration options for polling services.
/// </summary>
public class PollingOptions
{
    public const string SectionName = "Polling";

    /// <summary>
    /// Equipment polling interval in seconds. Default: 30
    /// </summary>
    public int EquipmentPollingIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Alarm monitoring interval in seconds. Default: 10
    /// </summary>
    public int AlarmMonitoringIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Maintenance check interval in seconds. Default: 300 (5 minutes)
    /// </summary>
    public int MaintenanceCheckIntervalSeconds { get; set; } = 300;

    /// <summary>
    /// Production summary update interval in seconds. Default: 60
    /// </summary>
    public int ProductionSummaryIntervalSeconds { get; set; } = 60;
}
