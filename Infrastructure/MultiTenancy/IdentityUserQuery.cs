using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class IdentityUserQuery : IIdentityUserQuery
	{
		private readonly Func<ICurrentTennantSession> _currentTennantSession;

		//remove "func" when we later move away from list of datasources
		public IdentityUserQuery(Func<ICurrentTennantSession> currentTennantSession)
		{
			_currentTennantSession = currentTennantSession;
		}

		public PersonInfo FindUserData(string identity)
		{
			var session = _currentTennantSession().CurrentSession();
			return session.GetNamedQuery("identityUserQuery")
				.SetString("identity", identity)
				.UniqueResult<PersonInfo>();
		}
	}
}