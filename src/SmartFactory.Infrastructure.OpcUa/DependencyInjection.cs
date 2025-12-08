using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartFactory.Infrastructure.OpcUa.Configuration;
using SmartFactory.Infrastructure.OpcUa.Interfaces;
using SmartFactory.Infrastructure.OpcUa.Services;

namespace SmartFactory.Infrastructure.OpcUa;

/// <summary>
/// Dependency injection extensions for OPC-UA infrastructure.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds OPC-UA infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing OPC-UA settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpcUaInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure OPC-UA options
        services.Configure<OpcUaOptions>(configuration.GetSection(OpcUaOptions.SectionName));

        // Register connection manager as singleton
        services.AddSingleton<IOpcUaConnectionManager, OpcUaConnectionManager>();

        // Register equipment data collector as hosted service
        services.AddHostedService<EquipmentDataCollectorService>();

        return services;
    }

    /// <summary>
    /// Adds OPC-UA infrastructure services with custom options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure OPC-UA options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpcUaInfrastructure(
        this IServiceCollection services,
        Action<OpcUaOptions> configureOptions)
    {
        // Configure OPC-UA options
        services.Configure(configureOptions);

        // Register connection manager as singleton
        services.AddSingleton<IOpcUaConnectionManager, OpcUaConnectionManager>();

        // Register equipment data collector as hosted service
        services.AddHostedService<EquipmentDataCollectorService>();

        return services;
    }
}
