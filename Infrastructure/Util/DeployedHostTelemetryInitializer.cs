using System;
using System.Configuration;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Infrastructure.Util
{
	public class DeployedHostTelemetryInitializer : ITelemetryInitializer
	{
		private static readonly Lazy<string> deployedHost = new Lazy<string>(getHost);

		private static string getHost()
		{
			return ConfigurationManager.AppSettings["MessageBroker"] ?? (StateHolderReader.IsInitialized ? StateHolderReader.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["MessageBroker"] : "");
		}

		public void Initialize(ITelemetry telemetry)
		{
			telemetry.Context.Properties["DeployedHost"] = deployedHost.Value;
		}
	}
}