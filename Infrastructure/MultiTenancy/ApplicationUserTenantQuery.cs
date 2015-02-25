using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
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
}