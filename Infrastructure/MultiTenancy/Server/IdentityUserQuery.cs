using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
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
			return session.GetNamedQuery("identityUserQuery_OldSchema")
				.SetString("identity", identity)
				.UniqueResult<PersonInfo>();
		}
	}

	public class IdentityUserQuery_OldSchema : IIdentityUserQuery
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public IdentityUserQuery_OldSchema(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public PersonInfo FindUserData(string identity)
		{
			var session = _currentTenantSession.CurrentSession();
			return session.GetNamedQuery("identityUserQuery_OldSchema")
				.SetString("identity", identity)
				.UniqueResult<PersonInfo>();
		}
	}
}