using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class ApplicationUserTenantQuery : IApplicationUserTenantQuery
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public ApplicationUserTenantQuery(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public PersonInfo Find(string username)
		{
			var session = _currentTenantSession.CurrentSession();
			return session.GetNamedQuery("applicationUserQuery")
				.SetString("userName", username)
				.UniqueResult<PersonInfo>();
		}
	}

	public class ApplicationUserTenantQuery_OldSchema : IApplicationUserTenantQuery
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public ApplicationUserTenantQuery_OldSchema(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public PersonInfo Find(string username)
		{
			var session = _currentTenantSession.CurrentSession();
			return session.GetNamedQuery("applicationUserQuery_OldSchema")
				.SetString("userName", username)
				.UniqueResult<PersonInfo>();
		}
	}
}