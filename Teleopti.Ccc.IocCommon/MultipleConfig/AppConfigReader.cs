using System.Configuration;

namespace Teleopti.Ccc.IocCommon.MultipleConfig
{
	public class AppConfigReader : IAppConfigReader
	{
		public string AppConfig(string key)
		{
			return ConfigurationManager.AppSettings[key];
		}
	}
}