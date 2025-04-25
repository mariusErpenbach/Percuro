using System;
using Avalonia.Data.Converters;

namespace Percuro.Services
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return booleanValue;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}