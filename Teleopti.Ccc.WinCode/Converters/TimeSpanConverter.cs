using System;
using System.Globalization;
using System.Windows.Data;

namespace Teleopti.Ccc.WinCode.Converters
{
    [ValueConversion(typeof(TimeSpan), typeof(double), ParameterType = typeof(string))]
    public class TimeSpanConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan) 
					return ((TimeSpan) value).TotalMinutes;
            throw new ArgumentException("Argument must be TimeSpan");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double) 
					return TimeSpan.FromMinutes((double) value);
            throw new ArgumentException("Argument must be double");
        }
    }
}
