using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SmartFactory.Infrastructure;
using SmartFactory.Presentation.Services;
using SmartFactory.Presentation.ViewModels.Base;
using SmartFactory.Presentation.ViewModels.Dashboard;
using SmartFactory.Presentation.ViewModels.Equipment;
using SmartFactory.Presentation.ViewModels.Shell;
using SmartFactory.Presentation.Views.Alarms;
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
public partial class App : Application
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
        // Infrastructure
        services.AddInfrastructure(configuration);

        // Application Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IFactoryContextService, FactoryContextService>();

        // ViewModels
        services.AddTransient<ShellViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<EquipmentViewModel>();
        services.AddTransient<EquipmentDetailViewModel>();

        // Views
        services.AddTransient<ShellView>();
        services.AddTransient<DashboardView>();
        services.AddTransient<EquipmentView>();
        services.AddTransient<EquipmentDetailView>();
        services.AddTransient<ProductionView>();
        services.AddTransient<QualityView>();
        services.AddTransient<MaintenanceView>();
        services.AddTransient<AlarmsView>();
        services.AddTransient<ReportsView>();
        services.AddTransient<SettingsView>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

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
