using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public class LoadAllTenants : ILoadAllTenants
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public LoadAllTenants(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public IEnumerable<Tenant> Tenants()
		{
			return _currentTenantSession.CurrentSession()
				.GetNamedQuery("loadAllTenants")
				.List<Tenant>();
		}


	}
}