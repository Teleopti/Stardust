using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public class AuthenticationQueryResult
	{
		public bool Success { get; set; }
		public string FailReason { get; set; }
		public Guid PersonId { get; set; }
		public string Tenant { get; set; }
		public DataSourceConfig DataSourceConfiguration { get; set; }
		//public string DataSource { get; set; }
	}

	public class DataSourceConfig
	{
		public string AnalyticsConnectionString { get; set; }
		public IDictionary<string, string> ApplicationNHibernateConfig { get; set; }
	}
}