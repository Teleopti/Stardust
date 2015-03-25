using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommon.MultipleConfig
{
	public class AppConfigReader
	{
		private readonly IConfigReader _configReader;
		private readonly IConfigOverrider _configOverrider;

		public AppConfigReader(IConfigReader configReader, IConfigOverrider configOverrider)
		{
			_configReader = configReader;
			_configOverrider = configOverrider;
			Instance = this;
		}

		public string AppConfig(string key)
		{
			var overrideSetting = _configOverrider.AppSettings().Settings[key];
			return overrideSetting == null ?
				_configReader.AppSettings[key] :
				overrideSetting.Value;
		}

		///////////////////////////
		//Don't like this but app.config is read all over the desktop client -> would be nicer with DI instead. Let's see if this could be removed in the future...
		public static AppConfigReader Instance { get; private set; }
		public static void Destroy()
		{
			Instance = null;
		}
		/////////////////////////////
	}
}