using System;
using System.Globalization;
using System.Windows.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Converters
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

			return (DateTime) values[0] <= DateHelper.MinSmallDateTime
				       ? null
				       : TimeZoneHelper.ConvertFromUtc((DateTime) values[0], (TimeZoneInfo) values[1])
				                       .ToString(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
