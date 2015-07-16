using System.Configuration;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.MultipleConfig;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	public class AppConfigSettings : ISettings
	{
		
		public string nhibConfPath()
		{
			return ConfigurationManager.AppSettings["nhibConfPath"];
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