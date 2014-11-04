using System.Collections.Specialized;
using System.Configuration;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class ConfigReader : IConfigReader
	{
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