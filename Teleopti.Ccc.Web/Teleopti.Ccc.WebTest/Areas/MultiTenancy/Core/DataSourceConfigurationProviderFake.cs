using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class DataSourceConfigurationProviderFake : IDataSourceConfigurationProvider
	{
		private readonly IDictionary<Tenant, DataSourceConfiguration> data = new Dictionary<Tenant, DataSourceConfiguration>();

		public DataSourceConfiguration ForTenant(Tenant tenant)
		{
			DataSourceConfiguration ret;
			return data.TryGetValue(tenant, out ret) ? ret : null;
		}

		public void Has(Tenant tenant, DataSourceConfiguration dataSourceConfiguration)
		{
			data[tenant] = dataSourceConfiguration;
		}
	}
}