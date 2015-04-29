using System;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public class LogonInfoModel
	{
		public Guid PersonId { get; set; }
		public string LogonName { get; set; }
		public string Identity { get; set; }
	}
}