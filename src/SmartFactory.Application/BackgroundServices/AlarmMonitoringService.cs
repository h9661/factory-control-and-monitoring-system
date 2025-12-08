using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartFactory.Application.DTOs.Alarm;
using SmartFactory.Application.Events;
using SmartFactory.Application.Interfaces;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Application.BackgroundServices;

/// <summary>
/// Background service that monitors alarms and publishes alarm-related events.
/// </summary>
public class AlarmMonitoringService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger<AlarmMonitoringService> _logger;
    private readonly PollingOptions _options;
    private readonly HashSet<Guid> _knownAlarmIds = new();
    private AlarmSummaryDto? _lastSummary;

    public AlarmMonitoringService(
        IServiceProvider serviceProvider,
        IEventAggregator eventAggregator,
        IOptions<PollingOptions> options,
        ILogger<AlarmMonitoringService> logger)
    {
        _serviceProvider = serviceProvider;
        _eventAggregator = eventAggregator;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Alarm Monitoring Service starting");

        // Initial load of known alarms
        await InitializeKnownAlarmsAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MonitorAlarmsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring alarms");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.AlarmMonitoringIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Alarm Monitoring Service stopped");
    }

    private async Task InitializeKnownAlarmsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var alarmService = scope.ServiceProvider.GetRequiredService<IAlarmService>();

            var alarms = await alarmService.GetActiveAlarmsAsync(null, cancellationToken);

            foreach (var alarm in alarms)
            {
                _knownAlarmIds.Add(alarm.Id);
            }

            _logger.LogInformation("Initialized with {Count} known active alarms", _knownAlarmIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize known alarms");
        }
    }

    private async Task MonitorAlarmsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var alarmService = scope.ServiceProvider.GetRequiredService<IAlarmService>();

        // Get current active alarms
        var currentAlarms = await alarmService.GetActiveAlarmsAsync(null, cancellationToken);

        // Check for new alarms
        foreach (var alarm in currentAlarms)
        {
            if (!_knownAlarmIds.Contains(alarm.Id))
            {
                _logger.LogInformation(
                    "New alarm detected: {AlarmCode} - {Message} (Severity: {Severity})",
                    alarm.AlarmCode, alarm.Message, alarm.Severity);

                await _eventAggregator.PublishAsync(new AlarmCreatedEvent
                {
                    AlarmId = alarm.Id,
                    AlarmCode = alarm.AlarmCode,
                    Severity = alarm.Severity,
                    Message = alarm.Message,
                    EquipmentId = alarm.EquipmentId,
                    EquipmentCode = alarm.EquipmentCode,
                    EquipmentName = alarm.EquipmentName
                }, cancellationToken);

                _knownAlarmIds.Add(alarm.Id);
            }
        }

        // Check for resolved alarms
        var currentAlarmIds = currentAlarms.Select(a => a.Id).ToHashSet();
        var resolvedIds = _knownAlarmIds.Where(id => !currentAlarmIds.Contains(id)).ToList();

        foreach (var resolvedId in resolvedIds)
        {
            _knownAlarmIds.Remove(resolvedId);
        }

        // Update alarm summary
        var summary = await alarmService.GetAlarmSummaryAsync(null, cancellationToken);

        if (HasSummaryChanged(summary))
        {
            _logger.LogDebug("Alarm summary changed: {ActiveCount} active, {CriticalCount} critical",
                summary.TotalActive, summary.CriticalCount);

            await _eventAggregator.PublishAsync(new AlarmSummaryChangedEvent
            {
                TotalActiveAlarms = summary.TotalActive,
                CriticalCount = summary.CriticalCount,
                ErrorCount = summary.ErrorCount,
                WarningCount = summary.WarningCount,
                UnacknowledgedCount = summary.UnacknowledgedCount
            }, cancellationToken);

            _lastSummary = summary;
        }
    }

    private bool HasSummaryChanged(AlarmSummaryDto current)
    {
        if (_lastSummary == null) return true;

        return current.TotalActive != _lastSummary.TotalActive
            || current.CriticalCount != _lastSummary.CriticalCount
            || current.ErrorCount != _lastSummary.ErrorCount
            || current.WarningCount != _lastSummary.WarningCount
            || current.UnacknowledgedCount != _lastSummary.UnacknowledgedCount;
    }
}
