using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class DataSourceConfigurationProviderFake : IDataSourceConfigurationProvider
	{
		private readonly IDictionary<string, DataSourceConfiguration> data = new Dictionary<string, DataSourceConfiguration>();

		public DataSourceConfiguration ForTenant(string tenant)
		{
			DataSourceConfiguration ret;
			return data.TryGetValue(tenant, out ret) ? ret : null;
		}

		public void Has(string tenant, DataSourceConfiguration dataSourceConfiguration)
		{
			data[tenant] = dataSourceConfiguration;
		}
	}
}