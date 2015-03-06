using System;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class ApplicationAuthenticationResult
	{
		public bool Success { get; set; }
		public string FailReason { get; set; }
		public Guid PersonId { get; set; }
		public string Tenant { get; set; }
		public DataSourceConfiguration DataSourceConfiguration { get; set; }
		public bool PasswordExpired { get; set; }
		public string PasswordPolicy { get; set; }
	}
}