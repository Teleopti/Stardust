using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Conventions
{
	/// <summary>
	/// Sets public module properties to appsettings values based on
	/// appsettings[appsettings key] convention.
	/// </summary>
	public class SetPropertiesFromAppSettings : ISetModuleProperties
	{
		private readonly IDictionary<string, string> _appSettingsAsDictionary;
		private const string nameConvention = "appsettings";

		public SetPropertiesFromAppSettings(IConfigReader configReader)
		{
			var appSettings = configReader.AppSettings;
			var appSettingAllKeys = appSettings.AllKeys;
			_appSettingsAsDictionary = appSettingAllKeys.ToDictionary(key => key, key => appSettings[key], StringComparer.OrdinalIgnoreCase);
		}

		public void SetPropertyValue(Autofac.Module module, PropertyInfo propertyInfo)
		{
			if (!propertyInfo.Name.StartsWith(nameConvention, StringComparison.OrdinalIgnoreCase)) return;

			string value;
			if (_appSettingsAsDictionary.TryGetValue(propertyInfo.Name.Remove(0, nameConvention.Length), out value))
			{
				propertyInfo.SetValue(module, Convert.ChangeType(value, propertyInfo.PropertyType), null);
			}
		}
	}
}