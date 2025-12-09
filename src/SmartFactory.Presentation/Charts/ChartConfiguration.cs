using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace SmartFactory.Presentation.Charts;

/// <summary>
/// Static configuration for chart styling consistent with the application's dark theme.
/// </summary>
public static class ChartConfiguration
{
    // Theme colors matching Colors.xaml
    public static readonly SKColor BackgroundColor = SKColor.Parse("#1E1E1E");
    public static readonly SKColor CardColor = SKColor.Parse("#323232");
    public static readonly SKColor TextColor = SKColor.Parse("#FFFFFF");
    public static readonly SKColor SecondaryTextColor = SKColor.Parse("#AAAAAA");
    public static readonly SKColor GridLineColor = SKColor.Parse("#3F3F46");

    // Status colors
    public static readonly SKColor RunningColor = SKColor.Parse("#4CAF50");
    public static readonly SKColor IdleColor = SKColor.Parse("#2196F3");
    public static readonly SKColor WarningColor = SKColor.Parse("#FF9800");
    public static readonly SKColor ErrorColor = SKColor.Parse("#F44336");
    public static readonly SKColor MaintenanceColor = SKColor.Parse("#9C27B0");
    public static readonly SKColor OfflineColor = SKColor.Parse("#607D8B");
    public static readonly SKColor AccentColor = SKColor.Parse("#00BCD4");

    // Sensor colors for trend charts
    public static readonly SKColor TemperatureColor = SKColor.Parse("#FF5722");
    public static readonly SKColor VibrationColor = SKColor.Parse("#4CAF50");
    public static readonly SKColor PressureColor = SKColor.Parse("#2196F3");
    public static readonly SKColor CurrentColor = SKColor.Parse("#FFEB3B");
    public static readonly SKColor SpeedColor = SKColor.Parse("#E91E63");

    /// <summary>
    /// Initializes LiveCharts with the dark theme settings.
    /// </summary>
    public static void Initialize()
    {
        LiveCharts.Configure(config =>
            config
                .AddDarkTheme()
                .HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('A'))
        );
    }

    /// <summary>
    /// Creates a standard X-axis for time-based charts.
    /// </summary>
    public static Axis CreateTimeAxis(string? name = null)
    {
        return new Axis
        {
            Name = name,
            NamePaint = new SolidColorPaint(SecondaryTextColor),
            LabelsPaint = new SolidColorPaint(SecondaryTextColor),
            SeparatorsPaint = new SolidColorPaint(GridLineColor) { StrokeThickness = 1 },
            TextSize = 12,
            LabelsRotation = 0
        };
    }

    /// <summary>
    /// Creates a standard Y-axis for value charts.
    /// </summary>
    public static Axis CreateValueAxis(string? name = null, double? minValue = null, double? maxValue = null)
    {
        return new Axis
        {
            Name = name,
            NamePaint = new SolidColorPaint(SecondaryTextColor),
            LabelsPaint = new SolidColorPaint(SecondaryTextColor),
            SeparatorsPaint = new SolidColorPaint(GridLineColor) { StrokeThickness = 1 },
            TextSize = 12,
            MinLimit = minValue,
            MaxLimit = maxValue
        };
    }

    /// <summary>
    /// Creates a line series with standard styling.
    /// </summary>
    public static LineSeries<T> CreateLineSeries<T>(string name, SKColor color, IEnumerable<T>? values = null)
    {
        return new LineSeries<T>
        {
            Name = name,
            Values = values?.ToList() ?? new List<T>(),
            Stroke = new SolidColorPaint(color) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(color.WithAlpha(30)),
            GeometrySize = 0,
            GeometryStroke = null,
            GeometryFill = null,
            LineSmoothness = 0.5
        };
    }

    /// <summary>
    /// Creates a line series for real-time sensor data.
    /// </summary>
    public static LineSeries<double> CreateSensorLineSeries(string sensorName, SKColor color)
    {
        return new LineSeries<double>
        {
            Name = sensorName,
            Values = new List<double>(),
            Stroke = new SolidColorPaint(color) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(color.WithAlpha(20)),
            GeometrySize = 0,
            GeometryStroke = null,
            GeometryFill = null,
            LineSmoothness = 0.3,
            AnimationsSpeed = TimeSpan.FromMilliseconds(300)
        };
    }

    /// <summary>
    /// Creates a pie series slice with standard styling.
    /// </summary>
    public static PieSeries<double> CreatePieSeries(string name, double value, SKColor color)
    {
        return new PieSeries<double>
        {
            Name = name,
            Values = new[] { value },
            Fill = new SolidColorPaint(color),
            Stroke = new SolidColorPaint(CardColor) { StrokeThickness = 2 },
            Pushout = 0,
            InnerRadius = 50
        };
    }

    /// <summary>
    /// Creates a column series with standard styling.
    /// </summary>
    public static ColumnSeries<T> CreateColumnSeries<T>(string name, SKColor color, IEnumerable<T>? values = null)
    {
        return new ColumnSeries<T>
        {
            Name = name,
            Values = values?.ToList() ?? new List<T>(),
            Fill = new SolidColorPaint(color),
            Stroke = null,
            MaxBarWidth = 30,
            Padding = 5
        };
    }

    /// <summary>
    /// Gets the color for a specific sensor type.
    /// </summary>
    public static SKColor GetSensorColor(string sensorType)
    {
        return sensorType.ToLower() switch
        {
            "temperature" => TemperatureColor,
            "vibration" => VibrationColor,
            "pressure" => PressureColor,
            "current" => CurrentColor,
            "speed" => SpeedColor,
            _ => AccentColor
        };
    }

    /// <summary>
    /// Gets the color for a specific equipment status.
    /// </summary>
    public static SKColor GetStatusColor(Domain.Enums.EquipmentStatus status)
    {
        return status switch
        {
            Domain.Enums.EquipmentStatus.Running => RunningColor,
            Domain.Enums.EquipmentStatus.Idle => IdleColor,
            Domain.Enums.EquipmentStatus.Warning => WarningColor,
            Domain.Enums.EquipmentStatus.Error => ErrorColor,
            Domain.Enums.EquipmentStatus.Maintenance => MaintenanceColor,
            Domain.Enums.EquipmentStatus.Offline => OfflineColor,
            _ => SecondaryTextColor
        };
    }
}
