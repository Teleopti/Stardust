using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class PersistExternalApplicationAccess : IPersistExternalApplicationAccess
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

		public void Remove(int id, Guid personId)
		{
			_currentTenantSession.CurrentSession()
				.GetNamedQuery("deleteApplicationsByIdAndPerson")
				.SetInt32("id", id)
				.SetGuid("personId", personId)
				.ExecuteUpdate();
		}
	}
}