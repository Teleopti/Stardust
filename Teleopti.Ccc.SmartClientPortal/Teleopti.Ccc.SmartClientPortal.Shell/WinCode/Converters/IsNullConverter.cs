using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters
{
    public class IsNullConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #endregion
    }
}
