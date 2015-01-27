using System;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public class ApplicationAuthenticationResult
	{
		public bool Success { get; set; }
		public string FailReason { get; set; }
		public Guid PersonId { get; set; }
		public string Tennant { get; set; }
		public string DataSourceEncrypted { get; set; }
		public bool PasswordExpired { get; set; }
	}
}