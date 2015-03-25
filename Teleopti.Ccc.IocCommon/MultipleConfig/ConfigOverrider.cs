using System.Configuration;

namespace Teleopti.Ccc.IocCommon.MultipleConfig
{
	public class ConfigOverrider : IConfigOverrider
	{
		private readonly string _filePath;
		private System.Configuration.Configuration _overrideConfiguration;

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