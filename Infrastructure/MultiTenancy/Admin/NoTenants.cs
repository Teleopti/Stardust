using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public class NoTenants : ILoadAllTenants
	{
		public IEnumerable<Tenant> Tenants()
		{
			yield break;
		}
	}
}