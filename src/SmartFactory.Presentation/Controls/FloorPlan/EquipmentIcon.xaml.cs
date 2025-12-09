using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.IconPacks;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Presentation.Controls.FloorPlan;

/// <summary>
/// Equipment icon control for floor plan display.
/// </summary>
public partial class EquipmentIcon : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty EquipmentIdProperty =
        DependencyProperty.Register(nameof(EquipmentId), typeof(Guid), typeof(EquipmentIcon),
            new PropertyMetadata(Guid.Empty));

    public static readonly DependencyProperty EquipmentNameProperty =
        DependencyProperty.Register(nameof(EquipmentName), typeof(string), typeof(EquipmentIcon),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty EquipmentTypeProperty =
        DependencyProperty.Register(nameof(EquipmentType), typeof(EquipmentType), typeof(EquipmentIcon),
            new PropertyMetadata(EquipmentType.Other, OnEquipmentTypeChanged));

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(nameof(Status), typeof(EquipmentStatus), typeof(EquipmentIcon),
            new PropertyMetadata(EquipmentStatus.Offline, OnStatusChanged));

    public static readonly DependencyProperty TemperatureProperty =
        DependencyProperty.Register(nameof(Temperature), typeof(double), typeof(EquipmentIcon),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty VibrationProperty =
        DependencyProperty.Register(nameof(Vibration), typeof(double), typeof(EquipmentIcon),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty PressureProperty =
        DependencyProperty.Register(nameof(Pressure), typeof(double), typeof(EquipmentIcon),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty HealthScoreProperty =
        DependencyProperty.Register(nameof(HealthScore), typeof(double), typeof(EquipmentIcon),
            new PropertyMetadata(100.0));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(EquipmentIcon),
            new PropertyMetadata(false, OnIsSelectedChanged));

    public static readonly DependencyProperty IconKindProperty =
        DependencyProperty.Register(nameof(IconKind), typeof(PackIconMaterialKind), typeof(EquipmentIcon),
            new PropertyMetadata(PackIconMaterialKind.Cog));

    public static readonly DependencyProperty StatusColorProperty =
        DependencyProperty.Register(nameof(StatusColor), typeof(Brush), typeof(EquipmentIcon),
            new PropertyMetadata(Brushes.Gray));

    public static readonly DependencyProperty StatusColorValueProperty =
        DependencyProperty.Register(nameof(StatusColorValue), typeof(Color), typeof(EquipmentIcon),
            new PropertyMetadata(Colors.Gray));

    public static readonly DependencyProperty StatusTextProperty =
        DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(EquipmentIcon),
            new PropertyMetadata("Unknown"));

    public static readonly DependencyProperty IsPulsingProperty =
        DependencyProperty.Register(nameof(IsPulsing), typeof(bool), typeof(EquipmentIcon),
            new PropertyMetadata(false));

    #endregion

    #region Properties

    public Guid EquipmentId
    {
        get => (Guid)GetValue(EquipmentIdProperty);
        set => SetValue(EquipmentIdProperty, value);
    }

    public string EquipmentName
    {
        get => (string)GetValue(EquipmentNameProperty);
        set => SetValue(EquipmentNameProperty, value);
    }

    public EquipmentType EquipmentType
    {
        get => (EquipmentType)GetValue(EquipmentTypeProperty);
        set => SetValue(EquipmentTypeProperty, value);
    }

    public EquipmentStatus Status
    {
        get => (EquipmentStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public double Temperature
    {
        get => (double)GetValue(TemperatureProperty);
        set => SetValue(TemperatureProperty, value);
    }

    public double Vibration
    {
        get => (double)GetValue(VibrationProperty);
        set => SetValue(VibrationProperty, value);
    }

    public double Pressure
    {
        get => (double)GetValue(PressureProperty);
        set => SetValue(PressureProperty, value);
    }

    public double HealthScore
    {
        get => (double)GetValue(HealthScoreProperty);
        set => SetValue(HealthScoreProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public PackIconMaterialKind IconKind
    {
        get => (PackIconMaterialKind)GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public Brush StatusColor
    {
        get => (Brush)GetValue(StatusColorProperty);
        set => SetValue(StatusColorProperty, value);
    }

    public Color StatusColorValue
    {
        get => (Color)GetValue(StatusColorValueProperty);
        set => SetValue(StatusColorValueProperty, value);
    }

    public string StatusText
    {
        get => (string)GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    public bool IsPulsing
    {
        get => (bool)GetValue(IsPulsingProperty);
        set => SetValue(IsPulsingProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<Guid>? EquipmentClicked;
    public event EventHandler<Guid>? EquipmentDoubleClicked;

    #endregion

    public EquipmentIcon()
    {
        InitializeComponent();
        UpdateVisuals();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (e.ClickCount == 2)
        {
            EquipmentDoubleClicked?.Invoke(this, EquipmentId);
        }
        else
        {
            EquipmentClicked?.Invoke(this, EquipmentId);
        }

        e.Handled = true;
    }

    private static void OnEquipmentTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EquipmentIcon icon)
        {
            icon.UpdateIconKind();
        }
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EquipmentIcon icon)
        {
            icon.UpdateVisuals();
        }
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EquipmentIcon icon)
        {
            icon.UpdateSelectionVisual();
        }
    }

    private void UpdateIconKind()
    {
        IconKind = EquipmentType switch
        {
            EquipmentType.SMTMachine => PackIconMaterialKind.Chip,
            EquipmentType.AOIMachine => PackIconMaterialKind.Eye,
            EquipmentType.ReflowOven => PackIconMaterialKind.Fire,
            EquipmentType.WaveSolder => PackIconMaterialKind.Waves,
            EquipmentType.PickAndPlace => PackIconMaterialKind.RobotIndustrial,
            EquipmentType.SolderPastePrinter => PackIconMaterialKind.Printer,
            EquipmentType.Conveyor => PackIconMaterialKind.ArrowRightBold,
            EquipmentType.TestStation => PackIconMaterialKind.CheckCircle,
            EquipmentType.PackagingMachine => PackIconMaterialKind.PackageVariant,
            EquipmentType.CleaningMachine => PackIconMaterialKind.Broom,
            EquipmentType.XRayInspection => PackIconMaterialKind.RadioactiveCircle,
            EquipmentType.LaserMarker => PackIconMaterialKind.RayVertex,
            EquipmentType.Depaneling => PackIconMaterialKind.ContentCut,
            _ => PackIconMaterialKind.Cog
        };
    }

    private void UpdateVisuals()
    {
        var (color, text, shouldPulse) = Status switch
        {
            EquipmentStatus.Running => (Color.FromRgb(0x4C, 0xAF, 0x50), "Running", false),      // Green
            EquipmentStatus.Idle => (Color.FromRgb(0x21, 0x96, 0xF3), "Idle", false),            // Blue
            EquipmentStatus.Warning => (Color.FromRgb(0xFF, 0x98, 0x00), "Warning", true),       // Orange
            EquipmentStatus.Error => (Color.FromRgb(0xF4, 0x43, 0x36), "Error", true),           // Red
            EquipmentStatus.Maintenance => (Color.FromRgb(0x9C, 0x27, 0xB0), "Maintenance", false), // Purple
            EquipmentStatus.Setup => (Color.FromRgb(0x00, 0xBC, 0xD4), "Setup", false),          // Cyan
            EquipmentStatus.Offline => (Color.FromRgb(0x75, 0x75, 0x75), "Offline", false),      // Gray
            _ => (Color.FromRgb(0x75, 0x75, 0x75), "Unknown", false)
        };

        StatusColorValue = color;
        StatusColor = new SolidColorBrush(color);
        StatusText = text;
        IsPulsing = shouldPulse;
    }

    private void UpdateSelectionVisual()
    {
        if (FindName("SelectionBorder") is System.Windows.Controls.Border selectionBorder)
        {
            selectionBorder.Visibility = IsSelected ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
