using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Converters
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
            DateTimePeriod period = (DateTimePeriod)value;
            return TimeSpan.FromSeconds(Math.Round(period.ElapsedTime().TotalSeconds));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #endregion
    }
}
