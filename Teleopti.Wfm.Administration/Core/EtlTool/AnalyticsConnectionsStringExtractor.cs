using System;
using System.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;

namespace Teleopti.Wfm.Administration.Core.EtlTool
{
	public class AnalyticsConnectionsStringExtractor
	{
		private readonly ILoadAllTenants _loadAllTenants;

		public AnalyticsConnectionsStringExtractor(ILoadAllTenants loadAllTenants)
		{
			_loadAllTenants = loadAllTenants;
		}
		public string Extract(string tenantName)
		{
			var currentTenant = _loadAllTenants.Tenants()
				.SingleOrDefault(x => x.Name == tenantName);

			if (currentTenant == null)
				throw new ArgumentException($"A tenant with name '{tenantName}' could not be found.", nameof(tenantName));

			return currentTenant.DataSourceConfiguration.AnalyticsConnectionString;
		}
	}
}