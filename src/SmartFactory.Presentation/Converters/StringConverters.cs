using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Presentation.Converters;

/// <summary>
/// Converts string to Visibility based on parameter match.
/// Shows content if the value equals the ConverterParameter.
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return Visibility.Collapsed;

        return value.ToString() == parameter.ToString()
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts string equality check to boolean.
/// Returns true if value equals ConverterParameter.
/// </summary>
public class StringEqualityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        return value.ToString() == parameter.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter != null)
        {
            return parameter.ToString();
        }
        return Binding.DoNothing;
    }
}

/// <summary>
/// MultiValueConverter that checks if two values are equal.
/// </summary>
public class EqualityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2)
            return false;

        if (values[0] == null && values[1] == null)
            return true;

        if (values[0] == null || values[1] == null)
            return false;

        return values[0].Equals(values[1]);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Inverts a boolean value.
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}

/// <summary>
/// Converts a boolean value to a color brush.
/// ConverterParameter format: "TrueColor|FalseColor" (e.g., "#4CAF50|#F44336")
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string paramString)
        {
            var colors = paramString.Split('|');
            if (colors.Length >= 2)
            {
                var colorHex = boolValue ? colors[0] : colors[1];
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorHex);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    return new SolidColorBrush(Colors.Gray);
                }
            }
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts EquipmentStatus to a color brush.
/// </summary>
public class EquipmentStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EquipmentStatus status)
        {
            return status switch
            {
                EquipmentStatus.Running => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)),      // Green
                EquipmentStatus.Idle => new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3)),         // Blue
                EquipmentStatus.Warning => new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)),      // Orange
                EquipmentStatus.Error => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)),        // Red
                EquipmentStatus.Maintenance => new SolidColorBrush(Color.FromRgb(0x9C, 0x27, 0xB0)),  // Purple
                EquipmentStatus.Setup => new SolidColorBrush(Color.FromRgb(0x00, 0xBC, 0xD4)),        // Cyan
                EquipmentStatus.Offline => new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75)),      // Gray
                _ => new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75))
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
