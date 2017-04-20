using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public class AuthenticationInternalQuerierResult
	{
		public bool Success { get; set; }
		public string FailReason { get; set; }
		public Guid PersonId { get; set; }
		public string Tenant { get; set; }
		public DataSourceConfig DataSourceConfiguration { get; set; }
		public string TenantPassword { get; set; }
		public IDictionary<string, string> ApplicationConfig { get; set; }
	}

	public class DataSourceConfig
	{
		public string AnalyticsConnectionString { get; set; }
		public string ApplicationConnectionString { get; set; }
	}
}