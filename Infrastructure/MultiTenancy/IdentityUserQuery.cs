using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class IdentityUserQuery : IIdentityUserQuery
	{
		private readonly Func<ICurrentTenantSession> _currentTenantSession;

		//remove "func" when we later move away from list of datasources
		public IdentityUserQuery(Func<ICurrentTenantSession> currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public PersonInfo FindUserData(string identity)
		{
			var session = _currentTenantSession().CurrentSession();
			return session.GetNamedQuery("identityUserQuery")
				.SetString("identity", identity)
				.UniqueResult<PersonInfo>();
		}
	}
}