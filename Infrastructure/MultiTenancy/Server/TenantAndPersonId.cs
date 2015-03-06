using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class TenantAndPersonId
	{
		public string Tenant { get; set; }
		public Guid PersonId { get; set; }
	}
}