using System.Collections.Generic;
using System.Data.SqlClient;
using NHibernate.Cfg;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config
{
	public class DataSourceConfiguration
	{
		public DataSourceConfiguration()
		{
			ApplicationConnectionString = string.Empty;
			AnalyticsConnectionString = string.Empty;
			ApplicationNHibernateConfig = new Dictionary<string, string> { { Environment.CommandTimeout, "60" } };
		}

		public virtual string AnalyticsConnectionString { get; set; }
		public virtual string ApplicationConnectionString { get; set; }
		public virtual IDictionary<string, string> ApplicationNHibernateConfig { get; set; }

		public virtual void SetApplicationConnectionString(string applicationConnectionString)
		{
			new SqlConnectionStringBuilder(applicationConnectionString);
			ApplicationConnectionString = applicationConnectionString;
		}

		public virtual void SetAnalyticsConnectionString(string analyticsConnectionString)
		{
			new SqlConnectionStringBuilder(analyticsConnectionString);
			AnalyticsConnectionString = analyticsConnectionString;
		}
	}
}