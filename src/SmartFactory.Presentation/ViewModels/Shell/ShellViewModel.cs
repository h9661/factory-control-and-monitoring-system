using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Presentation.Services;
using SmartFactory.Presentation.ViewModels.Base;
using SmartFactory.Presentation.Views.Alarms;
using SmartFactory.Presentation.Views.Dashboard;
using SmartFactory.Presentation.Views.Equipment;
using SmartFactory.Presentation.Views.Maintenance;
using SmartFactory.Presentation.Views.Production;
using SmartFactory.Presentation.Views.Quality;
using SmartFactory.Presentation.Views.Analytics;
using SmartFactory.Presentation.Views.FloorPlan;
using SmartFactory.Presentation.Views.Reports;
using SmartFactory.Presentation.Views.Settings;

namespace SmartFactory.Presentation.ViewModels.Shell;

/// <summary>
/// ViewModel for the main shell/window of the application.
/// </summary>
public partial class ShellViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IFactoryContextService _factoryContext;
    private readonly IFactoryRepository _factoryRepository;
    private readonly IAlarmRepository _alarmRepository;
    private readonly DispatcherTimer _clockTimer;

    [ObservableProperty]
    private Factory? _selectedFactory;

    [ObservableProperty]
    private ObservableCollection<Factory> _factories = new();

    [ObservableProperty]
    private ObservableCollection<NavigationItem> _navigationItems = new();

    [ObservableProperty]
    private NavigationItem? _selectedNavigationItem;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private int _activeAlarmCount;

    [ObservableProperty]
    private DateTime _currentTime = DateTime.Now;

    [ObservableProperty]
    private bool _isConnected = true;

    [ObservableProperty]
    private string _connectionStatus = "Connected";

    public ShellViewModel(
        INavigationService navigationService,
        IFactoryContextService factoryContext,
        IFactoryRepository factoryRepository,
        IAlarmRepository alarmRepository)
    {
        _navigationService = navigationService;
        _factoryContext = factoryContext;
        _factoryRepository = factoryRepository;
        _alarmRepository = alarmRepository;

        Title = "Smart Factory System";

        // Subscribe to navigation events
        _navigationService.Navigated += OnNavigated;

        // Initialize clock timer
        _clockTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _clockTimer.Tick += (s, e) => CurrentTime = DateTime.Now;
        _clockTimer.Start();

        InitializeNavigationItems();
        _ = LoadFactoriesAsync();
        _ = UpdateAlarmCountAsync();
    }

    private void InitializeNavigationItems()
    {
        NavigationItems = new ObservableCollection<NavigationItem>
        {
            new("Dashboard", "ViewDashboard", typeof(DashboardView)),
            new("Floor Plan", "FloorPlan", typeof(FloorPlanView)),
            new("OEE Analytics", "ChartArc", typeof(OeeAnalyticsView)),
            new("Predictive Maint.", "HeartPulse", typeof(PredictiveMaintenanceView)),
            new("Equipment", "Cog", typeof(EquipmentView)),
            new("Production", "Factory", typeof(ProductionView)),
            new("Quality", "CheckCircle", typeof(QualityView)),
            new("Maintenance", "Wrench", typeof(MaintenanceView)),
            new("Alarms", "Bell", typeof(AlarmsView)),
            new("Reports", "ChartBar", typeof(ReportsView)),
            new("Settings", "CogOutline", typeof(SettingsView))
        };
    }

    private async Task LoadFactoriesAsync()
    {
        try
        {
            var factories = await _factoryRepository.GetActiveFactoriesAsync();
            Factories = new ObservableCollection<Factory>(factories);

            if (Factories.Any())
            {
                SelectedFactory = Factories.First();
            }
        }
        catch (Exception ex)
        {
            SetError($"Failed to load factories: {ex.Message}");
        }
    }

    private async Task UpdateAlarmCountAsync()
    {
        try
        {
            ActiveAlarmCount = await _alarmRepository.GetActiveAlarmCountAsync(SelectedFactory?.Id);
        }
        catch
        {
            ActiveAlarmCount = 0;
        }
    }

    partial void OnSelectedFactoryChanged(Factory? value)
    {
        if (value != null)
        {
            _factoryContext.SetCurrentFactory(value);
            _ = UpdateAlarmCountAsync();
        }
    }

    partial void OnSelectedNavigationItemChanged(NavigationItem? value)
    {
        if (value != null)
        {
            _navigationService.NavigateTo(value.ViewType);
        }
    }

    private void OnNavigated(object? sender, NavigatedEventArgs e)
    {
        CurrentView = _navigationService.CurrentView;

        // Update selected navigation item
        var navItem = NavigationItems.FirstOrDefault(n => n.ViewType == e.ViewType);
        if (navItem != null && SelectedNavigationItem != navItem)
        {
            SelectedNavigationItem = navItem;
        }
    }

    [RelayCommand]
    private void NavigateToAlarms()
    {
        _navigationService.NavigateTo(typeof(AlarmsView));
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        _navigationService.NavigateTo(typeof(SettingsView));
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadFactoriesAsync();
        await UpdateAlarmCountAsync();
    }
}

/// <summary>
/// Represents a navigation menu item.
/// </summary>
public record NavigationItem(string Title, string IconName, Type ViewType);
