using System;
using System.Windows.Data;
using Teleopti.Ccc.WinCode.Converters.DateTimeConverter;

namespace Teleopti.Ccc.Win.WpfControls.Converters
{
    /// <summary>
    /// values[0] is the UTC date time to format.
    /// values[1] is the time span resolution:
    /// 1.00:00:00, day format
    /// else, time format
    /// </summary>
    /// <remarks>
    /// Henrik 2009-03-23: The logic for presenting different views dependning on the Timespan should be moved and used 
    /// with ItemtemplateSelector. 
    /// This must be refactored or removed
    /// </remarks>
    public class DateTimeResolutionConverter : IMultiValueConverter
    {
        private readonly DateTimeToLocalConverter _converter = new DateTimeToLocalConverter();

        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length >= 2)
            {
                TimeZoneInfo info = values[2] as TimeZoneInfo;
                if (info != null) _converter.ConverterTimeZone = info;
            }
            else
            {
                throw new ArgumentException("The time zone to use must be supplied!","values");
            }

            DateTime dateTime = (DateTime)_converter.Convert(values[0], targetType, null, culture);
            TimeSpan resolution = (TimeSpan) values[1];
            if (resolution == TimeSpan.FromDays(1))
            {
                return dateTime.ToShortDateString();
            }
            return dateTime.ToShortTimeString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
 
    }

    
}
