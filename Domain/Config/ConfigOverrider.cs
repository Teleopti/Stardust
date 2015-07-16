using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace Teleopti.Ccc.Domain.Config
{
	public class ConfigOverrider : IConfigReader
	{
		private readonly IConfigReader _defaultConfigReader;
		private readonly IDictionary<string, string> _overridenSettings;

		public ConfigOverrider(IConfigReader defaultConfigReader, IDictionary<string, string> overridenSettings)
		{
			_defaultConfigReader = defaultConfigReader;
			_overridenSettings = overridenSettings;
		}

		public string AppConfig(string name)
		{
			string retValue;
			if (!_overridenSettings.TryGetValue(name, out retValue))
			{
				retValue = _defaultConfigReader.AppConfig(name);
			}
			return retValue;
		}

		public NameValueCollection AppSettings_DontUse
		{
			get { return ConfigurationManager.AppSettings; }
		}

		public ConnectionStringSettingsCollection ConnectionStrings_DontUse
		{
			get { return ConfigurationManager.ConnectionStrings; }
		}

	}
}