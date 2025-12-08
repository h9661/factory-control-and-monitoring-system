using SmartFactory.Infrastructure.OpcUa.Configuration;
using SmartFactory.Infrastructure.OpcUa.Models;

namespace SmartFactory.Infrastructure.OpcUa.Interfaces;

/// <summary>
/// Interface for OPC-UA client operations.
/// </summary>
public interface IOpcUaClient
{
    /// <summary>
    /// Connects to the OPC-UA server.
    /// </summary>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects from the OPC-UA server.
    /// </summary>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current connection status.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the server configuration.
    /// </summary>
    OpcUaServerConfig ServerConfig { get; }

    /// <summary>
    /// Reads a single node value.
    /// </summary>
    Task<OpcUaNodeValue> ReadNodeAsync(string nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads multiple node values.
    /// </summary>
    Task<IEnumerable<OpcUaNodeValue>> ReadNodesAsync(IEnumerable<string> nodeIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a value to a node.
    /// </summary>
    Task WriteNodeAsync(string nodeId, object value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to node value changes.
    /// </summary>
    Task<uint> SubscribeAsync(IEnumerable<OpcUaNodeConfig> nodes, Action<OpcUaNodeValue> onValueChanged, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unsubscribes from a subscription.
    /// </summary>
    Task UnsubscribeAsync(uint subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Browses child nodes of a node.
    /// </summary>
    Task<IEnumerable<OpcUaBrowseResult>> BrowseAsync(string nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when connection state changes.
    /// </summary>
    event EventHandler<bool>? ConnectionStateChanged;

    /// <summary>
    /// Event raised when a value is received from subscription.
    /// </summary>
    event EventHandler<OpcUaNodeValue>? ValueReceived;
}

/// <summary>
/// Result of browsing OPC-UA nodes.
/// </summary>
public record OpcUaBrowseResult
{
    /// <summary>
    /// The node ID.
    /// </summary>
    public string NodeId { get; init; } = string.Empty;

    /// <summary>
    /// The browse name.
    /// </summary>
    public string BrowseName { get; init; } = string.Empty;

    /// <summary>
    /// The display name.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// The node class (Object, Variable, Method, etc.).
    /// </summary>
    public string NodeClass { get; init; } = string.Empty;

    /// <summary>
    /// Whether the node has children.
    /// </summary>
    public bool HasChildren { get; init; }
}
