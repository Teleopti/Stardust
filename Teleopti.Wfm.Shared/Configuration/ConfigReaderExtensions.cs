using System;
using System.Collections.Specialized;

namespace Teleopti.Ccc.Domain.Config
{
	public static class ConfigReaderExtensions
	{
		public static bool ReadValue(this NameValueCollection configuration, string key, Func<bool> ifNotFound = null)
		{
			ifNotFound = ifNotFound ?? (() => false);
			var value = configuration.Get(key);
			bool result;
			return string.IsNullOrEmpty(value) ? ifNotFound() : (bool.TryParse(value, out result) ? result : ifNotFound());
		}

		public static bool ReadValue(this IConfigReader reader, string name, bool @default)
		{
			var value = reader.AppConfig(name);
			if (string.IsNullOrEmpty(value))
				return @default;
			bool result;
			return bool.TryParse(value, out result) ? result : @default;
		}

		public static int ReadValue(this IConfigReader reader, string name, int @default)
		{
			if (reader == null)
				return @default;
			var value = reader.AppConfig(name);
			if (string.IsNullOrEmpty(value))
				return @default;
			int result;
			return int.TryParse(value, out result) ? result : @default;
		}

		public static double ReadValue(this IConfigReader reader, string name, double @default)
		{
			var value = reader.AppConfig(name);
			if (string.IsNullOrEmpty(value))
				return @default;
			double result;
			return double.TryParse(value, out result) ? result : @default;
		}

		public static string ReadValue(this IConfigReader reader, string name, string @default)
		{
			var value = reader.AppConfig(name);
			return string.IsNullOrEmpty(value) ? @default : value;
		}

	}
}