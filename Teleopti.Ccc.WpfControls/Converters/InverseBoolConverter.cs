﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Teleopti.Ccc.WpfControls.Converters
{
	[ValueConversion(typeof(bool), typeof(bool))]
	public class InverseBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType != typeof (bool))
				throw new InvalidOperationException("The target must be boolean");
			return !(bool) value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
