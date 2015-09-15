using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class FindTenantByNameWithEnsuredTransaction : IFindTenantByNameWithEnsuredTransaction
	{
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly IFindTenantByName _findTenantByName;

		public FindTenantByNameWithEnsuredTransaction(ITenantUnitOfWork tenantUnitOfWork, IFindTenantByName findTenantByName)
		{
			_tenantUnitOfWork = tenantUnitOfWork;
			_findTenantByName = findTenantByName;
		}

		public Tenant Find(string name)
		{
			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				return _findTenantByName.Find(name);
			}
		}
	}
}