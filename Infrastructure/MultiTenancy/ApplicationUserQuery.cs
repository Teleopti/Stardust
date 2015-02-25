using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class ApplicationUserQuery : IApplicationUserQuery
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public ApplicationUserQuery(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public PasswordPolicyForUser FindUserData(string userName)
		{
			var session = _currentTenantSession.CurrentSession();
			var readPersonInfo = session.GetNamedQuery("applicationUserQuery")
				.SetString("userName", userName)
				.UniqueResult<PersonInfo>();
			if (readPersonInfo == null)
				return null;

			var readPasswordPolicy = session.GetNamedQuery("passwordPolicyForUser")
				.SetEntity("personInfo", readPersonInfo)
				.UniqueResult<PasswordPolicyForUser>();
			if (readPasswordPolicy == null)
			{
				readPasswordPolicy = new PasswordPolicyForUser(readPersonInfo);
				session.Save(readPasswordPolicy);
			}
			return readPasswordPolicy;
		}
	}
}