using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public class FindTenantAdminUserByEmail
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public FindTenantAdminUserByEmail(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public TenantAdminUser Find(string email)
		{
			return _currentTenantSession.CurrentSession()
				.GetNamedQuery("findTenantAdminUserByEmail")
				.SetString("email", email)
				.UniqueResult<TenantAdminUser>();
		}
	}
}