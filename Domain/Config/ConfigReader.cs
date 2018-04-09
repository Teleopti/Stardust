using System.Configuration;

namespace Teleopti.Ccc.Domain.Config
{
	public class ConfigReader : IConfigReader
	{
		public string AppConfig(string name)
		{
			return ConfigurationManager.AppSettings[name];
		}

		public string ConnectionString(string name)
		{
			var connectionStringSetting = ConfigurationManager.ConnectionStrings[name];
			return connectionStringSetting?.ConnectionString;
		}
	}
}