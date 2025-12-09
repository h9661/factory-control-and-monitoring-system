using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using SmartFactory.Domain.Enums;
using SmartFactory.Presentation.Themes;

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
            return StatusColors.GetBrush(status);
        }
        return new SolidColorBrush(StatusColors.Default);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
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
            return StatusColors.GetBrush(severity);
        }
        return new SolidColorBrush(StatusColors.Default);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
