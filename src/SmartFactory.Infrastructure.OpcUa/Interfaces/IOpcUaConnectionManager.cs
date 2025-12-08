using SmartFactory.Infrastructure.OpcUa.Models;

namespace SmartFactory.Infrastructure.OpcUa.Interfaces;

/// <summary>
/// Interface for managing OPC-UA connections to multiple servers.
/// </summary>
public interface IOpcUaConnectionManager
{
    /// <summary>
    /// Connects to all configured servers that have auto-connect enabled.
    /// </summary>
    Task ConnectAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects from all connected servers.
    /// </summary>
    Task DisconnectAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Connects to a specific server by ID.
    /// </summary>
    Task ConnectAsync(string serverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects from a specific server.
    /// </summary>
    Task DisconnectAsync(string serverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a client for a specific server.
    /// </summary>
    IOpcUaClient? GetClient(string serverId);

    /// <summary>
    /// Gets all connected clients.
    /// </summary>
    IEnumerable<IOpcUaClient> GetConnectedClients();

    /// <summary>
    /// Gets the connection status for all servers.
    /// </summary>
    IEnumerable<OpcUaConnectionStatus> GetConnectionStatuses();

    /// <summary>
    /// Gets the connection status for a specific server.
    /// </summary>
    OpcUaConnectionStatus? GetConnectionStatus(string serverId);

    /// <summary>
    /// Event raised when any server's connection state changes.
    /// </summary>
    event EventHandler<OpcUaConnectionStatus>? ConnectionStateChanged;
}
