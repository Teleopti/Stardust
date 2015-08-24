using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public class DeleteTenant
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public DeleteTenant(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public void Delete(Tenant tenant)
		{
			_currentTenantSession.CurrentSession().Delete(tenant);
		}
	}
}