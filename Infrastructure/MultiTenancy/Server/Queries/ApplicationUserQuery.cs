using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
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
}