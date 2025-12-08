using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartFactory.Infrastructure.OpcUa.Configuration;
using SmartFactory.Infrastructure.OpcUa.Interfaces;
using SmartFactory.Infrastructure.OpcUa.Models;

namespace SmartFactory.Infrastructure.OpcUa.Services;

/// <summary>
/// Manages connections to multiple OPC-UA servers.
/// </summary>
public class OpcUaConnectionManager : IOpcUaConnectionManager, IDisposable
{
    private readonly ILogger<OpcUaConnectionManager> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly OpcUaOptions _options;
    private readonly Dictionary<string, OpcUaClient> _clients = new();
    private readonly Dictionary<string, OpcUaConnectionStatus> _connectionStatuses = new();
    private readonly object _lock = new();
    private bool _disposed;

    public event EventHandler<OpcUaConnectionStatus>? ConnectionStateChanged;

    public OpcUaConnectionManager(
        IOptions<OpcUaOptions> options,
        ILoggerFactory loggerFactory)
    {
        _options = options.Value;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<OpcUaConnectionManager>();

        InitializeClients();
    }

    private void InitializeClients()
    {
        foreach (var serverConfig in _options.Servers)
        {
            var client = new OpcUaClient(
                serverConfig,
                _options,
                _loggerFactory.CreateLogger<OpcUaClient>());

            client.ConnectionStateChanged += OnClientConnectionStateChanged;

            lock (_lock)
            {
                _clients[serverConfig.Id] = client;
                _connectionStatuses[serverConfig.Id] = new OpcUaConnectionStatus
                {
                    ServerId = serverConfig.Id,
                    ServerName = serverConfig.Name,
                    EndpointUrl = serverConfig.EndpointUrl,
                    IsConnected = false,
                    State = "Disconnected"
                };
            }

            _logger.LogInformation("Initialized OPC-UA client for server {ServerId}", serverConfig.Id);
        }
    }

    private void OnClientConnectionStateChanged(object? sender, bool isConnected)
    {
        if (sender is not OpcUaClient client) return;

        var serverId = client.ServerConfig.Id;
        OpcUaConnectionStatus status;

        lock (_lock)
        {
            if (!_connectionStatuses.TryGetValue(serverId, out var existingStatus))
                return;

            status = existingStatus with
            {
                IsConnected = isConnected,
                State = isConnected ? "Connected" : "Disconnected",
                LastConnectedAt = isConnected ? DateTime.UtcNow : existingStatus.LastConnectedAt,
                LastDisconnectedAt = !isConnected ? DateTime.UtcNow : existingStatus.LastDisconnectedAt
            };

            _connectionStatuses[serverId] = status;
        }

        _logger.LogInformation("Server {ServerId} connection state changed to {IsConnected}",
            serverId, isConnected);

        ConnectionStateChanged?.Invoke(this, status);
    }

    public async Task ConnectAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Connecting to all OPC-UA servers with auto-connect enabled");

        var connectTasks = new List<Task>();

        foreach (var serverConfig in _options.Servers.Where(s => s.AutoConnect))
        {
            connectTasks.Add(ConnectAsync(serverConfig.Id, cancellationToken));
        }

        await Task.WhenAll(connectTasks);

        _logger.LogInformation("Completed connecting to all auto-connect servers");
    }

    public async Task DisconnectAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Disconnecting from all OPC-UA servers");

        var disconnectTasks = new List<Task>();

        lock (_lock)
        {
            foreach (var client in _clients.Values)
            {
                disconnectTasks.Add(client.DisconnectAsync(cancellationToken));
            }
        }

        await Task.WhenAll(disconnectTasks);

        _logger.LogInformation("Disconnected from all OPC-UA servers");
    }

    public async Task ConnectAsync(string serverId, CancellationToken cancellationToken = default)
    {
        OpcUaClient? client;

        lock (_lock)
        {
            if (!_clients.TryGetValue(serverId, out client))
            {
                _logger.LogWarning("Server {ServerId} not found in configuration", serverId);
                return;
            }
        }

        try
        {
            await client.ConnectAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to server {ServerId}", serverId);

            lock (_lock)
            {
                if (_connectionStatuses.TryGetValue(serverId, out var status))
                {
                    _connectionStatuses[serverId] = status with
                    {
                        IsConnected = false,
                        State = "Error",
                        LastError = ex.Message,
                        LastDisconnectedAt = DateTime.UtcNow
                    };

                    ConnectionStateChanged?.Invoke(this, _connectionStatuses[serverId]);
                }
            }

            throw;
        }
    }

    public async Task DisconnectAsync(string serverId, CancellationToken cancellationToken = default)
    {
        OpcUaClient? client;

        lock (_lock)
        {
            if (!_clients.TryGetValue(serverId, out client))
            {
                _logger.LogWarning("Server {ServerId} not found", serverId);
                return;
            }
        }

        await client.DisconnectAsync(cancellationToken);
    }

    public IOpcUaClient? GetClient(string serverId)
    {
        lock (_lock)
        {
            return _clients.TryGetValue(serverId, out var client) ? client : null;
        }
    }

    public IEnumerable<IOpcUaClient> GetConnectedClients()
    {
        lock (_lock)
        {
            return _clients.Values.Where(c => c.IsConnected).ToList();
        }
    }

    public IEnumerable<OpcUaConnectionStatus> GetConnectionStatuses()
    {
        lock (_lock)
        {
            return _connectionStatuses.Values.ToList();
        }
    }

    public OpcUaConnectionStatus? GetConnectionStatus(string serverId)
    {
        lock (_lock)
        {
            return _connectionStatuses.TryGetValue(serverId, out var status) ? status : null;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        lock (_lock)
        {
            foreach (var client in _clients.Values)
            {
                client.ConnectionStateChanged -= OnClientConnectionStateChanged;
                client.Dispose();
            }

            _clients.Clear();
            _connectionStatuses.Clear();
        }
    }
}
