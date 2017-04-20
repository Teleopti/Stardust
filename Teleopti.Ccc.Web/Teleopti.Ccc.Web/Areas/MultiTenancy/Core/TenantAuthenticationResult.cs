using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class TenantAuthenticationResult
	{
		public bool Success { get; set; }
		public string FailReason { get; set; }
		public Guid PersonId { get; set; }
		public string Tenant { get; set; }
		public DataSourceConfiguration DataSourceConfiguration { get; set; }

		public IDictionary<string, string> ApplicationConfig { get; set; }
		public bool PasswordExpired { get; set; }
		public string TenantPassword { get; set; }
	}
}