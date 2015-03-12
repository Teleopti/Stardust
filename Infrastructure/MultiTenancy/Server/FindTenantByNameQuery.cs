using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class FindTenantByNameQuery : IFindTenantByNameQuery
	{
		private readonly ICurrentTenantSession _tenantUnitOfWorkManager;

		public FindTenantByNameQuery(ICurrentTenantSession tenantUnitOfWorkManager)
		{
			_tenantUnitOfWorkManager = tenantUnitOfWorkManager;
		}

		public Tenant Find(string name)
		{
			return _tenantUnitOfWorkManager.CurrentSession()
				.GetNamedQuery("findTenantByName")
				.SetString("name", name)
				.UniqueResult<Tenant>();
		}
	}
}