using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SmartFactory.Domain.Enums;
using SmartFactory.Presentation.Themes;

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
        return Binding.DoNothing;
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
        return new object[] { Binding.DoNothing, Binding.DoNothing };
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
        return Binding.DoNothing;
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
            return StatusColors.GetBrush(status);
        }
        return new SolidColorBrush(StatusColors.Default);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
