using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class ApplicationUserQuery : IApplicationUserQuery
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly IApplicationUserTenantQuery _applicationUserTenantQuery;

		public ApplicationUserQuery(ICurrentTenantSession currentTenantSession, 
															IApplicationUserTenantQuery applicationUserTenantQuery)
		{
			_currentTenantSession = currentTenantSession;
			_applicationUserTenantQuery = applicationUserTenantQuery;
		}

		public PasswordPolicyForUser FindUserData(string userName)
		{
			var readPersonInfo = _applicationUserTenantQuery.Find(userName);
			if (readPersonInfo == null)
				return null;

			var session = _currentTenantSession.CurrentSession();
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

	public class ApplicationUserQuery_OldSchema : IApplicationUserQuery
	{
		private readonly ICurrentTenantSession _currentTenantSession;
		private readonly IApplicationUserTenantQuery _applicationUserTenantQuery;

		public ApplicationUserQuery_OldSchema(ICurrentTenantSession currentTenantSession,
															IApplicationUserTenantQuery applicationUserTenantQuery)
		{
			_currentTenantSession = currentTenantSession;
			_applicationUserTenantQuery = applicationUserTenantQuery;
		}

		public PasswordPolicyForUser FindUserData(string userName)
		{
			var readPersonInfo = _applicationUserTenantQuery.Find(userName);
			if (readPersonInfo == null)
				return null;

			var session = _currentTenantSession.CurrentSession();
			var readPasswordPolicy = session.GetNamedQuery("passwordPolicyForUser_OldSchema")
				.SetGuid("personInfoId", readPersonInfo.Id)
				.UniqueResult<PasswordPolicyForUser>();
			if (readPasswordPolicy == null)
			{
				readPasswordPolicy = new PasswordPolicyForUser(readPersonInfo);
				session.Save("PasswordPolicyForUser_OldSchema", readPasswordPolicy);
			}
			return readPasswordPolicy;
		}
	}
}