using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public interface ILoadAllTenantsUsers
	{
		IEnumerable<TenantAdminUser> TenantUsers();
	}
	public class LoadAllTenantsUsers : ILoadAllTenantsUsers
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;

		public LoadAllTenantsUsers(ICurrentTenantSession currentTenantSession, ITenantUnitOfWork tenantUnitOfWork)
		{
			_currentTenantSession = currentTenantSession;
			_tenantUnitOfWork = tenantUnitOfWork;
		}

		public IEnumerable<TenantAdminUser> TenantUsers()
		{
			_tenantUnitOfWork.EnsureUnitOfWorkIsStarted();
			return _currentTenantSession.CurrentSession()
				.GetNamedQuery("loadAllTenantUsers")
				.List<TenantAdminUser>();
		}
	}
}