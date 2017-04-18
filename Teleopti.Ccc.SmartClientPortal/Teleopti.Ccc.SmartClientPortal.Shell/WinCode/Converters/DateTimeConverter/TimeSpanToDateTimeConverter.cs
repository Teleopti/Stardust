using System;
using System.Globalization;
using System.Windows.Data;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters.DateTimeConverter
{
    /// <summary>
    /// Combines a timespan with the parameter (datetime) or uses a default datetime
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-09-06
    /// </remarks>
    public class TimeSpanToDateTimeConverter : IValueConverter
    {
        public  DateTime DefaultDateTime { get; private set; }

        public TimeSpanToDateTimeConverter()
        {
            DefaultDateTime = new DateTime(0001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dateTime = parameter is DateTime
                                    ? (DateTime) parameter
                                    : DefaultDateTime;
            return dateTime.Date.Add((TimeSpan) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DateTime)value).Subtract(DefaultDateTime);
        }
    }
}
