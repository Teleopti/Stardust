using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class FindExternalApplicationAccess : IFindExternalApplicationAccess
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public FindExternalApplicationAccess(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public ExternalApplicationAccess FindByTokenHash(string hash)
		{
			return _currentTenantSession.CurrentSession()
				.GetNamedQuery("findByHash")
				.SetString("hash", hash)
				.UniqueResult<ExternalApplicationAccess>();
		}

		public IEnumerable<ExternalApplicationAccess> FindByPerson(Guid personId)
		{
			return _currentTenantSession.CurrentSession()
				.GetNamedQuery("findApplicationsByPerson")
				.SetGuid("personId", personId)
				.List<ExternalApplicationAccess>();
		}
	}
}