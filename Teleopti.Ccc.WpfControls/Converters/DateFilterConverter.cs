using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Converters
{
    /// <summary>
    /// Filter for selecting unique dates from list
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-06-11
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses"), ValueConversion(typeof(IList<DateTimePeriod>), typeof(IList<DateTimePeriod>), ParameterType = typeof(string))]
    internal class DateFilterConverter:IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IList<DateTime> fulldates = value as IList<DateTime>;
            return fulldates != null ? fulldates.Select(d => d.Date).Distinct().ToList() : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
