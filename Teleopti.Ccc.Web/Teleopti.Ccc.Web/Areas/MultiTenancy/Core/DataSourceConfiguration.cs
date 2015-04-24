using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class DataSourceConfiguration
	{
		public string AnalyticsConnectionString { get; set; }
		public IDictionary<string, string> ApplicationNHibernateConfig { get; set; }
	}
}