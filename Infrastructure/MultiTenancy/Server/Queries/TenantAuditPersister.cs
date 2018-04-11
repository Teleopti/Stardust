using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface ITenantAuditPersister
	{
		void Persist(TenantAudit tenantAuditInfo);
	}

	public class TenantAuditPersister : ITenantAuditPersister
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public TenantAuditPersister(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public void Persist(TenantAudit tenantAuditInfo)
		{
			var session = _currentTenantSession.CurrentSession();
			tenantAuditInfo.Correlation = _currentTenantSession.CurrentSessionId();	
			session.Save(tenantAuditInfo);
		}
	}
	
}