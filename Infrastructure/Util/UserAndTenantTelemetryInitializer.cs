using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public class UserAndTenantTelemetryInitializer : ITelemetryInitializer
	{
		public void Initialize(ITelemetry telemetry)
		{
			var item = tenantAndUser();
			telemetry.Context.Properties["TeleoptiTenant"] = item.Item1;
			telemetry.Context.Properties["TeleoptiUser"] = item.Item2;
		}
		
		private Tuple<string,string> tenantAndUser()
		{
			var principal = TeleoptiPrincipal.CurrentPrincipal;
			var identity = principal?.Identity as ITeleoptiIdentity;
			if (identity == null)
				return new Tuple<string, string>("N/A","N/A");

			return new Tuple<string, string>(identity.DataSource.DataSourceName,identity.TokenIdentity);
		}
	}
}