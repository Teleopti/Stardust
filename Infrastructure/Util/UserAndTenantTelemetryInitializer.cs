using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public class UserAndTenantTelemetryInitializer : ITelemetryInitializer
	{
		private DataSourceState state = new DataSourceState();

		public void Initialize(ITelemetry telemetry)
		{
			var item = tenantAndUser();
			telemetry.Context.Properties["TeleoptiTenant"] = item.Item1;
			telemetry.Context.Properties["TeleoptiUser"] = item.Item2;
		}
		
		private Tuple<string,string> tenantAndUser()
		{
			var dataSource = state.Get();
			if (dataSource?.DataSourceName != null)
			{
				return new Tuple<string, string>(dataSource.DataSourceName, "N/A");
			}

			var principal = TeleoptiPrincipal.CurrentPrincipal;
			var identity = principal?.Identity as ITeleoptiIdentity;
			if (identity == null)
				return new Tuple<string, string>("N/A","N/A");

			return new Tuple<string, string>(identity.DataSource.DataSourceName,identity.Name);
		}
	}
}