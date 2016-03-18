using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.Converter
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class VisibilityOnNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
