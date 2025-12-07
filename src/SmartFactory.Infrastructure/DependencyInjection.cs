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
        // Database Context
        services.AddDbContext<SmartFactoryDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("SmartFactory"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

        // Repositories
        services.AddScoped<IFactoryRepository, FactoryRepository>();
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<IAlarmRepository, AlarmRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
