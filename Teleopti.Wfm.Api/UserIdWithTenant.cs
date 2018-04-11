using System;

namespace Teleopti.Wfm.Api
{
	public struct UserIdWithTenant
	{
		public Guid UserId { get; set; }
		public string Tenant { get; set; }
	}
}