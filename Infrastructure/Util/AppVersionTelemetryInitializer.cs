using System;
using System.Reflection;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public class AppVersionTelemetryInitializer : ITelemetryInitializer
	{
		private static readonly string applicationVersion = getApplicationVersion();

		private static string getApplicationVersion()
		{
			var attr2 = Attribute
					.GetCustomAttribute(
						typeof(AppVersionTelemetryInitializer).Assembly,
						typeof(AssemblyInformationalVersionAttribute))
				as AssemblyInformationalVersionAttribute;
			return attr2 == null ? "1.0.0.0" : attr2.InformationalVersion;
		}

		public void Initialize(ITelemetry telemetry)
		{
			telemetry.Context.GlobalProperties["TeleoptiVersion"] = applicationVersion;
		}
	}
}