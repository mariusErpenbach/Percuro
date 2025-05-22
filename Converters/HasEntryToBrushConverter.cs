using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Percuro.Converters
{
    public class HasEntryToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool hasEntry && hasEntry)
            {
                return Brushes.LightGreen; // Color for days with entries
            }
            return Brushes.Transparent; // Default background for days without entries
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
