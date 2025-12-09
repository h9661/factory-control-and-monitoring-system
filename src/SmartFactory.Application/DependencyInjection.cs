using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartFactory.Application.BackgroundServices;
using SmartFactory.Application.Events;
using SmartFactory.Application.Interfaces;
using SmartFactory.Application.Services;
using SmartFactory.Application.Services.Analytics;
using SmartFactory.Application.Services.Maintenance;
using SmartFactory.Application.Services.DataSource;
using SmartFactory.Application.Services.Simulation;

namespace SmartFactory.Application;

/// <summary>
/// Extension methods for configuring Application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application layer services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Optional configuration for binding options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // AutoMapper - register all profiles from this assembly
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // FluentValidation - register all validators from this assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Event Aggregator (Singleton for pub/sub messaging)
        services.AddSingleton<IEventAggregator, EventAggregator>();

        // Application Services
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IWorkOrderService, WorkOrderService>();
        services.AddScoped<IAlarmService, AlarmService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<IQualityService, QualityService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IFactoryService, FactoryService>();

        // Analytics Services
        services.AddScoped<IOeeCalculationService, OeeCalculationService>();

        // Predictive Maintenance Services
        services.AddScoped<IPredictiveMaintenanceService, PredictiveMaintenanceService>();

        // Configure polling options
        if (configuration != null)
        {
            services.Configure<PollingOptions>(options =>
                configuration.GetSection(PollingOptions.SectionName).Bind(options));

            // Configure simulation profile
            services.Configure<SimulationProfile>(options =>
                configuration.GetSection("SimulationProfile").Bind(options));

            // Configure data source options
            services.Configure<DataSourceOptions>(options =>
                configuration.GetSection(DataSourceOptions.SectionName).Bind(options));
        }
        else
        {
            services.Configure<PollingOptions>(options => { });
            services.Configure<SimulationProfile>(options => { });
            services.Configure<DataSourceOptions>(options => { });
        }

        // Simulation Services
        services.AddSingleton<IDataSimulatorService, FactoryDataSimulatorService>();

        // Data Source Providers
        services.AddSingleton<SimulatorDataSourceProvider>();
        services.AddSingleton<HybridDataSourceProvider>();

        // Register the appropriate IDataSourceProvider based on configuration
        services.AddSingleton<IDataSourceProvider>(sp =>
        {
            var options = configuration?.GetSection(DataSourceOptions.SectionName).Get<DataSourceOptions>()
                ?? new DataSourceOptions();

            return options.Mode switch
            {
                DataSourceMode.Simulation => sp.GetRequiredService<SimulatorDataSourceProvider>(),
                DataSourceMode.Hybrid => sp.GetRequiredService<HybridDataSourceProvider>(),
                // TODO: Add OpcUaDataSourceProvider when implemented
                _ => sp.GetRequiredService<SimulatorDataSourceProvider>()
            };
        });

        // Background Services
        services.AddHostedService<EquipmentPollingService>();
        services.AddHostedService<AlarmMonitoringService>();
        services.AddHostedService<MaintenanceCheckService>();
        services.AddHostedService<ProductionSummaryService>();

        return services;
    }
}
