using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Teleopti.Ccc.WinCode.Converters
{
    /// <summary>
    /// -Scroll value * view width = translate value for the view
    /// </summary>
    public class ScrollPositionConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
            {
                return DependencyProperty.UnsetValue;
            }
            return -(double)values[0] * (double)values[1];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
