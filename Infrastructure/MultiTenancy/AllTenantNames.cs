using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class AllTenantNames : IAllTenantNames
	{
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly ILoadAllTenants _loadAllTenants;

		public AllTenantNames(
			ITenantUnitOfWork tenantUnitOfWork,
			ILoadAllTenants loadAllTenants
		)
		{
			_tenantUnitOfWork = tenantUnitOfWork;
			_loadAllTenants = loadAllTenants;
		}

		public IEnumerable<string> Tenants()
		{
			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				return _loadAllTenants.Tenants().Select(x => x.Name).ToArray();
			}
		}
	}
}