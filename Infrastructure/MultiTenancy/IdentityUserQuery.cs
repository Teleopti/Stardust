using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class IdentityUserQuery : IIdentityUserQuery
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public IdentityUserQuery(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public PersonInfo FindUserData(string identity)
		{
			var session = _currentTenantSession.CurrentSession();
			return session.GetNamedQuery("identityUserQuery")
				.SetString("identity", identity)
				.UniqueResult<PersonInfo>();
		}
	}
}