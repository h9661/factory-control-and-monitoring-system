using System.Windows.Media;
using SkiaSharp;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Presentation.Themes;

/// <summary>
/// Centralized color definitions for equipment status and alarm severity.
/// This provides a single source of truth for status colors used throughout the application.
/// </summary>
public static class StatusColors
{
    #region Equipment Status Colors

    /// <summary>
    /// Color for Running status (Green).
    /// </summary>
    public static readonly Color Running = Color.FromRgb(0x4C, 0xAF, 0x50);

    /// <summary>
    /// Color for Idle status (Blue).
    /// </summary>
    public static readonly Color Idle = Color.FromRgb(0x21, 0x96, 0xF3);

    /// <summary>
    /// Color for Warning status (Orange).
    /// </summary>
    public static readonly Color Warning = Color.FromRgb(0xFF, 0x98, 0x00);

    /// <summary>
    /// Color for Error status (Red).
    /// </summary>
    public static readonly Color Error = Color.FromRgb(0xF4, 0x43, 0x36);

    /// <summary>
    /// Color for Maintenance status (Purple).
    /// </summary>
    public static readonly Color Maintenance = Color.FromRgb(0x9C, 0x27, 0xB0);

    /// <summary>
    /// Color for Setup status (Cyan).
    /// </summary>
    public static readonly Color Setup = Color.FromRgb(0x00, 0xBC, 0xD4);

    /// <summary>
    /// Color for Offline status (Gray).
    /// </summary>
    public static readonly Color Offline = Color.FromRgb(0x75, 0x75, 0x75);

    /// <summary>
    /// Default color for unknown status (Gray-Blue).
    /// </summary>
    public static readonly Color Default = Color.FromRgb(0x60, 0x7D, 0x8B);

    #endregion

    #region Alarm Severity Colors

    /// <summary>
    /// Color for Critical severity alarms (Dark Red).
    /// </summary>
    public static readonly Color Critical = Color.FromRgb(0xD3, 0x2F, 0x2F);

    /// <summary>
    /// Color for Information severity alarms (Light Blue).
    /// </summary>
    public static readonly Color Information = Color.FromRgb(0x29, 0xB6, 0xF6);

    /// <summary>
    /// Color for Warning severity alarms (Orange/Amber).
    /// </summary>
    public static readonly Color AlarmWarning = Color.FromRgb(0xFF, 0xA7, 0x26);

    #endregion

    #region Equipment Status Methods

    /// <summary>
    /// Gets the WPF Color for an equipment status.
    /// </summary>
    public static Color GetColor(EquipmentStatus status)
    {
        return status switch
        {
            EquipmentStatus.Running => Running,
            EquipmentStatus.Idle => Idle,
            EquipmentStatus.Warning => Warning,
            EquipmentStatus.Error => Error,
            EquipmentStatus.Maintenance => Maintenance,
            EquipmentStatus.Setup => Setup,
            EquipmentStatus.Offline => Offline,
            _ => Default
        };
    }

    /// <summary>
    /// Gets a SolidColorBrush for an equipment status.
    /// </summary>
    public static SolidColorBrush GetBrush(EquipmentStatus status)
    {
        return new SolidColorBrush(GetColor(status));
    }

    /// <summary>
    /// Gets the SkiaSharp SKColor for an equipment status.
    /// </summary>
    public static SKColor GetSkiaColor(EquipmentStatus status)
    {
        var color = GetColor(status);
        return new SKColor(color.R, color.G, color.B);
    }

    #endregion

    #region Alarm Severity Methods

    /// <summary>
    /// Gets the WPF Color for an alarm severity.
    /// </summary>
    public static Color GetColor(AlarmSeverity severity)
    {
        return severity switch
        {
            AlarmSeverity.Critical => Critical,
            AlarmSeverity.Error => Error,
            AlarmSeverity.Warning => AlarmWarning,
            AlarmSeverity.Information => Information,
            _ => Default
        };
    }

    /// <summary>
    /// Gets a SolidColorBrush for an alarm severity.
    /// </summary>
    public static SolidColorBrush GetBrush(AlarmSeverity severity)
    {
        return new SolidColorBrush(GetColor(severity));
    }

    /// <summary>
    /// Gets the SkiaSharp SKColor for an alarm severity.
    /// </summary>
    public static SKColor GetSkiaColor(AlarmSeverity severity)
    {
        var color = GetColor(severity);
        return new SKColor(color.R, color.G, color.B);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Converts a WPF Color to SkiaSharp SKColor.
    /// </summary>
    public static SKColor ToSkiaColor(Color color)
    {
        return new SKColor(color.R, color.G, color.B, color.A);
    }

    /// <summary>
    /// Converts a SkiaSharp SKColor to WPF Color.
    /// </summary>
    public static Color ToWpfColor(SKColor color)
    {
        return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
    }

    #endregion
}
