using System.Configuration;

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
	}
}