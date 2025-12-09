using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application;
using SmartFactory.Infrastructure;
using SmartFactory.Infrastructure.Data;
using SmartFactory.Infrastructure.OpcUa;
using SmartFactory.Presentation.Services;
using SmartFactory.Presentation.ViewModels.Alarms;
using SmartFactory.Presentation.ViewModels.Dashboard;
using SmartFactory.Presentation.ViewModels.Equipment;
using SmartFactory.Presentation.ViewModels.Maintenance;
using SmartFactory.Presentation.ViewModels.Production;
using SmartFactory.Presentation.ViewModels.Quality;
using SmartFactory.Presentation.ViewModels.Reports;
using SmartFactory.Presentation.ViewModels.Settings;
using SmartFactory.Presentation.ViewModels.Shell;
using SmartFactory.Presentation.ViewModels.Analytics;
using SmartFactory.Presentation.ViewModels.FloorPlan;
using SmartFactory.Presentation.Views.Alarms;
using SmartFactory.Presentation.Views.Analytics;
using SmartFactory.Presentation.Views.FloorPlan;
using SmartFactory.Presentation.Views.Dashboard;
using SmartFactory.Presentation.Views.Equipment;
using SmartFactory.Presentation.Views.Maintenance;
using SmartFactory.Presentation.Views.Production;
using SmartFactory.Presentation.Views.Quality;
using SmartFactory.Presentation.Views.Reports;
using SmartFactory.Presentation.Views.Settings;
using SmartFactory.Presentation.Views.Shell;

namespace SmartFactory.Presentation;

/// <summary>
/// WPF Application entry point with dependency injection setup.
/// </summary>
public partial class App : System.Windows.Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
            })
            .UseSerilog((context, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File("logs/smartfactory-.log", rollingInterval: RollingInterval.Day);
            })
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(context.Configuration, services);
            })
            .Build();
    }

    private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        // Infrastructure Layer
        services.AddInfrastructure(configuration);

        // Application Layer (DTOs, Services, Events, Background Services)
        services.AddApplication(configuration);

        // OPC-UA Infrastructure
        services.AddOpcUaInfrastructure(configuration);

        // Presentation Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IFactoryContextService, FactoryContextService>();

        // Shell ViewModel
        services.AddSingleton<ShellViewModel>();

        // ViewModels - Dashboard
        services.AddTransient<DashboardViewModel>();

        // ViewModels - Equipment
        services.AddTransient<EquipmentViewModel>();
        services.AddTransient<EquipmentDetailViewModel>();

        // ViewModels - Production
        services.AddTransient<ProductionViewModel>();

        // ViewModels - Quality
        services.AddTransient<QualityViewModel>();

        // ViewModels - Maintenance
        services.AddTransient<MaintenanceViewModel>();
        services.AddTransient<PredictiveMaintenanceViewModel>();

        // ViewModels - Alarms
        services.AddTransient<AlarmsViewModel>();

        // ViewModels - Reports
        services.AddTransient<ReportsViewModel>();

        // ViewModels - Settings
        services.AddTransient<SettingsViewModel>();

        // ViewModels - Analytics
        services.AddTransient<OeeAnalyticsViewModel>();

        // ViewModels - Floor Plan
        services.AddTransient<FloorPlanViewModel>();

        // Views - Shell
        services.AddSingleton<ShellView>();

        // Views - Main
        services.AddTransient<DashboardView>();
        services.AddTransient<EquipmentView>();
        services.AddTransient<EquipmentDetailView>();
        services.AddTransient<ProductionView>();
        services.AddTransient<QualityView>();
        services.AddTransient<MaintenanceView>();
        services.AddTransient<AlarmsView>();
        services.AddTransient<ReportsView>();
        services.AddTransient<SettingsView>();
        services.AddTransient<OeeAnalyticsView>();
        services.AddTransient<PredictiveMaintenanceView>();
        services.AddTransient<FloorPlanView>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Apply pending database migrations
        using (var scope = _host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SmartFactoryDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        var shellView = _host.Services.GetRequiredService<ShellView>();
        shellView.DataContext = _host.Services.GetRequiredService<ShellViewModel>();

        MainWindow = shellView;
        MainWindow.Show();

        // Navigate to dashboard
        var navigationService = _host.Services.GetRequiredService<INavigationService>();
        navigationService.NavigateTo<DashboardView>();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        await _host.StopAsync(TimeSpan.FromSeconds(5));
        _host.Dispose();
        base.OnExit(e);
    }
}
