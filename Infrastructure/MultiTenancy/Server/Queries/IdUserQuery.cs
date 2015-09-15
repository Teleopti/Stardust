using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class IdUserQuery : IIdUserQuery
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public IdUserQuery(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public PersonInfo FindUserData(Guid id)
		{
			var session = _currentTenantSession.CurrentSession();
			return session.GetNamedQuery("idUserQuery")
				.SetGuid("id", id)
				.UniqueResult<PersonInfo>();
		}
	}
}