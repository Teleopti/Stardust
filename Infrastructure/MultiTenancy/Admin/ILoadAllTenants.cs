using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public interface ILoadAllTenants
	{
		IEnumerable<Tenant> Tenants();
	}
}