using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters
{
    /// <summary>
    /// Top header to sub header width converter.
    /// Convert width to sum of width.
    /// </summary>
    public class ColumnWidthConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Sum(x => (double)x);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
