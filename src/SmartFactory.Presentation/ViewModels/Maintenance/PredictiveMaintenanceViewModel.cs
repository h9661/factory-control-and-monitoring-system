using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using SmartFactory.Application.DTOs.Maintenance;
using SmartFactory.Application.Services.Maintenance;
using SmartFactory.Presentation.Charts;
using SmartFactory.Presentation.Services;
using SmartFactory.Presentation.ViewModels.Base;

namespace SmartFactory.Presentation.ViewModels.Maintenance;

/// <summary>
/// ViewModel for Predictive Maintenance view with health scoring and anomaly detection.
/// </summary>
public partial class PredictiveMaintenanceViewModel : PageViewModelBase
{
    private readonly IPredictiveMaintenanceService _maintenanceService;
    private readonly IFactoryContextService _factoryContext;

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<HealthScoreDto> _equipmentHealthScores = new();

    [ObservableProperty]
    private ObservableCollection<MaintenancePredictionDto> _maintenancePredictions = new();

    [ObservableProperty]
    private ObservableCollection<AnomalyResultDto> _activeAnomalies = new();

    [ObservableProperty]
    private HealthScoreDto? _selectedEquipment;

    [ObservableProperty]
    private int _criticalCount;

    [ObservableProperty]
    private int _highRiskCount;

    [ObservableProperty]
    private int _mediumRiskCount;

    [ObservableProperty]
    private int _lowRiskCount;

    [ObservableProperty]
    private double _averageHealthScore;

    [ObservableProperty]
    private int _maintenancesDueSoon;

    #endregion

    #region Chart Properties

    // Health Score Gauge for selected equipment
    [ObservableProperty]
    private ISeries[] _healthGaugeSeries = Array.Empty<ISeries>();

    // Health Trend Chart
    [ObservableProperty]
    private ISeries[] _healthTrendSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _trendXAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _trendYAxes = Array.Empty<Axis>();

    // Risk Distribution Pie Chart
    [ObservableProperty]
    private ISeries[] _riskDistributionSeries = Array.Empty<ISeries>();

    // Component Health Bar Chart
    [ObservableProperty]
    private ISeries[] _componentHealthSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _componentXAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _componentYAxes = Array.Empty<Axis>();

    #endregion

    public PredictiveMaintenanceViewModel(
        INavigationService navigationService,
        IPredictiveMaintenanceService maintenanceService,
        IFactoryContextService factoryContext)
        : base(navigationService)
    {
        Title = "Predictive Maintenance";
        _maintenanceService = maintenanceService;
        _factoryContext = factoryContext;

        _factoryContext.CurrentFactoryChanged += (s, f) => _ = LoadMaintenanceDataAsync();

        InitializeCharts();
    }

    private void InitializeCharts()
    {
        // Initialize trend chart axes
        TrendXAxes = new Axis[]
        {
            new Axis
            {
                Name = "Time",
                NamePaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                LabelsPaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                SeparatorsPaint = new SolidColorPaint(ChartConfiguration.GridLineColor) { StrokeThickness = 1 },
                TextSize = 10,
                LabelsRotation = 45
            }
        };

        TrendYAxes = new Axis[]
        {
            new Axis
            {
                Name = "Health Score",
                NamePaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                LabelsPaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                SeparatorsPaint = new SolidColorPaint(ChartConfiguration.GridLineColor) { StrokeThickness = 1 },
                TextSize = 10,
                MinLimit = 0,
                MaxLimit = 100
            }
        };

        // Initialize component chart axes
        ComponentYAxes = new Axis[]
        {
            new Axis
            {
                Name = "Score",
                NamePaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                LabelsPaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                SeparatorsPaint = new SolidColorPaint(ChartConfiguration.GridLineColor) { StrokeThickness = 1 },
                TextSize = 10,
                MinLimit = 0,
                MaxLimit = 100
            }
        };
    }

    public override async void OnNavigatedTo(object? parameter)
    {
        await LoadMaintenanceDataAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadMaintenanceDataAsync();
    }

    [RelayCommand]
    private async Task SelectEquipmentAsync(HealthScoreDto equipment)
    {
        SelectedEquipment = equipment;
        await LoadEquipmentDetailsAsync(equipment.EquipmentId);
    }

    private async Task LoadMaintenanceDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            var factoryId = _factoryContext.CurrentFactoryId;
            if (!factoryId.HasValue) return;

            var fId = factoryId.Value;

            // Load health scores for all equipment
            var healthScores = await _maintenanceService.GetFactoryHealthScoresAsync(fId);
            EquipmentHealthScores = new ObservableCollection<HealthScoreDto>(healthScores);

            // Calculate summary statistics
            CriticalCount = healthScores.Count(h => h.RiskLevel == RiskLevel.Critical);
            HighRiskCount = healthScores.Count(h => h.RiskLevel == RiskLevel.High);
            MediumRiskCount = healthScores.Count(h => h.RiskLevel == RiskLevel.Medium);
            LowRiskCount = healthScores.Count(h => h.RiskLevel == RiskLevel.Low);
            AverageHealthScore = healthScores.Any() ? Math.Round(healthScores.Average(h => h.OverallScore), 1) : 0;

            // Update risk distribution chart
            UpdateRiskDistributionChart();

            // Load maintenance predictions
            var predictions = await _maintenanceService.GetFactoryMaintenancePredictionsAsync(fId);
            MaintenancePredictions = new ObservableCollection<MaintenancePredictionDto>(predictions);
            MaintenancesDueSoon = predictions.Count(p => p.DaysUntilMaintenance <= 14);

            // Load active anomalies
            var anomalies = await _maintenanceService.GetActiveAnomaliesAsync(fId);
            ActiveAnomalies = new ObservableCollection<AnomalyResultDto>(anomalies);

            // Select first equipment if available
            if (EquipmentHealthScores.Any() && SelectedEquipment == null)
            {
                await SelectEquipmentAsync(EquipmentHealthScores.First());
            }
        });
    }

    private async Task LoadEquipmentDetailsAsync(Guid equipmentId)
    {
        // Update health gauge
        if (SelectedEquipment != null)
        {
            UpdateHealthGauge(SelectedEquipment.OverallScore);
            UpdateComponentHealthChart(SelectedEquipment.ComponentScores);
        }

        // Load health trend
        var trendData = await _maintenanceService.GetHealthTrendAsync(
            equipmentId,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        UpdateHealthTrendChart(trendData);
    }

    private void UpdateHealthGauge(double score)
    {
        var color = GetHealthColor(score);
        var remaining = Math.Max(0, 100 - score);

        HealthGaugeSeries = new ISeries[]
        {
            new PieSeries<double>
            {
                Name = "Health",
                Values = new[] { score },
                Fill = new SolidColorPaint(color),
                InnerRadius = 60,
                MaxRadialColumnWidth = 30,
                HoverPushout = 0
            },
            new PieSeries<double>
            {
                Name = "Remaining",
                Values = new[] { remaining },
                Fill = new SolidColorPaint(SKColor.Parse("#3F3F46")),
                InnerRadius = 60,
                MaxRadialColumnWidth = 30,
                HoverPushout = 0
            }
        };
    }

    private void UpdateRiskDistributionChart()
    {
        RiskDistributionSeries = new ISeries[]
        {
            new PieSeries<double>
            {
                Name = "Critical",
                Values = new[] { (double)CriticalCount },
                Fill = new SolidColorPaint(SKColor.Parse("#F44336")),
                InnerRadius = 40,
                HoverPushout = 5
            },
            new PieSeries<double>
            {
                Name = "High",
                Values = new[] { (double)HighRiskCount },
                Fill = new SolidColorPaint(SKColor.Parse("#FF5722")),
                InnerRadius = 40,
                HoverPushout = 5
            },
            new PieSeries<double>
            {
                Name = "Medium",
                Values = new[] { (double)MediumRiskCount },
                Fill = new SolidColorPaint(SKColor.Parse("#FF9800")),
                InnerRadius = 40,
                HoverPushout = 5
            },
            new PieSeries<double>
            {
                Name = "Low",
                Values = new[] { (double)LowRiskCount },
                Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")),
                InnerRadius = 40,
                HoverPushout = 5
            }
        };
    }

    private void UpdateHealthTrendChart(List<HealthTrendPointDto> trendData)
    {
        if (!trendData.Any()) return;

        var healthValues = trendData.Select(d => d.HealthScore).ToList();
        var labels = trendData.Select(d => d.Timestamp.ToString("MM/dd HH:mm")).ToArray();

        TrendXAxes = new Axis[]
        {
            new Axis
            {
                Labels = labels,
                NamePaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                LabelsPaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                SeparatorsPaint = new SolidColorPaint(ChartConfiguration.GridLineColor) { StrokeThickness = 1 },
                TextSize = 9,
                LabelsRotation = 45,
                MinStep = Math.Max(1, labels.Length / 10)
            }
        };

        HealthTrendSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Name = "Health Score",
                Values = healthValues,
                Stroke = new SolidColorPaint(ChartConfiguration.AccentColor) { StrokeThickness = 3 },
                Fill = new SolidColorPaint(ChartConfiguration.AccentColor.WithAlpha(30)),
                GeometrySize = 4,
                GeometryFill = new SolidColorPaint(ChartConfiguration.AccentColor),
                GeometryStroke = new SolidColorPaint(ChartConfiguration.CardColor) { StrokeThickness = 1 },
                LineSmoothness = 0.3
            },
            // Add threshold lines
            new LineSeries<double>
            {
                Name = "Critical Threshold",
                Values = Enumerable.Repeat(40.0, healthValues.Count).ToList(),
                Stroke = new SolidColorPaint(SKColor.Parse("#F44336")) { StrokeThickness = 1, PathEffect = new DashEffect(new float[] { 5, 5 }) },
                Fill = null,
                GeometrySize = 0,
                LineSmoothness = 0
            },
            new LineSeries<double>
            {
                Name = "Warning Threshold",
                Values = Enumerable.Repeat(60.0, healthValues.Count).ToList(),
                Stroke = new SolidColorPaint(SKColor.Parse("#FF9800")) { StrokeThickness = 1, PathEffect = new DashEffect(new float[] { 5, 5 }) },
                Fill = null,
                GeometrySize = 0,
                LineSmoothness = 0
            }
        };
    }

    private void UpdateComponentHealthChart(List<ComponentHealthDto> components)
    {
        if (!components.Any()) return;

        var labels = components.Select(c => c.ComponentName).ToArray();
        var scores = components.Select(c => c.Score).ToList();

        ComponentXAxes = new Axis[]
        {
            new Axis
            {
                Labels = labels,
                LabelsPaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                TextSize = 11
            }
        };

        // Create bars with colors based on score
        ComponentHealthSeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name = "Component Health",
                Values = scores,
                Fill = new SolidColorPaint(ChartConfiguration.AccentColor),
                MaxBarWidth = 40,
                Padding = 10
            }
        };
    }

    private static SKColor GetHealthColor(double score)
    {
        return score switch
        {
            >= 80 => SKColor.Parse("#4CAF50"),  // Green - Healthy
            >= 60 => SKColor.Parse("#8BC34A"),  // Light Green - Good
            >= 40 => SKColor.Parse("#FF9800"),  // Orange - Fair
            >= 20 => SKColor.Parse("#FF5722"),  // Deep Orange - Poor
            _ => SKColor.Parse("#F44336")        // Red - Critical
        };
    }

    partial void OnSelectedEquipmentChanged(HealthScoreDto? value)
    {
        if (value != null)
        {
            _ = LoadEquipmentDetailsAsync(value.EquipmentId);
        }
    }
}
