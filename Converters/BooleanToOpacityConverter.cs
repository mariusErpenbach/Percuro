using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Percuro.Converters
{
    public class BooleanToOpacityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isCurrentMonth)
            {
                return isCurrentMonth ? 1.0 : 0.4; // Full opacity for current month days, less for others
            }
            return 1.0; // Default to full opacity
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
