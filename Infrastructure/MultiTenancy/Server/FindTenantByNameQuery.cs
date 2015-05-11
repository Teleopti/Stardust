using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	//TODO: remove me! when authorization is in place, tenant should be read from there instead!
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