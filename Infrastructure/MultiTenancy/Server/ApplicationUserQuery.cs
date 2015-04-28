using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class ApplicationUserQuery : IApplicationUserQuery
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public ApplicationUserQuery(ICurrentTenantSession currentTenantSession)
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

	public class ApplicationUserQueryOldSchema : IApplicationUserQuery
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public ApplicationUserQueryOldSchema(ICurrentTenantSession currentTenantSession)
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