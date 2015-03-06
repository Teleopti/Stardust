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

		public PasswordPolicyForUser FindUserData(string userName)
		{
			//TODO: tenant reuse ApplicationUserTenantQuery here
			var session = _currentTenantSession.CurrentSession();
			var readPersonInfo = session.GetNamedQuery("applicationUserQuery_OldSchema")
				.SetString("userName", userName)
				.UniqueResult<PersonInfo>();
			if (readPersonInfo == null)
				return null;

			var readPasswordPolicy = session.GetNamedQuery("passwordPolicyForUser")
				.SetGuid("personInfoId", readPersonInfo.Id)
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