using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartFactory.Application.DTOs.Maintenance;
using SmartFactory.Application.Events;
using SmartFactory.Application.Interfaces;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Application.BackgroundServices;

/// <summary>
/// Background service that checks for overdue maintenance and publishes alerts.
/// </summary>
public class MaintenanceCheckService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger<MaintenanceCheckService> _logger;
    private readonly PollingOptions _options;
    private readonly HashSet<Guid> _notifiedMaintenanceIds = new();

    public MaintenanceCheckService(
        IServiceProvider serviceProvider,
        IEventAggregator eventAggregator,
        IOptions<PollingOptions> options,
        ILogger<MaintenanceCheckService> logger)
    {
        _serviceProvider = serviceProvider;
        _eventAggregator = eventAggregator;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Maintenance Check Service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckMaintenanceDueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking maintenance due");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.MaintenanceCheckIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Maintenance Check Service stopped");
    }

    private async Task CheckMaintenanceDueAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var maintenanceService = scope.ServiceProvider.GetRequiredService<IMaintenanceService>();

        // Get overdue maintenance alerts
        var overdueAlerts = await maintenanceService.GetOverdueMaintenanceAsync(null, cancellationToken);
        // Also get upcoming maintenance (within 7 days)
        var upcomingAlerts = await maintenanceService.GetUpcomingMaintenanceAsync(7, null, cancellationToken);
        var alerts = overdueAlerts.Concat(upcomingAlerts);

        foreach (var alert in alerts)
        {
            var notificationKey = alert.MaintenanceRecordId ?? Guid.NewGuid();

            // Skip if we've already notified about this maintenance
            if (_notifiedMaintenanceIds.Contains(notificationKey))
                continue;

            _logger.LogWarning(
                "Maintenance due for equipment {EquipmentCode}: {Title} (Due: {DueDate}, Overdue: {IsOverdue})",
                alert.EquipmentCode, alert.Title, alert.DueDate, alert.IsOverdue);

            await _eventAggregator.PublishAsync(new MaintenanceDueEvent
            {
                MaintenanceRecordId = alert.MaintenanceRecordId,
                EquipmentId = alert.EquipmentId,
                EquipmentCode = alert.EquipmentCode,
                EquipmentName = alert.EquipmentName,
                MaintenanceType = Enum.TryParse<MaintenanceType>(alert.Type, out var type) ? type : MaintenanceType.Preventive,
                DueDate = alert.DueDate,
                DaysOverdue = alert.DaysOverdue,
                IsOverdue = alert.IsOverdue
            }, cancellationToken);

            _notifiedMaintenanceIds.Add(notificationKey);
        }

        // Clean up old notification tracking (keep last 1000)
        if (_notifiedMaintenanceIds.Count > 1000)
        {
            var toRemove = _notifiedMaintenanceIds.Take(500).ToList();
            foreach (var id in toRemove)
            {
                _notifiedMaintenanceIds.Remove(id);
            }
        }
    }
}
