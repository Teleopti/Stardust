using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Teleopti.Ccc.WinCode.Converters
{
    public class AvailableConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Available" : "Not available";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #endregion
    }
}
