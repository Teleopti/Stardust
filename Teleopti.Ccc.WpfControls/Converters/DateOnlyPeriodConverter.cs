using System;
using System.Globalization;
using System.Windows.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Converters
{
	public class DateOnlyPeriodConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is DateOnlyPeriod)
			{
				var dateOnlyPeriod = (DateOnlyPeriod) value;
				return dateOnlyPeriod.StartDate == dateOnlyPeriod.EndDate
					       ? dateOnlyPeriod.StartDate.ToString()
					       : dateOnlyPeriod.ToString();
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
