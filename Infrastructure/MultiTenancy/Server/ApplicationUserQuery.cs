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
}