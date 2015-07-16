using System.Collections.Specialized;
using System.Configuration;

namespace Teleopti.Ccc.Domain.MultipleConfig
{
	public class ConfigReader : IConfigReader
	{
		public string AppConfig(string name)
		{
			return ConfigurationManager.AppSettings[name];
		}

		public NameValueCollection AppSettings
		{
			get { return ConfigurationManager.AppSettings; }
		}

		public ConnectionStringSettingsCollection ConnectionStrings
		{
			get { return ConfigurationManager.ConnectionStrings; }
		}

	}
}