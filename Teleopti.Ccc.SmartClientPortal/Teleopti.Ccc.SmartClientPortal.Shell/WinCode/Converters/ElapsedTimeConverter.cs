using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters
{
    /// <summary>
    /// DateTimePeriod to period.ElapsedTime()
    /// </summary>
    public class ElapsedTimeConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            var dateTimeValue = (DateTime)value;
            if (dateTimeValue == DateTime.MinValue) return null;
            var time = TimeSpan.FromSeconds(DateTime.UtcNow.Subtract(dateTimeValue).TotalSeconds);
            if (time > TimeSpan.FromDays(1)) return null;
            return new TimeSpan(time.Hours, time.Minutes, time.Seconds);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #endregion
    }
}
