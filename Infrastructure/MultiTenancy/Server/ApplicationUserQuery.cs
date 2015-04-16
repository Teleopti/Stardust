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

		public ApplicationLogonInfo FindUserData(string userName)
		{
			var readPersonInfo = _applicationUserTenantQuery.Find(userName);
			if (readPersonInfo == null)
				return null;

			var session = _currentTenantSession.CurrentSession();
			var readPasswordPolicy = session.GetNamedQuery("applicationLogonInfo")
				.SetGuid("personInfoId", readPersonInfo.Id)
				.UniqueResult<ApplicationLogonInfo>();
			if (readPasswordPolicy == null)
			{
				readPasswordPolicy = new ApplicationLogonInfo(readPersonInfo);
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

		public ApplicationLogonInfo FindUserData(string userName)
		{
			var readPersonInfo = _applicationUserTenantQuery.Find(userName);
			if (readPersonInfo == null)
				return null;

			var session = _currentTenantSession.CurrentSession();
			var readPasswordPolicy = session.GetNamedQuery("applicationLogonInfo_OldSchema")
				.SetGuid("personInfoId", readPersonInfo.Id)
				.UniqueResult<ApplicationLogonInfo>();
			if (readPasswordPolicy == null)
			{
				readPasswordPolicy = new ApplicationLogonInfo(readPersonInfo);
				session.Save("ApplicationLogonInfo_OldSchema", readPasswordPolicy);
			}
			return readPasswordPolicy;
		}
	}
}