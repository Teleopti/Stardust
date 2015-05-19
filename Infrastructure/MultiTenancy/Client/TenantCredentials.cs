using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class TenantCredentials
	{
		public TenantCredentials(Guid personId, string tenantPassword)
		{
			PersonId = personId;
			TenantPassword = tenantPassword;
		}

		public Guid PersonId { get; private set; }
		public string TenantPassword { get; private set; }
	}
}