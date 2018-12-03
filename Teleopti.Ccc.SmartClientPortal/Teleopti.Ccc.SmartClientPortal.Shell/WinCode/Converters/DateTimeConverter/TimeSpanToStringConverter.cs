using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters.DateTimeConverter
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan timeSpan = (TimeSpan)value;
            return TimeHelper.GetLongHourMinuteTimeString(timeSpan, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
        #endregion
    }
}
