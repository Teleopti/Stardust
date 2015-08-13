using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config
{
	public class ReadDataSourceConfiguration : IReadDataSourceConfiguration
	{
		private readonly ILoadAllTenants _loadAllTenants;

		public ReadDataSourceConfiguration(ILoadAllTenants loadAllTenants)
		{
			_loadAllTenants = loadAllTenants;
		}

		public IDictionary<string, DataSourceConfiguration> Read()
		{
			var ret = new Dictionary<string, DataSourceConfiguration>();
			foreach (var tenant in _loadAllTenants.Tenants())
			{
				ret[tenant.Name] = new DataSourceConfiguration
				{
					//AnalyticsConnectionString = tenant.AnalyticsConnectionString,
					//ApplicationConnectionString = tenant.ApplicationConnectionString,
					//ApplicationNHibernateConfig = tenant.ApplicationNHibernateConfig.ToDictionary(dic => dic.Key, dic => dic.Value)
				};
			}
			return ret;
		}
	}
}