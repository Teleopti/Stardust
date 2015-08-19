using System.Collections.Specialized;
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
			return connectionStringSetting == null ? null : connectionStringSetting.ConnectionString;
		}

		public NameValueCollection AppSettings_DontUse
		{
			get { return ConfigurationManager.AppSettings; }
		}
	}
}