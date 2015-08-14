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
			return ConfigurationManager.ConnectionStrings[name].ConnectionString;
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