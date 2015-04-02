using System.Collections.Generic;

namespace Teleopti.Ccc.IocCommon.MultipleConfig
{
	public class AppConfigOverrider : IAppConfigReader
	{
		private readonly IAppConfigReader _defaultAppConfigReader;
		private readonly IDictionary<string, string> _overridenSettings;

		public AppConfigOverrider(IAppConfigReader defaultAppConfigReader, IDictionary<string, string> overridenSettings)
		{
			_defaultAppConfigReader = defaultAppConfigReader;
			_overridenSettings = overridenSettings;
		}

		public string AppConfig(string key)
		{
			string retValue;
			if (!_overridenSettings.TryGetValue(key, out retValue))
			{
				retValue = _defaultAppConfigReader.AppConfig(key);
			}
			return retValue;
		}
	}
}