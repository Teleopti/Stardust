using System.Collections.Generic;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class Tenant
	{
#pragma warning disable 169
		private int id;
#pragma warning restore 169

		protected Tenant(){}

		public Tenant(string tenantName)
		{
			Name = tenantName;
			ApplicationConnectionString = string.Empty;
			AnalyticsConnectionString = string.Empty;
			ApplicationNHibernateConfig = new Dictionary<string, string>();
		}

		public virtual string Name { get; protected set; }
		public virtual string ApplicationConnectionString { get; protected set; }
		public virtual string AnalyticsConnectionString { get; protected set; }
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