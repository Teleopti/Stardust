using System;
using System.Windows.Data;

namespace Teleopti.Ccc.Sdk.SimpleSample
{
	public class TextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var key = (string)value;
			if (!key.StartsWith("xx")) return key;

			key = key.Replace("xxx", "");
			key = key.Replace("xx", "");

			return UserTexts.Resources.ResourceManager.GetString(key);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}
