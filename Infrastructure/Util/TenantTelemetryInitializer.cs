using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Util
{
	public class TenantTelemetryInitializer : ITelemetryInitializer
	{
		private readonly DataSourceState state = new DataSourceState();

		public void Initialize(ITelemetry telemetry)
		{
			telemetry.Context.Properties["TeleoptiTenant"] = tenant();
		}
		
		private string tenant()
		{
			var dataSource = state.Get();
			if (dataSource?.DataSourceName != null)
			{
				return dataSource.DataSourceName;
			}

			var principal = TeleoptiPrincipal.CurrentPrincipal;
			return !(principal?.Identity is ITeleoptiIdentity identity) ? "N/A" : identity.DataSource.DataSourceName;
		}
	}
}