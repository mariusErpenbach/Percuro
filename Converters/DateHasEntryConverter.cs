using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace Percuro.Converters
{
    public class DateHasEntryConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null || values.Count == 0 || values[0] is not DateTimeOffset buttonDateOffset)
            {
                return false; // Default to false if data is not as expected
            }

            DateTime date = buttonDateOffset.Date;

            // **** HARDCODED TEST ****
            // Check if the button's date is May 1st of the button's year
            if (date.Month == 5 && date.Day == 1)
            {
                return true; 
            }

            return false;
        }
    }
}
