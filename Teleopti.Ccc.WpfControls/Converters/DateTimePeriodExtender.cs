using System;
using System.Windows.Data;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Converters
{
    /// <summary>
    /// Returns 3x Period for zooming and scrolling
    /// </summary>
    /// 
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    class DateTimePeriodExtender:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTimePeriod)
            {
                DateTimePeriod ret = (DateTimePeriod)value;
                if (ret.StartDateTime != DateTime.MinValue)
                {
                    TimeSpan timeSpan = ret.ElapsedTime();
                   ret = ret.ChangeStartTime(timeSpan.Negate()).ChangeEndTime(timeSpan);
                }
                return ret;

            }
            //TODO: Change to use internaltexts:
            throw new ArgumentException("The argument for this ColorConverter is not a System.Drawing.Color", "value");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {


            if (value is DateTimePeriod)
            {
                DateTimePeriod ret = (DateTimePeriod)value;

                TimeSpan timeSpan = ret.ElapsedTime();
                return ret.ChangeEndTime(timeSpan.Negate()).ChangeStartTime(timeSpan);


            }
            //TODO: Change to use internaltexts:
            throw new ArgumentException("The argument for this ColorConverter is not a System.Drawing.Color", "value");
        }

    }
}
