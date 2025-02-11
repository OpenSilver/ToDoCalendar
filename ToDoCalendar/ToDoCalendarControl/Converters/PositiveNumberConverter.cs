using System;
using System.Globalization;
using System.Windows.Data;

namespace ToDoCalendarControl.Converters;

public class PositiveNumberConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is double v && v > 0 ? v : double.NaN;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
