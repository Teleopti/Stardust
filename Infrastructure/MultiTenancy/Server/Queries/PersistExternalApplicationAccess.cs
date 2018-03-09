using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class PersistExternalApplicationAccess
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public PersistExternalApplicationAccess(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public void Persist(ExternalApplicationAccess externalApplicationAccess)
		{
			_currentTenantSession.CurrentSession().Save(externalApplicationAccess);
		}
	}
}