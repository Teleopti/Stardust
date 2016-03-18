using System;
using System.Globalization;
using System.Windows.Data;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.Converter
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class IsServiceRunningJobBooleanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool && (bool)value) ? "Service" : "Manual";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
