using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Infrastructure.Data;
using SmartFactory.Infrastructure.Repositories;

namespace SmartFactory.Infrastructure;

/// <summary>
/// Extension methods for configuring Infrastructure services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database Context - SQLite
        var connectionString = configuration.GetConnectionString("SmartFactory");
        services.AddDbContext<SmartFactoryDbContext>(options =>
            options.UseSqlite(connectionString));

        // Repositories
        services.AddScoped<IFactoryRepository, FactoryRepository>();
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<IAlarmRepository, AlarmRepository>();
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
        services.AddScoped<IQualityRecordRepository, QualityRecordRepository>();
        services.AddScoped<IProductionLineRepository, ProductionLineRepository>();
        services.AddScoped<ISensorDataRepository, SensorDataRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
