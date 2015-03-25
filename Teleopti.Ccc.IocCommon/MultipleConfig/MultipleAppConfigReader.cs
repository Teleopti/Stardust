namespace Teleopti.Ccc.IocCommon.MultipleConfig
{
	public class MultipleAppConfigReader : IAppConfigReader
	{
		private readonly IConfigOverrider _configOverrider;
		private readonly IAppConfigReader _defaultAppConfigReader;

		public MultipleAppConfigReader(IAppConfigReader defaultAppConfigReader, IConfigOverrider configOverrider)
		{
			_defaultAppConfigReader = defaultAppConfigReader;
			_configOverrider = configOverrider;
		}

		public string AppConfig(string key)
		{
			var overrideSetting = _configOverrider.AppSettings().Settings[key];
			return overrideSetting == null ?
				_defaultAppConfigReader.AppConfig(key) :
				overrideSetting.Value;
		}
	}
}