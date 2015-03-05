using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class TenantAndPersonId
	{
		public string Tenant { get; set; }
		public Guid PersonId { get; set; }
	}
}