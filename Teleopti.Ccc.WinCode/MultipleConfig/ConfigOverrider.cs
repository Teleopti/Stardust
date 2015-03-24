using System.Configuration;

namespace Teleopti.Ccc.WinCode.MultipleConfig
{
	public class ConfigOverrider : IConfigOverrider
	{
		private readonly string _filePath;
		private Configuration _overrideConfiguration;

		public ConfigOverrider(string filePath)
		{
			_filePath = filePath;
		}

		public AppSettingsSection AppSettings()
		{
			if (_overrideConfiguration == null)
				_overrideConfiguration = ConfigurationManager.OpenExeConfiguration(_filePath);

			return _overrideConfiguration.AppSettings;
		}
	}
}