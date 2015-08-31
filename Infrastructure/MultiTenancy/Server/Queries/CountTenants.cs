using System.Linq;
using NHibernate.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class CountTenants : ICountTenants
	{
		private readonly ICurrentTenantSession _tenantSession;

		public CountTenants(ICurrentTenantSession tenantSession)
		{
			_tenantSession = tenantSession;
		}

		public int Count()
		{
			return _tenantSession.CurrentSession()
				.Query<Tenant>()
				.Count();
		}
	}
}