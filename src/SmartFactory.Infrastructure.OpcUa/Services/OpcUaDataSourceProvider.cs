using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartFactory.Application.Interfaces;
using SmartFactory.Application.Services.DataSource;
using SmartFactory.Application.Services.Simulation;
using SmartFactory.Domain.Enums;
using SmartFactory.Infrastructure.OpcUa.Configuration;
using SmartFactory.Infrastructure.OpcUa.Interfaces;
using SmartFactory.Infrastructure.OpcUa.Models;

namespace SmartFactory.Infrastructure.OpcUa.Services;

/// <summary>
/// Data source provider that connects to OPC-UA servers for real equipment data.
/// Translates OPC-UA node values into application-level events.
/// </summary>
public class OpcUaDataSourceProvider : IDataSourceProvider
{
    private readonly ILogger<OpcUaDataSourceProvider> _logger;
    private readonly IOpcUaConnectionManager _connectionManager;
    private readonly OpcUaOptions _options;
    private readonly Dictionary<string, uint> _subscriptions = new();
    private bool _isConnected;

    public DataSourceMode Mode => DataSourceMode.OpcUa;
    public bool IsConnected => _isConnected;
    public string StatusMessage { get; private set; } = "Disconnected";

    public event EventHandler<SimulatedSensorDataEventArgs>? SensorDataReceived;
    public event EventHandler<SimulatedEquipmentStatusEventArgs>? EquipmentStatusChanged;
    public event EventHandler<SimulatedAlarmEventArgs>? AlarmReceived;
    public event EventHandler<SimulatedProductionEventArgs>? ProductionReceived;
    public event EventHandler<DataSourceConnectionEventArgs>? ConnectionStatusChanged;

    public OpcUaDataSourceProvider(
        ILogger<OpcUaDataSourceProvider> logger,
        IOpcUaConnectionManager connectionManager,
        IOptions<OpcUaOptions> options)
    {
        _logger = logger;
        _connectionManager = connectionManager;
        _options = options.Value;

        // Wire up connection manager events
        _connectionManager.ConnectionStateChanged += OnConnectionStateChanged;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting OPC-UA data source...");

        try
        {
            // Connect to all configured servers
            await _connectionManager.ConnectAllAsync(cancellationToken);

            // Subscribe to nodes on all connected clients
            foreach (var client in _connectionManager.GetConnectedClients())
            {
                await SubscribeToNodesAsync(client, cancellationToken);
            }

            _isConnected = _connectionManager.GetConnectedClients().Any();

            if (_isConnected)
            {
                StatusMessage = "Connected to OPC-UA servers";
                _logger.LogInformation("OPC-UA data source started successfully");
            }
            else
            {
                StatusMessage = "No OPC-UA servers connected";
                _logger.LogWarning("OPC-UA data source started but no servers are connected");
            }

            ConnectionStatusChanged?.Invoke(this, new DataSourceConnectionEventArgs
            {
                IsConnected = _isConnected,
                Mode = DataSourceMode.OpcUa,
                Message = StatusMessage,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _isConnected = false;
            StatusMessage = $"Failed to start: {ex.Message}";
            _logger.LogError(ex, "Failed to start OPC-UA data source");

            ConnectionStatusChanged?.Invoke(this, new DataSourceConnectionEventArgs
            {
                IsConnected = false,
                Mode = DataSourceMode.OpcUa,
                Message = StatusMessage,
                Timestamp = DateTime.UtcNow
            });

            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping OPC-UA data source...");

        try
        {
            // Unsubscribe from all nodes
            foreach (var client in _connectionManager.GetConnectedClients())
            {
                var serverId = client.ServerConfig.Id;
                if (_subscriptions.TryGetValue(serverId, out var subscriptionId))
                {
                    await client.UnsubscribeAsync(subscriptionId, cancellationToken);
                    _subscriptions.Remove(serverId);
                }
            }

            // Disconnect from all servers
            await _connectionManager.DisconnectAllAsync(cancellationToken);

            _isConnected = false;
            StatusMessage = "Disconnected";

            ConnectionStatusChanged?.Invoke(this, new DataSourceConnectionEventArgs
            {
                IsConnected = false,
                Mode = DataSourceMode.OpcUa,
                Message = "OPC-UA data source stopped",
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("OPC-UA data source stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping OPC-UA data source");
            throw;
        }
    }

    private async Task SubscribeToNodesAsync(IOpcUaClient client, CancellationToken cancellationToken)
    {
        var serverConfig = client.ServerConfig;
        if (serverConfig.Nodes.Count == 0)
        {
            _logger.LogDebug("No nodes configured for server {ServerId}", serverConfig.Id);
            return;
        }

        try
        {
            var subscriptionId = await client.SubscribeAsync(
                serverConfig.Nodes,
                OnValueReceived,
                cancellationToken);

            _subscriptions[serverConfig.Id] = subscriptionId;
            _logger.LogInformation(
                "Subscribed to {NodeCount} nodes on server {ServerId}",
                serverConfig.Nodes.Count,
                serverConfig.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to nodes on server {ServerId}", serverConfig.Id);
        }
    }

    private void OnValueReceived(OpcUaNodeValue nodeValue)
    {
        if (!nodeValue.IsGood)
        {
            _logger.LogDebug(
                "Received bad quality value for node {NodeId}: StatusCode={StatusCode}",
                nodeValue.NodeId,
                nodeValue.StatusCode);
            return;
        }

        // Route the value based on sensor type
        var sensorType = nodeValue.SensorType?.ToUpperInvariant();

        switch (sensorType)
        {
            case "TEMPERATURE":
            case "PRESSURE":
            case "VIBRATION":
            case "HUMIDITY":
            case "SPEED":
            case "POWER":
                RaiseSensorDataEvent(nodeValue);
                break;

            case "STATUS":
            case "EQUIPMENT_STATUS":
                RaiseEquipmentStatusEvent(nodeValue);
                break;

            case "ALARM":
            case "ALERT":
                RaiseAlarmEvent(nodeValue);
                break;

            case "PRODUCTION":
            case "OUTPUT":
            case "COUNT":
                RaiseProductionEvent(nodeValue);
                break;

            default:
                // Default to sensor data for numeric values
                if (nodeValue.Value is double or float or int or long)
                {
                    RaiseSensorDataEvent(nodeValue);
                }
                else
                {
                    _logger.LogDebug(
                        "Unhandled sensor type '{SensorType}' for node {NodeId}",
                        sensorType,
                        nodeValue.NodeId);
                }
                break;
        }
    }

    private void RaiseSensorDataEvent(OpcUaNodeValue nodeValue)
    {
        if (nodeValue.EquipmentId == null)
        {
            _logger.LogDebug("Sensor data received without equipment ID for node {NodeId}", nodeValue.NodeId);
            return;
        }

        var value = Convert.ToDouble(nodeValue.Value);

        SensorDataReceived?.Invoke(this, new SimulatedSensorDataEventArgs
        {
            EquipmentId = nodeValue.EquipmentId.Value,
            EquipmentCode = nodeValue.DisplayName,
            TagName = nodeValue.SensorType ?? nodeValue.DisplayName,
            Value = value,
            Unit = nodeValue.Unit ?? string.Empty,
            Timestamp = nodeValue.SourceTimestamp,
            IsAnomaly = false // OPC-UA doesn't provide anomaly detection
        });
    }

    private void RaiseEquipmentStatusEvent(OpcUaNodeValue nodeValue)
    {
        if (nodeValue.EquipmentId == null)
        {
            _logger.LogDebug("Equipment status received without equipment ID for node {NodeId}", nodeValue.NodeId);
            return;
        }

        var status = ParseEquipmentStatus(nodeValue.Value);

        EquipmentStatusChanged?.Invoke(this, new SimulatedEquipmentStatusEventArgs
        {
            EquipmentId = nodeValue.EquipmentId.Value,
            EquipmentCode = nodeValue.DisplayName,
            EquipmentName = nodeValue.DisplayName,
            PreviousStatus = EquipmentStatus.Idle, // OPC-UA doesn't track previous state
            NewStatus = status,
            Timestamp = nodeValue.SourceTimestamp
        });
    }

    private void RaiseAlarmEvent(OpcUaNodeValue nodeValue)
    {
        if (nodeValue.EquipmentId == null)
        {
            _logger.LogDebug("Alarm received without equipment ID for node {NodeId}", nodeValue.NodeId);
            return;
        }

        var message = nodeValue.Value?.ToString() ?? "Unknown alarm";
        var severity = DetermineAlarmSeverity(message);

        AlarmReceived?.Invoke(this, new SimulatedAlarmEventArgs
        {
            EquipmentId = nodeValue.EquipmentId.Value,
            EquipmentCode = nodeValue.DisplayName,
            AlarmCode = nodeValue.NodeId,
            Severity = severity,
            Message = message,
            Timestamp = nodeValue.SourceTimestamp
        });
    }

    private void RaiseProductionEvent(OpcUaNodeValue nodeValue)
    {
        if (nodeValue.EquipmentId == null)
        {
            _logger.LogDebug("Production data received without equipment ID for node {NodeId}", nodeValue.NodeId);
            return;
        }

        var count = Convert.ToInt32(nodeValue.Value);

        ProductionReceived?.Invoke(this, new SimulatedProductionEventArgs
        {
            EquipmentId = nodeValue.EquipmentId.Value,
            EquipmentCode = nodeValue.DisplayName,
            UnitsProduced = count,
            DefectCount = 0, // OPC-UA typically reports production count only
            Timestamp = nodeValue.SourceTimestamp
        });
    }

    private static EquipmentStatus ParseEquipmentStatus(object? value)
    {
        if (value == null) return EquipmentStatus.Offline;

        // Handle numeric status codes
        if (value is int or long or short or byte)
        {
            var code = Convert.ToInt32(value);
            return code switch
            {
                0 => EquipmentStatus.Offline,
                1 => EquipmentStatus.Idle,
                2 => EquipmentStatus.Running,
                3 => EquipmentStatus.Warning,
                4 => EquipmentStatus.Error,
                5 => EquipmentStatus.Maintenance,
                6 => EquipmentStatus.Setup,
                _ => EquipmentStatus.Idle
            };
        }

        // Handle string status
        var statusString = value.ToString()?.ToUpperInvariant();
        return statusString switch
        {
            "RUNNING" or "RUN" or "ACTIVE" => EquipmentStatus.Running,
            "IDLE" or "STANDBY" or "READY" => EquipmentStatus.Idle,
            "WARNING" or "WARN" => EquipmentStatus.Warning,
            "ERROR" or "FAULT" or "FAILURE" => EquipmentStatus.Error,
            "MAINTENANCE" or "MAINT" => EquipmentStatus.Maintenance,
            "SETUP" or "CHANGEOVER" => EquipmentStatus.Setup,
            "OFFLINE" or "OFF" or "STOPPED" => EquipmentStatus.Offline,
            _ => EquipmentStatus.Idle
        };
    }

    private static AlarmSeverity DetermineAlarmSeverity(string message)
    {
        var upperMessage = message.ToUpperInvariant();

        if (upperMessage.Contains("CRITICAL") || upperMessage.Contains("EMERGENCY"))
            return AlarmSeverity.Critical;

        if (upperMessage.Contains("ERROR") || upperMessage.Contains("FAULT") || upperMessage.Contains("FAILURE"))
            return AlarmSeverity.Error;

        if (upperMessage.Contains("WARNING") || upperMessage.Contains("WARN"))
            return AlarmSeverity.Warning;

        return AlarmSeverity.Information;
    }

    private void OnConnectionStateChanged(object? sender, OpcUaConnectionStatus status)
    {
        _logger.LogInformation(
            "OPC-UA connection state changed: Server={ServerId}, Connected={IsConnected}",
            status.ServerId,
            status.IsConnected);

        // Update overall connection state
        _isConnected = _connectionManager.GetConnectedClients().Any();
        StatusMessage = _isConnected
            ? "Connected to OPC-UA servers"
            : "No OPC-UA servers connected";

        ConnectionStatusChanged?.Invoke(this, new DataSourceConnectionEventArgs
        {
            IsConnected = _isConnected,
            Mode = DataSourceMode.OpcUa,
            Message = $"Server {status.ServerName}: {(status.IsConnected ? "Connected" : "Disconnected")}",
            Timestamp = DateTime.UtcNow
        });
    }
}
