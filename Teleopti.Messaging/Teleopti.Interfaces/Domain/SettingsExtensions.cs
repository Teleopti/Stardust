using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public static class SettingsExtensions
	{
		public static T GetSettingValue<T>(this IDictionary<string, string> settingsDictionary, string key, Func<string, T> parser)
		{
			T settingValue = default(T);
			string stringValue;
			if (settingsDictionary.TryGetValue(key, out stringValue))
			{
				settingValue = parser(stringValue);
			}
			return settingValue;
		}
	}
}