using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IValidTenantUser
	{
		Tenant IsValidForTenant(Guid personId, string tenantPassword);
	}
}