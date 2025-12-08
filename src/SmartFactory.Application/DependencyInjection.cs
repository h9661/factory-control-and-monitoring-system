using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartFactory.Application.BackgroundServices;
using SmartFactory.Application.Events;
using SmartFactory.Application.Interfaces;
using SmartFactory.Application.Services;

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

        // Configure polling options
        if (configuration != null)
        {
            services.Configure<PollingOptions>(options =>
                configuration.GetSection(PollingOptions.SectionName).Bind(options));
        }
        else
        {
            services.Configure<PollingOptions>(options => { });
        }

        // Background Services
        services.AddHostedService<EquipmentPollingService>();
        services.AddHostedService<AlarmMonitoringService>();
        services.AddHostedService<MaintenanceCheckService>();
        services.AddHostedService<ProductionSummaryService>();

        return services;
    }
}
