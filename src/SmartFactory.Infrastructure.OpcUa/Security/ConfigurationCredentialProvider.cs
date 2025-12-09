using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartFactory.Infrastructure.OpcUa.Configuration;

namespace SmartFactory.Infrastructure.OpcUa.Security;

/// <summary>
/// Credential provider that reads credentials from configuration.
/// WARNING: This stores credentials in plain text and should only be used for demos.
/// </summary>
public class ConfigurationCredentialProvider : IOpcUaCredentialProvider
{
    private readonly OpcUaOptions _options;
    private readonly ILogger<ConfigurationCredentialProvider> _logger;
    private bool _warningLogged;

    public ConfigurationCredentialProvider(
        IOptions<OpcUaOptions> options,
        ILogger<ConfigurationCredentialProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool SupportsSecureStorage => false;
    public string ProviderName => "Configuration";

    public Task<(string? Username, string? Password)> GetCredentialsAsync(string serverId)
    {
        // Log warning once about insecure credential storage
        if (!_warningLogged)
        {
            _logger.LogWarning(
                "Using ConfigurationCredentialProvider which stores credentials in plain text. " +
                "This is NOT recommended for production. Use EnvironmentCredentialProvider or a secure vault instead.");
            _warningLogged = true;
        }

        var server = _options.Servers.FirstOrDefault(s => s.Id == serverId);
        if (server == null)
        {
            _logger.LogWarning("Server configuration not found for server ID: {ServerId}", serverId);
            return Task.FromResult<(string?, string?)>((null, null));
        }

        return Task.FromResult<(string?, string?)>((server.Username, server.Password));
    }
}
