using System;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public class AuthenticationQueryResult
	{
		public bool Success { get; set; }
		public string FailReason { get; set; }
		public Guid PersonId { get; set; }
		public string Tenant { get; set; }
		public string DataSourceConfig { get; set; }
		public string DataSource { get; set; }
	}
}