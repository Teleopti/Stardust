using System;
using System.Globalization;
using System.Windows.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Converters
{
    /// <summary>
    /// Converter to return null if DateTime value is DateTime.Min
    /// </summary>
    public class DateTimeToLocalAndNotMinValueConverter : IMultiValueConverter
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
			InParameter.NotNull("values", values);
            InParameter.MustBeTrue("values", values.Length == 2);
            InParameter.MustBeTrue("values", values[0] is DateTime);
            InParameter.MustBeTrue("values", values[1] is TimeZoneInfo);

            if (((DateTime)values[0]).Equals(new DateTime(1900, 1, 1, 00, 00, 00))
                || ((DateTime)values[0]).Equals(DateTime.MinValue))
                return null;
            return TimeZoneInfo.ConvertTimeFromUtc((DateTime)values[0], (TimeZoneInfo)values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
