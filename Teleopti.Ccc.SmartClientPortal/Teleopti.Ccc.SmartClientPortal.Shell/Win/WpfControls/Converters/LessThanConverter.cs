using System;
using System.Globalization;
using System.Windows.Data;

namespace Teleopti.Ccc.Win.WpfControls.Converters
{
    [ValueConversion(typeof(double), typeof(bool), ParameterType = typeof(double))]
    public class LessThanConverter : IValueConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double doubleValue = (double)value;
            double doubleParameter;

            if (parameter is double)
                doubleParameter = (double)parameter;
            else if (parameter is string)
            {
                if (!Double.TryParse((string)parameter, out doubleParameter))
                    throw new FormatException("The parameter for this LessThanConverter could not be converted to a System.Double");
            }
            else
                throw new ArgumentException("The parameter for this LessThanConverer is of an unsupported type", "parameter");

           
            return doubleValue < doubleParameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new NotSupportedException();
        }
    }
}
