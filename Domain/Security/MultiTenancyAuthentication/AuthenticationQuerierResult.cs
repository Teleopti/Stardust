using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public class AuthenticationQuerierResult
	{
		public bool Success { get; set; }
		public string FailReason { get; set; }
		public IPerson Person { get; set; }
		public IDataSource DataSource { get; set; }
		public string AnalyticsConnectionString { get; set; }
		public IDictionary<string, string> ApplicationNHibernateConfig { get; set; }
	}
}