using System;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public class ChangePasswordInput
	{
		public string OldPassword { get; set; }
		public string NewPassword { get; set; }
		public Guid PersonId { get; set; }
	}
}