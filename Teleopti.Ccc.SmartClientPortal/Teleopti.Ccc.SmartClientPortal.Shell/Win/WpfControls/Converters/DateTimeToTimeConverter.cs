using System;
using System.Windows.Data;
using Teleopti.Ccc.WinCode.Converters.DateTimeConverter;

namespace Teleopti.Ccc.Win.WpfControls.Converters
{
    [ValueConversion(typeof(DateTime), typeof(string), ParameterType = typeof(TimeZoneInfo))]
    public class DateTimeToTimeConverter : DateTimeBaseConverter
    {
        public override object Transform(DateTime convertedDateTime, object parameter)
        {
            return convertedDateTime.ToShortTimeString();
        }

        public override object TransformBack(DateTime convertedDateTime, object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
