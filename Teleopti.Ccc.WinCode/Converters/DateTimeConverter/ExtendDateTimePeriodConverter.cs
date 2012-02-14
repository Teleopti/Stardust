using System;
using System.Globalization;
using System.Windows.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Converters.DateTimeConverter
{
    /// <summary>
    /// Takes at DateTime and extends it by the parameter, if no parameter it makes the period x 3 (adds a period in front and back)
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-09-01
    /// </remarks>
    public class ExtendDateTimePeriodConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTimePeriod period = (DateTimePeriod)value;
            TimeSpan span = CalculateSpan(period, parameter);


            return new DateTimePeriod(SubtractFrom(period.StartDateTime,span), AddTo(period.EndDateTime,span));


        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTimePeriod period = (DateTimePeriod)value;
            TimeSpan span = CalculateSpan(period, parameter);

            if (span.TotalMinutes * 3 > period.ElapsedTime().TotalMinutes) return value;

            return new DateTimePeriod(AddTo(period.StartDateTime,span), SubtractFrom(period.EndDateTime,span));

        }
        private  static TimeSpan CalculateSpan(DateTimePeriod period, object parameter)
        {
            if (parameter is TimeSpan) return (TimeSpan) parameter;
            return period.ElapsedTime();
        }

        private static DateTime SubtractFrom(DateTime dateTime,TimeSpan timeSpan)
        {
            try
            {
                return dateTime.Subtract(timeSpan);
            }
            catch (ArgumentOutOfRangeException)
            {
                return TimeZoneHelper.ConvertToUtc(DateTime.MinValue);
            }
                
            
        }
        private static DateTime AddTo(DateTime dateTime, TimeSpan timeSpan)
        {
            try
            {
                return dateTime.Add(timeSpan);
            }
            catch (ArgumentOutOfRangeException)
            {
				return TimeZoneHelper.ConvertToUtc(DateTime.MaxValue);
            }
        }


    }

}
