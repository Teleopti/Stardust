using System.Configuration;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Teleopti.Ccc.Web.Core
{
	public class DeployedHostTelemetryInitializer : ITelemetryInitializer
	{
		private static readonly string deployedHost = getHost();

		private static string getHost()
		{
			return ConfigurationManager.AppSettings["MessageBroker"];
		}

		public void Initialize(ITelemetry telemetry)
		{
			telemetry.Context.Properties["DeployedHost"] = deployedHost;
		}
	}
}