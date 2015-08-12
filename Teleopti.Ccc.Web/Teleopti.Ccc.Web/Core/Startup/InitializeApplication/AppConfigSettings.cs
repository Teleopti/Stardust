using System.Configuration;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	public class AppConfigSettings : ISettings
	{
		public string ConfigurationFilesPath()
		{
			return ConfigurationManager.AppSettings["ConfigurationFilesPath"];
		}

		public string MessageBroker()
		{
			return ConfigurationManager.AppSettings["MessageBroker"];
		}

		public bool MessageBrokerLongPolling()
		{
			return ConfigurationManager.AppSettings.ReadValue("MessageBrokerLongPolling", () => true);
		}
	}
}