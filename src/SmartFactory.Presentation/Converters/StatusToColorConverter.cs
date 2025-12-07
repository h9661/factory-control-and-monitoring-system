using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Presentation.Converters;

/// <summary>
/// Converts EquipmentStatus to color brush.
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EquipmentStatus status)
        {
            return status switch
            {
                EquipmentStatus.Running => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)),     // Green
                EquipmentStatus.Idle => new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3)),        // Blue
                EquipmentStatus.Warning => new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)),     // Orange
                EquipmentStatus.Error => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)),       // Red
                EquipmentStatus.Maintenance => new SolidColorBrush(Color.FromRgb(0x9C, 0x27, 0xB0)), // Purple
                EquipmentStatus.Setup => new SolidColorBrush(Color.FromRgb(0x00, 0xBC, 0xD4)),       // Cyan
                _ => new SolidColorBrush(Color.FromRgb(0x60, 0x7D, 0x8B))                            // Gray
            };
        }
        return new SolidColorBrush(Color.FromRgb(0x60, 0x7D, 0x8B));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts AlarmSeverity to color brush.
/// </summary>
public class AlarmSeverityToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is AlarmSeverity severity)
        {
            return severity switch
            {
                AlarmSeverity.Critical => new SolidColorBrush(Color.FromRgb(0xD3, 0x2F, 0x2F)),
                AlarmSeverity.Error => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)),
                AlarmSeverity.Warning => new SolidColorBrush(Color.FromRgb(0xFF, 0xA7, 0x26)),
                AlarmSeverity.Information => new SolidColorBrush(Color.FromRgb(0x29, 0xB6, 0xF6)),
                _ => new SolidColorBrush(Color.FromRgb(0x60, 0x7D, 0x8B))
            };
        }
        return new SolidColorBrush(Color.FromRgb(0x60, 0x7D, 0x8B));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
