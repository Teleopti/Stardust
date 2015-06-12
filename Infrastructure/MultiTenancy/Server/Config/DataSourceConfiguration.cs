using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config
{
	public class DataSourceConfiguration
	{
		public string AnalyticsConnectionString { get; set; }
		public string ApplicationConnectionString { get; set; }
		public IDictionary<string, string> ApplicationNHibernateConfig { get; set; }
	}
}