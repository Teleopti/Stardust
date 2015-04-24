using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class DataSourceConfigurationProviderFake : IDataSourceConfigurationProvider
	{
		public DataSourceConfiguration ForTenant(string tenant)
		{
			return new DataSourceConfiguration
			{
				AnalyticsConnectionString = "something",
				ApplicationNHibernateConfig = new Dictionary<string, string>()
			};
		}
	}
}