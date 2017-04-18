using System;
using System.Diagnostics.CodeAnalysis;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters.DateTimeConverter;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Converters
{
    public class DateTimeToHourConverter:DateTimeBaseConverter
    {

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.ToString")]
        public override object Transform(DateTime convertedDateTime,object parameter)
        {
            return convertedDateTime.Hour.ToString();
        }

        public override object TransformBack(DateTime convertedDateTime, object parameter)
        {
            throw new System.NotImplementedException();
        }
    }
}