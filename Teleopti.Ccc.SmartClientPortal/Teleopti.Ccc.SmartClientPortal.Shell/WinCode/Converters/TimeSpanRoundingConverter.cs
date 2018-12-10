using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters
{
    /// <summary>
    /// DateTimePeriod rounded to seconds
    /// </summary>
    public class TimeSpanRoundingConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
			if (value is TimeSpan)
				return TimeSpan.FromSeconds(Math.Round(((TimeSpan)value).TotalSeconds));
			if (value is DateTimePeriod)
				return TimeSpan.FromSeconds(Math.Round(((DateTimePeriod)value).ElapsedTime().TotalSeconds));
        	throw new ArgumentException("Argument must be TimeSpan or DateTimePeriod");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        #endregion
    }
}
