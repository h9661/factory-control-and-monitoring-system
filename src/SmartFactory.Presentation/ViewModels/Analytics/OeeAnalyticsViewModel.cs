using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SmartFactory.Application.DTOs.Analytics;
using SmartFactory.Application.Interfaces;
using SmartFactory.Application.Services.Analytics;
using SmartFactory.Presentation.Charts;
using SmartFactory.Presentation.Services;
using SmartFactory.Presentation.ViewModels.Base;

namespace SmartFactory.Presentation.ViewModels.Analytics;

/// <summary>
/// ViewModel for the OEE Analytics view providing comprehensive OEE metrics visualization.
/// </summary>
public partial class OeeAnalyticsViewModel : PageViewModelBase
{
    private readonly IOeeCalculationService _oeeService;
    private readonly IFactoryContextService _factoryContext;

    #region Observable Properties

    [ObservableProperty]
    private OeeResultDto? _currentOee;

    [ObservableProperty]
    private OeeLossBreakdownDto? _lossBreakdown;

    [ObservableProperty]
    private ObservableCollection<OeeResultDto> _equipmentOeeList = new();

    [ObservableProperty]
    private ObservableCollection<OeeComparisonDto> _comparisons = new();

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(-7);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private OeeComparisonType _selectedComparisonType = OeeComparisonType.Day;

    [ObservableProperty]
    private string _oeeClassificationText = string.Empty;

    [ObservableProperty]
    private SKColor _oeeClassificationColor = ChartConfiguration.AccentColor;

    #endregion

    #region Chart Properties

    // OEE Gauge Chart (Pie chart styled as gauge)
    [ObservableProperty]
    private ISeries[] _oeeGaugeSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _availabilityGaugeSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _performanceGaugeSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _qualityGaugeSeries = Array.Empty<ISeries>();

    // OEE Trend Chart
    [ObservableProperty]
    private ISeries[] _oeeTrendSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _trendXAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _trendYAxes = Array.Empty<Axis>();

    // Loss Breakdown Stacked Bar Chart
    [ObservableProperty]
    private ISeries[] _lossBreakdownSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _lossXAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _lossYAxes = Array.Empty<Axis>();

    // Comparison Bar Chart
    [ObservableProperty]
    private ISeries[] _comparisonSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _comparisonXAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _comparisonYAxes = Array.Empty<Axis>();

    #endregion

    public OeeAnalyticsViewModel(
        INavigationService navigationService,
        IOeeCalculationService oeeService,
        IFactoryContextService factoryContext)
        : base(navigationService)
    {
        Title = "OEE Analytics";
        _oeeService = oeeService;
        _factoryContext = factoryContext;

        _factoryContext.CurrentFactoryChanged += (s, f) => _ = LoadOeeDataAsync();

        InitializeCharts();
    }

    private void InitializeCharts()
    {
        // Initialize trend axes
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
                Name = "OEE %",
                NamePaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                LabelsPaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                SeparatorsPaint = new SolidColorPaint(ChartConfiguration.GridLineColor) { StrokeThickness = 1 },
                TextSize = 10,
                MinLimit = 0,
                MaxLimit = 100
            }
        };

        // Initialize loss breakdown axes
        LossXAxes = new Axis[]
        {
            new Axis
            {
                Labels = new[] { "Production Time" },
                LabelsPaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                TextSize = 12
            }
        };

        LossYAxes = new Axis[]
        {
            new Axis
            {
                Name = "Percentage",
                NamePaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                LabelsPaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                SeparatorsPaint = new SolidColorPaint(ChartConfiguration.GridLineColor) { StrokeThickness = 1 },
                TextSize = 10,
                MinLimit = 0,
                MaxLimit = 100
            }
        };

        // Initialize comparison axes
        ComparisonXAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                TextSize = 10,
                LabelsRotation = 45
            }
        };

        ComparisonYAxes = new Axis[]
        {
            new Axis
            {
                Name = "OEE %",
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
        await LoadOeeDataAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadOeeDataAsync();
    }

    [RelayCommand]
    private async Task ApplyDateFilterAsync()
    {
        await LoadOeeDataAsync();
    }

    [RelayCommand]
    private void SetDateRange(string range)
    {
        EndDate = DateTime.Today.AddDays(1);
        StartDate = range switch
        {
            "today" => DateTime.Today,
            "week" => DateTime.Today.AddDays(-7),
            "month" => DateTime.Today.AddMonths(-1),
            "quarter" => DateTime.Today.AddMonths(-3),
            _ => DateTime.Today.AddDays(-7)
        };

        _ = LoadOeeDataAsync();
    }

    private async Task LoadOeeDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            var factoryId = _factoryContext.CurrentFactoryId;
            if (!factoryId.HasValue) return;

            var fId = factoryId.Value;

            // Load current OEE
            CurrentOee = await _oeeService.CalculateFactoryOeeAsync(
                fId,
                StartDate,
                EndDate);

            UpdateOeeClassification();
            UpdateGaugeCharts();

            // Load OEE trend
            var trendData = await _oeeService.GetOeeTrendAsync(
                fId,
                StartDate,
                EndDate,
                OeeGranularity.Hourly);

            UpdateTrendChart(trendData);

            // Load loss breakdown
            LossBreakdown = await _oeeService.GetLossBreakdownAsync(
                fId,
                StartDate,
                EndDate);

            UpdateLossBreakdownChart();

            // Load comparisons
            var comparisons = await _oeeService.GetOeeComparisonAsync(
                fId,
                SelectedComparisonType,
                7);

            Comparisons = new ObservableCollection<OeeComparisonDto>(comparisons);
            UpdateComparisonChart(comparisons);

            // Load equipment OEE list
            var equipmentOeeList = await _oeeService.GetEquipmentOeeListAsync(
                fId,
                StartDate,
                EndDate);

            EquipmentOeeList = new ObservableCollection<OeeResultDto>(equipmentOeeList);
        });
    }

    private void UpdateOeeClassification()
    {
        if (CurrentOee == null) return;

        (OeeClassificationText, OeeClassificationColor) = CurrentOee.Classification switch
        {
            OeeClassification.WorldClass => ("World Class", SKColor.Parse("#4CAF50")),
            OeeClassification.Good => ("Good", SKColor.Parse("#8BC34A")),
            OeeClassification.Average => ("Average", SKColor.Parse("#FF9800")),
            OeeClassification.NeedsImprovement => ("Needs Improvement", SKColor.Parse("#FF5722")),
            OeeClassification.Poor => ("Poor", SKColor.Parse("#F44336")),
            _ => ("N/A", ChartConfiguration.SecondaryTextColor)
        };
    }

    private void UpdateGaugeCharts()
    {
        if (CurrentOee == null) return;

        OeeGaugeSeries = CreateGaugeSeries(CurrentOee.OverallOee, GetOeeColor(CurrentOee.OverallOee));
        AvailabilityGaugeSeries = CreateGaugeSeries(CurrentOee.Availability, ChartConfiguration.RunningColor);
        PerformanceGaugeSeries = CreateGaugeSeries(CurrentOee.Performance, ChartConfiguration.IdleColor);
        QualityGaugeSeries = CreateGaugeSeries(CurrentOee.Quality, ChartConfiguration.AccentColor);
    }

    private static ISeries[] CreateGaugeSeries(double value, SKColor color)
    {
        var remaining = Math.Max(0, 100 - value);
        return new ISeries[]
        {
            new PieSeries<double>
            {
                Name = "Value",
                Values = new[] { value },
                Fill = new SolidColorPaint(color),
                InnerRadius = 60,
                MaxRadialColumnWidth = 25,
                HoverPushout = 0
            },
            new PieSeries<double>
            {
                Name = "Remaining",
                Values = new[] { remaining },
                Fill = new SolidColorPaint(SKColor.Parse("#3F3F46")),
                InnerRadius = 60,
                MaxRadialColumnWidth = 25,
                HoverPushout = 0
            }
        };
    }

    private static SKColor GetOeeColor(double oee)
    {
        return oee switch
        {
            >= 85 => SKColor.Parse("#4CAF50"),  // Green - World Class
            >= 75 => SKColor.Parse("#8BC34A"),  // Light Green - Good
            >= 60 => SKColor.Parse("#FF9800"),  // Orange - Average
            >= 40 => SKColor.Parse("#FF5722"),  // Deep Orange - Needs Improvement
            _ => SKColor.Parse("#F44336")        // Red - Poor
        };
    }

    private void UpdateTrendChart(List<OeeDataPointDto> trendData)
    {
        if (!trendData.Any()) return;

        var oeeValues = trendData.Select(d => d.OverallOee).ToList();
        var availabilityValues = trendData.Select(d => d.Availability).ToList();
        var performanceValues = trendData.Select(d => d.Performance).ToList();
        var qualityValues = trendData.Select(d => d.Quality).ToList();
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

        OeeTrendSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Name = "OEE",
                Values = oeeValues,
                Stroke = new SolidColorPaint(ChartConfiguration.AccentColor) { StrokeThickness = 3 },
                Fill = new SolidColorPaint(ChartConfiguration.AccentColor.WithAlpha(30)),
                GeometrySize = 0,
                LineSmoothness = 0.3
            },
            new LineSeries<double>
            {
                Name = "Availability",
                Values = availabilityValues,
                Stroke = new SolidColorPaint(ChartConfiguration.RunningColor) { StrokeThickness = 2 },
                Fill = null,
                GeometrySize = 0,
                LineSmoothness = 0.3
            },
            new LineSeries<double>
            {
                Name = "Performance",
                Values = performanceValues,
                Stroke = new SolidColorPaint(ChartConfiguration.IdleColor) { StrokeThickness = 2 },
                Fill = null,
                GeometrySize = 0,
                LineSmoothness = 0.3
            },
            new LineSeries<double>
            {
                Name = "Quality",
                Values = qualityValues,
                Stroke = new SolidColorPaint(ChartConfiguration.WarningColor) { StrokeThickness = 2 },
                Fill = null,
                GeometrySize = 0,
                LineSmoothness = 0.3
            }
        };
    }

    private void UpdateLossBreakdownChart()
    {
        if (LossBreakdown == null) return;

        LossBreakdownSeries = new ISeries[]
        {
            new StackedColumnSeries<double>
            {
                Name = "Effective Production",
                Values = new[] { LossBreakdown.EffectiveProductionPercent },
                Fill = new SolidColorPaint(ChartConfiguration.RunningColor),
                MaxBarWidth = 80
            },
            new StackedColumnSeries<double>
            {
                Name = "Availability Loss",
                Values = new[] { LossBreakdown.AvailabilityLossPercent },
                Fill = new SolidColorPaint(ChartConfiguration.ErrorColor),
                MaxBarWidth = 80
            },
            new StackedColumnSeries<double>
            {
                Name = "Performance Loss",
                Values = new[] { LossBreakdown.PerformanceLossPercent },
                Fill = new SolidColorPaint(ChartConfiguration.WarningColor),
                MaxBarWidth = 80
            },
            new StackedColumnSeries<double>
            {
                Name = "Quality Loss",
                Values = new[] { LossBreakdown.QualityLossPercent },
                Fill = new SolidColorPaint(ChartConfiguration.MaintenanceColor),
                MaxBarWidth = 80
            }
        };
    }

    private void UpdateComparisonChart(List<OeeComparisonDto> comparisons)
    {
        if (!comparisons.Any()) return;

        var labels = comparisons.Select(c => c.Label).ToArray();
        var oeeValues = comparisons.Select(c => c.OverallOee).ToList();

        ComparisonXAxes = new Axis[]
        {
            new Axis
            {
                Labels = labels,
                LabelsPaint = new SolidColorPaint(ChartConfiguration.SecondaryTextColor),
                TextSize = 10,
                LabelsRotation = 45
            }
        };

        // Color bars based on OEE value
        var colors = oeeValues.Select(GetOeeColor).ToList();

        ComparisonSeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name = "OEE",
                Values = oeeValues,
                Fill = new SolidColorPaint(ChartConfiguration.AccentColor),
                MaxBarWidth = 40,
                Padding = 5
            }
        };
    }

    partial void OnSelectedComparisonTypeChanged(OeeComparisonType value)
    {
        _ = LoadOeeDataAsync();
    }
}
