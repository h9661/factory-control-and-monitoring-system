using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SmartFactory.Application.Interfaces;
using SmartFactory.Application.Services.Simulation;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Presentation.Charts;
using SmartFactory.Presentation.Services;
using SmartFactory.Presentation.ViewModels.Base;
using SmartFactory.Presentation.Views.Equipment;

namespace SmartFactory.Presentation.ViewModels.Dashboard;

/// <summary>
/// ViewModel for the main dashboard view with real-time charts.
/// </summary>
public partial class DashboardViewModel : PageViewModelBase, IDisposable
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IAlarmRepository _alarmRepository;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IFactoryContextService _factoryContext;
    private readonly IDataSourceProvider _dataSourceProvider;
    private readonly DispatcherTimer _chartRefreshTimer;

    // Constants for chart data management
    private const int MaxDataPoints = 60; // Keep last 60 data points
    private const int ChartRefreshIntervalMs = 2000;

    // Lock object for buffer operations
    private readonly object _bufferLock = new();

    // Sensor data storage (thread-safe dictionary with regular lists, protected by lock)
    private readonly ConcurrentDictionary<string, List<double>> _sensorDataBuffer = new(
        new Dictionary<string, List<double>>
        {
            ["Temperature"] = new(),
            ["Vibration"] = new(),
            ["Pressure"] = new()
        });

    // Production data storage
    private readonly List<double> _productionDataBuffer = new();

    #region Observable Properties

    [ObservableProperty]
    private EquipmentStatusSummary? _equipmentSummary;

    [ObservableProperty]
    private ProductionSummary? _productionSummary;

    [ObservableProperty]
    private AlarmSummary? _alarmSummary;

    [ObservableProperty]
    private ObservableCollection<AlarmDisplayItem> _recentAlarms = new();

    [ObservableProperty]
    private double _overallEfficiency;

    [ObservableProperty]
    private int _activeEquipmentCount;

    [ObservableProperty]
    private int _totalEquipmentCount;

    [ObservableProperty]
    private string _dataSourceStatus = "Connecting...";

    [ObservableProperty]
    private bool _isSimulationRunning;

    #endregion

    #region Chart Properties

    // Sensor Trend Chart
    [ObservableProperty]
    private ISeries[] _sensorTrendSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _sensorXAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _sensorYAxes = Array.Empty<Axis>();

    // Equipment Status Pie Chart
    [ObservableProperty]
    private ISeries[] _equipmentStatusSeries = Array.Empty<ISeries>();

    // Production Output Chart
    [ObservableProperty]
    private ISeries[] _productionSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _productionXAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _productionYAxes = Array.Empty<Axis>();

    // Current sensor values for display
    [ObservableProperty]
    private double _currentTemperature;

    [ObservableProperty]
    private double _currentVibration;

    [ObservableProperty]
    private double _currentPressure;

    #endregion

    public DashboardViewModel(
        INavigationService navigationService,
        IEquipmentRepository equipmentRepository,
        IAlarmRepository alarmRepository,
        IWorkOrderRepository workOrderRepository,
        IFactoryContextService factoryContext,
        IDataSourceProvider dataSourceProvider)
        : base(navigationService)
    {
        Title = "Dashboard";
        _equipmentRepository = equipmentRepository;
        _alarmRepository = alarmRepository;
        _workOrderRepository = workOrderRepository;
        _factoryContext = factoryContext;
        _dataSourceProvider = dataSourceProvider;

        _factoryContext.CurrentFactoryChanged += (s, f) => _ = LoadDashboardDataAsync();

        // Initialize charts
        InitializeCharts();

        // Subscribe to data source events
        SubscribeToDataSource();

        // Setup chart refresh timer
        _chartRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(ChartRefreshIntervalMs)
        };
        _chartRefreshTimer.Tick += OnChartRefreshTick;
    }

    private void InitializeCharts()
    {
        // Initialize Sensor Trend Chart
        SensorTrendSeries = new ISeries[]
        {
            ChartConfiguration.CreateSensorLineSeries("Temperature", ChartConfiguration.TemperatureColor),
            ChartConfiguration.CreateSensorLineSeries("Vibration", ChartConfiguration.VibrationColor),
            ChartConfiguration.CreateSensorLineSeries("Pressure", ChartConfiguration.PressureColor)
        };

        SensorXAxes = new Axis[]
        {
            new Axis
            {
                Name = "Time",
                NamePaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                LabelsPaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                SeparatorsPaint = new SolidColorPaint(ChartConfiguration.GridLineColor) { StrokeThickness = 1 },
                TextSize = 10,
                Labeler = value => $"{value:F0}s",
                MinStep = 5
            }
        };

        SensorYAxes = new Axis[]
        {
            new Axis
            {
                Name = "Value",
                NamePaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                LabelsPaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                SeparatorsPaint = new SolidColorPaint(ChartConfiguration.GridLineColor) { StrokeThickness = 1 },
                TextSize = 10,
                MinLimit = 0
            }
        };

        // Initialize Equipment Status Pie Chart
        UpdateEquipmentStatusChart(new EquipmentStatusSummary
        {
            RunningCount = 0,
            IdleCount = 0,
            WarningCount = 0,
            ErrorCount = 0,
            MaintenanceCount = 0,
            OfflineCount = 0
        });

        // Initialize Production Chart
        ProductionSeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name = "Units Produced",
                Values = new List<double>(),
                Fill = new SolidColorPaint(ChartConfiguration.AccentColor),
                MaxBarWidth = 25,
                Padding = 3
            }
        };

        ProductionXAxes = new Axis[]
        {
            new Axis
            {
                Name = "Time",
                NamePaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                LabelsPaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                SeparatorsPaint = new SolidColorPaint(ChartConfiguration.GridLineColor) { StrokeThickness = 1 },
                TextSize = 10
            }
        };

        ProductionYAxes = new Axis[]
        {
            new Axis
            {
                Name = "Units",
                NamePaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                LabelsPaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                SeparatorsPaint = new SolidColorPaint(ChartConfiguration.GridLineColor) { StrokeThickness = 1 },
                TextSize = 10,
                MinLimit = 0
            }
        };
    }

    private void UpdateEquipmentStatusChart(EquipmentStatusSummary summary)
    {
        EquipmentStatusSeries = new ISeries[]
        {
            CreateStatusPieSeries("Running", summary.RunningCount, ChartConfiguration.RunningColor),
            CreateStatusPieSeries("Idle", summary.IdleCount, ChartConfiguration.IdleColor),
            CreateStatusPieSeries("Warning", summary.WarningCount, ChartConfiguration.WarningColor),
            CreateStatusPieSeries("Error", summary.ErrorCount, ChartConfiguration.ErrorColor),
            CreateStatusPieSeries("Maintenance", summary.MaintenanceCount, ChartConfiguration.MaintenanceColor),
            CreateStatusPieSeries("Offline", summary.OfflineCount, ChartConfiguration.OfflineColor)
        };
    }

    private static PieSeries<double> CreateStatusPieSeries(string name, int value, SKColor color)
    {
        return new PieSeries<double>
        {
            Name = name,
            Values = new[] { (double)Math.Max(value, 0) },
            Fill = new SolidColorPaint(color),
            Stroke = new SolidColorPaint(ChartConfiguration.CardColor) { StrokeThickness = 2 },
            InnerRadius = 40,
            HoverPushout = 5
        };
    }

    private void SubscribeToDataSource()
    {
        _dataSourceProvider.SensorDataReceived += OnSensorDataReceived;
        _dataSourceProvider.EquipmentStatusChanged += OnEquipmentStatusChanged;
        _dataSourceProvider.AlarmReceived += OnAlarmReceived;
        _dataSourceProvider.ProductionReceived += OnProductionReceived;
        _dataSourceProvider.ConnectionStatusChanged += OnConnectionStatusChanged;
    }

    private void UnsubscribeFromDataSource()
    {
        _dataSourceProvider.SensorDataReceived -= OnSensorDataReceived;
        _dataSourceProvider.EquipmentStatusChanged -= OnEquipmentStatusChanged;
        _dataSourceProvider.AlarmReceived -= OnAlarmReceived;
        _dataSourceProvider.ProductionReceived -= OnProductionReceived;
        _dataSourceProvider.ConnectionStatusChanged -= OnConnectionStatusChanged;
    }

    #region Data Source Event Handlers

    private void OnSensorDataReceived(object? sender, SimulatedSensorDataEventArgs e)
    {
        System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
        {
            // Update buffer with lock for thread safety
            lock (_bufferLock)
            {
                if (_sensorDataBuffer.TryGetValue(e.TagName, out var buffer))
                {
                    buffer.Add(e.Value);
                    if (buffer.Count > MaxDataPoints)
                        buffer.RemoveAt(0);
                }
            }

            // Update current value displays
            switch (e.TagName)
            {
                case "Temperature":
                    CurrentTemperature = Math.Round(e.Value, 1);
                    break;
                case "Vibration":
                    CurrentVibration = Math.Round(e.Value, 2);
                    break;
                case "Pressure":
                    CurrentPressure = Math.Round(e.Value, 2);
                    break;
            }
        });
    }

    private void OnEquipmentStatusChanged(object? sender, SimulatedEquipmentStatusEventArgs e)
    {
        // Reload equipment summary when status changes
        _ = LoadDashboardDataAsync();
    }

    private void OnAlarmReceived(object? sender, SimulatedAlarmEventArgs e)
    {
        System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
        {
            var newAlarm = new AlarmDisplayItem
            {
                Id = Guid.NewGuid(),
                AlarmCode = e.AlarmCode,
                Message = e.Message,
                Severity = e.Severity.ToString(),
                EquipmentName = e.EquipmentCode,
                OccurredAt = e.Timestamp
            };

            RecentAlarms.Insert(0, newAlarm);
            if (RecentAlarms.Count > 10)
                RecentAlarms.RemoveAt(RecentAlarms.Count - 1);
        });
    }

    private void OnProductionReceived(object? sender, SimulatedProductionEventArgs e)
    {
        System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
        {
            lock (_bufferLock)
            {
                _productionDataBuffer.Add(e.UnitsProduced);
                if (_productionDataBuffer.Count > 12) // Keep last 12 intervals
                    _productionDataBuffer.RemoveAt(0);
            }
        });
    }

    private void OnConnectionStatusChanged(object? sender, DataSourceConnectionEventArgs e)
    {
        System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
        {
            DataSourceStatus = e.Message;
            IsSimulationRunning = e.IsConnected;
        });
    }

    #endregion

    private void OnChartRefreshTick(object? sender, EventArgs e)
    {
        // Update charts with lock for thread safety
        lock (_bufferLock)
        {
            // Update sensor trend chart
            if (SensorTrendSeries.Length >= 3)
            {
                if (SensorTrendSeries[0] is LineSeries<double> tempSeries)
                    tempSeries.Values = _sensorDataBuffer["Temperature"].ToList();
                if (SensorTrendSeries[1] is LineSeries<double> vibSeries)
                    vibSeries.Values = _sensorDataBuffer["Vibration"].ToList();
                if (SensorTrendSeries[2] is LineSeries<double> presSeries)
                    presSeries.Values = _sensorDataBuffer["Pressure"].ToList();
            }

            // Update production chart
            if (ProductionSeries.Length > 0 && ProductionSeries[0] is ColumnSeries<double> prodSeries)
            {
                prodSeries.Values = _productionDataBuffer.ToList();
            }
        }
    }

    public override async void OnNavigatedTo(object? parameter)
    {
        await LoadDashboardDataAsync();
        await StartDataSourceAsync();
        _chartRefreshTimer.Start();
    }

    public override void OnNavigatedFrom()
    {
        _chartRefreshTimer.Stop();
        base.OnNavigatedFrom();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDashboardDataAsync();
    }

    [RelayCommand]
    private void NavigateToEquipment(Guid equipmentId)
    {
        NavigationService.NavigateTo(typeof(EquipmentDetailView), equipmentId);
    }

    [RelayCommand]
    private async Task ToggleSimulationAsync()
    {
        if (_dataSourceProvider.IsConnected)
        {
            await _dataSourceProvider.StopAsync();
        }
        else
        {
            await _dataSourceProvider.StartAsync();
        }
    }

    private async Task StartDataSourceAsync()
    {
        try
        {
            if (!_dataSourceProvider.IsConnected)
            {
                await _dataSourceProvider.StartAsync();
            }
        }
        catch (Exception ex)
        {
            DataSourceStatus = $"Error: {ex.Message}";
        }
    }

    private async Task LoadDashboardDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            var factoryId = _factoryContext.CurrentFactoryId;

            // Load equipment summary
            EquipmentSummary = await _equipmentRepository.GetStatusSummaryAsync(factoryId);
            TotalEquipmentCount = EquipmentSummary.TotalCount;
            ActiveEquipmentCount = EquipmentSummary.RunningCount + EquipmentSummary.IdleCount;

            // Update equipment status pie chart
            UpdateEquipmentStatusChart(EquipmentSummary);

            // Load production summary
            ProductionSummary = await _workOrderRepository.GetProductionSummaryAsync(factoryId, DateTime.Today);
            OverallEfficiency = ProductionSummary?.YieldRate ?? 0;

            // Load alarm summary
            AlarmSummary = await _alarmRepository.GetAlarmSummaryAsync(factoryId);

            // Load recent alarms
            var alarms = await _alarmRepository.GetRecentAlarmsAsync(10, factoryId);
            RecentAlarms = new ObservableCollection<AlarmDisplayItem>(
                alarms.Select(a => new AlarmDisplayItem
                {
                    Id = a.Id,
                    AlarmCode = a.AlarmCode,
                    Message = a.Message,
                    Severity = a.Severity.ToString(),
                    EquipmentName = a.Equipment.Name,
                    OccurredAt = a.OccurredAt
                }));
        });
    }

    public void Dispose()
    {
        _chartRefreshTimer.Stop();
        UnsubscribeFromDataSource();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Display model for alarms in the dashboard.
/// </summary>
public class AlarmDisplayItem
{
    public Guid Id { get; set; }
    public string AlarmCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
