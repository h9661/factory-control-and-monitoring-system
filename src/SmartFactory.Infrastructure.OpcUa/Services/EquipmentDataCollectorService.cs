using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartFactory.Infrastructure.OpcUa.Configuration;
using SmartFactory.Infrastructure.OpcUa.Interfaces;
using SmartFactory.Infrastructure.OpcUa.Models;

namespace SmartFactory.Infrastructure.OpcUa.Services;

/// <summary>
/// Background service that collects equipment data from OPC-UA servers
/// and publishes sensor data events.
/// </summary>
public class EquipmentDataCollectorService : BackgroundService
{
    private readonly ILogger<EquipmentDataCollectorService> _logger;
    private readonly IOpcUaConnectionManager _connectionManager;
    private readonly OpcUaOptions _options;
    private readonly Dictionary<string, uint> _serverSubscriptions = new();
    private readonly object _lock = new();

    /// <summary>
    /// Event raised when sensor data is received from OPC-UA nodes.
    /// </summary>
    public event EventHandler<OpcUaNodeValue>? SensorDataReceived;

    /// <summary>
    /// Event raised when equipment status changes.
    /// </summary>
    public event EventHandler<EquipmentStatusChangedEventArgs>? EquipmentStatusChanged;

    public EquipmentDataCollectorService(
        IOpcUaConnectionManager connectionManager,
        IOptions<OpcUaOptions> options,
        ILogger<EquipmentDataCollectorService> logger)
    {
        _connectionManager = connectionManager;
        _options = options.Value;
        _logger = logger;

        _connectionManager.ConnectionStateChanged += OnConnectionStateChanged;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Equipment Data Collector Service starting");

        try
        {
            // Connect to all configured servers
            await _connectionManager.ConnectAllAsync(stoppingToken);

            // Subscribe to all configured nodes
            await SubscribeToAllServersAsync(stoppingToken);

            // Keep running until cancelled
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                // Log status periodically
                var connectedCount = _connectionManager.GetConnectedClients().Count();
                _logger.LogDebug("Equipment Data Collector running. Connected servers: {Count}", connectedCount);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Equipment Data Collector Service stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Equipment Data Collector Service encountered an error");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Equipment Data Collector Service stopping");

        // Unsubscribe from all servers
        await UnsubscribeFromAllServersAsync(cancellationToken);

        // Disconnect from all servers
        await _connectionManager.DisconnectAllAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }

    private async Task SubscribeToAllServersAsync(CancellationToken cancellationToken)
    {
        foreach (var serverConfig in _options.Servers.Where(s => s.AutoConnect && s.Nodes.Count > 0))
        {
            await SubscribeToServerAsync(serverConfig.Id, serverConfig.Nodes, cancellationToken);
        }
    }

    private async Task SubscribeToServerAsync(
        string serverId,
        IEnumerable<OpcUaNodeConfig> nodes,
        CancellationToken cancellationToken)
    {
        var client = _connectionManager.GetClient(serverId);
        if (client == null || !client.IsConnected)
        {
            _logger.LogWarning("Cannot subscribe to server {ServerId}: client not connected", serverId);
            return;
        }

        try
        {
            var subscriptionId = await client.SubscribeAsync(
                nodes,
                OnNodeValueChanged,
                cancellationToken);

            lock (_lock)
            {
                _serverSubscriptions[serverId] = subscriptionId;
            }

            _logger.LogInformation(
                "Created subscription {SubscriptionId} for server {ServerId} with {NodeCount} nodes",
                subscriptionId, serverId, nodes.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to server {ServerId}", serverId);
        }
    }

    private async Task UnsubscribeFromAllServersAsync(CancellationToken cancellationToken)
    {
        Dictionary<string, uint> subscriptionsToRemove;

        lock (_lock)
        {
            subscriptionsToRemove = new Dictionary<string, uint>(_serverSubscriptions);
            _serverSubscriptions.Clear();
        }

        foreach (var (serverId, subscriptionId) in subscriptionsToRemove)
        {
            var client = _connectionManager.GetClient(serverId);
            if (client == null) continue;

            try
            {
                await client.UnsubscribeAsync(subscriptionId, cancellationToken);
                _logger.LogInformation("Unsubscribed from server {ServerId}", serverId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unsubscribe from server {ServerId}", serverId);
            }
        }
    }

    private void OnNodeValueChanged(OpcUaNodeValue value)
    {
        _logger.LogDebug(
            "Received value for node {NodeId}: {Value} (Equipment: {EquipmentId}, Sensor: {SensorType})",
            value.NodeId, value.Value, value.EquipmentId, value.SensorType);

        // Raise sensor data event
        SensorDataReceived?.Invoke(this, value);

        // Check for equipment status changes based on sensor type
        if (value.EquipmentId.HasValue && !string.IsNullOrEmpty(value.SensorType))
        {
            CheckEquipmentStatus(value);
        }
    }

    private void CheckEquipmentStatus(OpcUaNodeValue value)
    {
        // Check for status-related sensors
        if (value.SensorType?.Equals("Status", StringComparison.OrdinalIgnoreCase) == true ||
            value.SensorType?.Equals("RunningState", StringComparison.OrdinalIgnoreCase) == true)
        {
            var isRunning = value.Value switch
            {
                bool b => b,
                int i => i > 0,
                string s => s.Equals("Running", StringComparison.OrdinalIgnoreCase) ||
                           s.Equals("On", StringComparison.OrdinalIgnoreCase) ||
                           s.Equals("1", StringComparison.Ordinal),
                _ => false
            };

            EquipmentStatusChanged?.Invoke(this, new EquipmentStatusChangedEventArgs
            {
                EquipmentId = value.EquipmentId!.Value,
                IsRunning = isRunning,
                Timestamp = value.SourceTimestamp,
                SensorType = value.SensorType,
                RawValue = value.Value
            });
        }

        // Check for alarm conditions
        CheckAlarmConditions(value);
    }

    private void CheckAlarmConditions(OpcUaNodeValue value)
    {
        if (!value.IsGood || value.Value == null) return;

        // Temperature alarm thresholds (example)
        if (value.SensorType?.Equals("Temperature", StringComparison.OrdinalIgnoreCase) == true)
        {
            if (value.Value is double temp && temp > 80)
            {
                _logger.LogWarning(
                    "High temperature alarm for equipment {EquipmentId}: {Temperature}°C",
                    value.EquipmentId, temp);
            }
        }

        // Vibration alarm thresholds (example)
        if (value.SensorType?.Equals("Vibration", StringComparison.OrdinalIgnoreCase) == true)
        {
            if (value.Value is double vibration && vibration > 10)
            {
                _logger.LogWarning(
                    "High vibration alarm for equipment {EquipmentId}: {Vibration}",
                    value.EquipmentId, vibration);
            }
        }

        // Pressure alarm thresholds (example)
        if (value.SensorType?.Equals("Pressure", StringComparison.OrdinalIgnoreCase) == true)
        {
            if (value.Value is double pressure && pressure > 100)
            {
                _logger.LogWarning(
                    "High pressure alarm for equipment {EquipmentId}: {Pressure}",
                    value.EquipmentId, pressure);
            }
        }
    }

    private async void OnConnectionStateChanged(object? sender, OpcUaConnectionStatus status)
    {
        if (status.IsConnected)
        {
            // Re-subscribe when reconnected
            var serverConfig = _options.Servers.FirstOrDefault(s => s.Id == status.ServerId);
            if (serverConfig != null && serverConfig.Nodes.Count > 0)
            {
                _logger.LogInformation("Server {ServerId} reconnected, re-subscribing to nodes", status.ServerId);
                await SubscribeToServerAsync(status.ServerId, serverConfig.Nodes, CancellationToken.None);
            }
        }
        else
        {
            // Remove subscription tracking when disconnected
            lock (_lock)
            {
                _serverSubscriptions.Remove(status.ServerId);
            }
        }
    }

    public override void Dispose()
    {
        _connectionManager.ConnectionStateChanged -= OnConnectionStateChanged;
        base.Dispose();
    }
}

/// <summary>
/// Event arguments for equipment status changes.
/// </summary>
public class EquipmentStatusChangedEventArgs : EventArgs
{
    public Guid EquipmentId { get; init; }
    public bool IsRunning { get; init; }
    public DateTime Timestamp { get; init; }
    public string? SensorType { get; init; }
    public object? RawValue { get; init; }
}
