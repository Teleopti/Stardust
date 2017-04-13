using System;
using System.Windows.Data;

namespace Teleopti.Ccc.Win.WpfControls.Converters
{
    /// <summary>
    /// Converter for DateTime
    /// Use for DataBinding to get correct Localized content.
    /// TODO: Remove this class and use DateTimeToTime (Date) Converter
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    [ValueConversion(typeof(DateTime), typeof(string), ParameterType = typeof(string))]
    class DateTimeConverter:IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
        	if (value is DateTime)
            {
                DateTime dateTime = (DateTime) value;
                return dateTime.ToShortTimeString();
            }
        	return null;
        }

    	/// <summary>
        /// </summary>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-02-19
        /// </remarks>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
