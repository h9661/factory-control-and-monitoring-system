using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using SmartFactory.Infrastructure.OpcUa.Configuration;
using SmartFactory.Infrastructure.OpcUa.Interfaces;
using SmartFactory.Infrastructure.OpcUa.Models;

namespace SmartFactory.Infrastructure.OpcUa.Services;

/// <summary>
/// OPC-UA client implementation.
/// </summary>
public class OpcUaClient : IOpcUaClient, IDisposable
{
    private readonly ILogger<OpcUaClient> _logger;
    private readonly OpcUaOptions _options;
    private readonly OpcUaServerConfig _serverConfig;
    private Session? _session;
    private readonly Dictionary<uint, Subscription> _subscriptions = new();
    private readonly object _lock = new();
    private bool _disposed;

    public event EventHandler<bool>? ConnectionStateChanged;
    public event EventHandler<OpcUaNodeValue>? ValueReceived;

    public bool IsConnected => _session?.Connected ?? false;
    public OpcUaServerConfig ServerConfig => _serverConfig;

    public OpcUaClient(
        OpcUaServerConfig serverConfig,
        OpcUaOptions options,
        ILogger<OpcUaClient> logger)
    {
        _serverConfig = serverConfig;
        _options = options;
        _logger = logger;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
        {
            _logger.LogWarning("Already connected to {ServerId}", _serverConfig.Id);
            return;
        }

        try
        {
            _logger.LogInformation("Connecting to OPC-UA server {ServerId} at {Endpoint}",
                _serverConfig.Id, _serverConfig.EndpointUrl);

            // Create application configuration
            var config = new ApplicationConfiguration
            {
                ApplicationName = _options.ApplicationName,
                ApplicationUri = _options.ApplicationUri,
                ProductUri = _options.ProductUri,
                ApplicationType = ApplicationType.Client,
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier(),
                    AutoAcceptUntrustedCertificates = true,
                    AddAppCertToTrustedStore = false
                },
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = _options.SessionTimeoutMs }
            };

            await config.Validate(ApplicationType.Client);

            // Select endpoint
            var endpoint = CoreClientUtils.SelectEndpoint(_serverConfig.EndpointUrl, false);

            // Create session
            var identity = GetUserIdentity();
            _session = await Session.Create(
                config,
                new ConfiguredEndpoint(null, endpoint, EndpointConfiguration.Create(config)),
                false,
                _options.ApplicationName,
                (uint)_options.SessionTimeoutMs,
                identity,
                null);

            _session.KeepAlive += Session_KeepAlive;

            _logger.LogInformation("Connected to OPC-UA server {ServerId}", _serverConfig.Id);
            ConnectionStateChanged?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to OPC-UA server {ServerId}", _serverConfig.Id);
            throw;
        }
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected) return Task.CompletedTask;

        try
        {
            _logger.LogInformation("Disconnecting from OPC-UA server {ServerId}", _serverConfig.Id);

            // Remove all subscriptions
            lock (_lock)
            {
                foreach (var subscription in _subscriptions.Values)
                {
                    subscription.Delete(true);
                }
                _subscriptions.Clear();
            }

            if (_session != null)
            {
                _session.KeepAlive -= Session_KeepAlive;
                _session.Close();
                _session.Dispose();
                _session = null;
            }

            _logger.LogInformation("Disconnected from OPC-UA server {ServerId}", _serverConfig.Id);
            ConnectionStateChanged?.Invoke(this, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from OPC-UA server {ServerId}", _serverConfig.Id);
        }

        return Task.CompletedTask;
    }

    public async Task<OpcUaNodeValue> ReadNodeAsync(string nodeId, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        var node = new ReadValueId
        {
            NodeId = NodeId.Parse(nodeId),
            AttributeId = Attributes.Value
        };

        var response = await _session!.ReadAsync(
            null,
            0,
            TimestampsToReturn.Both,
            new ReadValueIdCollection { node },
            cancellationToken);

        var result = response.Results[0];
        return CreateNodeValue(nodeId, null, result);
    }

    public async Task<IEnumerable<OpcUaNodeValue>> ReadNodesAsync(IEnumerable<string> nodeIds, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        var nodes = nodeIds.Select(id => new ReadValueId
        {
            NodeId = NodeId.Parse(id),
            AttributeId = Attributes.Value
        }).ToList();

        var response = await _session!.ReadAsync(
            null,
            0,
            TimestampsToReturn.Both,
            new ReadValueIdCollection(nodes),
            cancellationToken);

        return nodes.Zip(response.Results, (node, result) =>
            CreateNodeValue(node.NodeId.ToString(), null, result));
    }

    public async Task WriteNodeAsync(string nodeId, object value, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        var writeValue = new WriteValue
        {
            NodeId = NodeId.Parse(nodeId),
            AttributeId = Attributes.Value,
            Value = new DataValue(new Variant(value))
        };

        var response = await _session!.WriteAsync(
            null,
            new WriteValueCollection { writeValue },
            cancellationToken);

        if (StatusCode.IsBad(response.Results[0]))
        {
            throw new Exception($"Failed to write value to node {nodeId}: {response.Results[0]}");
        }
    }

    public Task<uint> SubscribeAsync(IEnumerable<OpcUaNodeConfig> nodes, Action<OpcUaNodeValue> onValueChanged, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        var subscription = new Subscription(_session!.DefaultSubscription)
        {
            PublishingInterval = _options.PublishingIntervalMs,
            PublishingEnabled = true
        };

        _session!.AddSubscription(subscription);
        subscription.Create();

        foreach (var nodeConfig in nodes)
        {
            var monitoredItem = new MonitoredItem(subscription.DefaultItem)
            {
                DisplayName = nodeConfig.DisplayName,
                StartNodeId = NodeId.Parse(nodeConfig.NodeId),
                SamplingInterval = nodeConfig.SamplingIntervalMs ?? _options.SamplingIntervalMs,
                AttributeId = Attributes.Value,
                QueueSize = 1
            };

            // Store equipment info in Handle for callback
            monitoredItem.Handle = nodeConfig;

            monitoredItem.Notification += (item, args) =>
            {
                if (args.NotificationValue is MonitoredItemNotification notification)
                {
                    var config = item.Handle as OpcUaNodeConfig;
                    var value = CreateNodeValue(
                        item.StartNodeId.ToString(),
                        config,
                        notification.Value);

                    onValueChanged?.Invoke(value);
                    ValueReceived?.Invoke(this, value);
                }
            };

            subscription.AddItem(monitoredItem);
        }

        subscription.ApplyChanges();

        lock (_lock)
        {
            _subscriptions[subscription.Id] = subscription;
        }

        _logger.LogInformation("Created subscription {SubscriptionId} with {ItemCount} items on server {ServerId}",
            subscription.Id, subscription.MonitoredItemCount, _serverConfig.Id);

        return Task.FromResult(subscription.Id);
    }

    public Task UnsubscribeAsync(uint subscriptionId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_subscriptions.TryGetValue(subscriptionId, out var subscription))
            {
                subscription.Delete(true);
                _subscriptions.Remove(subscriptionId);
                _logger.LogInformation("Removed subscription {SubscriptionId} from server {ServerId}",
                    subscriptionId, _serverConfig.Id);
            }
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<OpcUaBrowseResult>> BrowseAsync(string nodeId, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        var browseDescription = new BrowseDescription
        {
            NodeId = string.IsNullOrEmpty(nodeId) ? ObjectIds.ObjectsFolder : NodeId.Parse(nodeId),
            BrowseDirection = BrowseDirection.Forward,
            ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
            IncludeSubtypes = true,
            NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method),
            ResultMask = (uint)BrowseResultMask.All
        };

        _session!.Browse(
            null,
            null,
            0,
            new BrowseDescriptionCollection { browseDescription },
            out var results,
            out _);

        IEnumerable<OpcUaBrowseResult> browseResults = results[0].References.Select(r => new OpcUaBrowseResult
        {
            NodeId = r.NodeId.ToString(),
            BrowseName = r.BrowseName.Name,
            DisplayName = r.DisplayName.Text,
            NodeClass = r.NodeClass.ToString(),
            HasChildren = r.NodeClass == NodeClass.Object
        }).ToList();

        return Task.FromResult(browseResults);
    }

    private void Session_KeepAlive(ISession session, KeepAliveEventArgs e)
    {
        if (ServiceResult.IsBad(e.Status))
        {
            _logger.LogWarning("Keep-alive failed for server {ServerId}: {Status}",
                _serverConfig.Id, e.Status);
            ConnectionStateChanged?.Invoke(this, false);
        }
    }

    private IUserIdentity GetUserIdentity()
    {
        return _serverConfig.UserIdentityType switch
        {
            "UserName" when !string.IsNullOrEmpty(_serverConfig.Username) =>
                new UserIdentity(_serverConfig.Username, _serverConfig.Password ?? string.Empty),
            _ => new UserIdentity()
        };
    }

    private void EnsureConnected()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException($"Not connected to OPC-UA server {_serverConfig.Id}");
        }
    }

    private static OpcUaNodeValue CreateNodeValue(string nodeId, OpcUaNodeConfig? config, DataValue dataValue)
    {
        return new OpcUaNodeValue
        {
            NodeId = nodeId,
            DisplayName = config?.DisplayName ?? nodeId,
            Value = dataValue.Value,
            DataType = dataValue.Value?.GetType().Name ?? "Unknown",
            SourceTimestamp = dataValue.SourceTimestamp,
            ServerTimestamp = dataValue.ServerTimestamp,
            StatusCode = dataValue.StatusCode.Code,
            IsGood = StatusCode.IsGood(dataValue.StatusCode),
            EquipmentId = config?.EquipmentId,
            SensorType = config?.SensorType,
            Unit = config?.Unit
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        DisconnectAsync().GetAwaiter().GetResult();
    }
}
