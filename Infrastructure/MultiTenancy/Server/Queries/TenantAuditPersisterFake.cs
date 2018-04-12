using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class TenantAuditPersisterFake : ITenantAuditPersister
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly List<TenantAudit> _storage;


		public TenantAuditPersisterFake(ICurrentTenantSession currentTenantSession)
		{
			_storage = new List<TenantAudit>();
			_currentTenantSession = currentTenantSession;
		}

		public void Persist(TenantAudit tenantAudit)
		{
			_storage.Add(tenantAudit);
		}

		public List<TenantAudit> PersistedData => _storage;
	}
}