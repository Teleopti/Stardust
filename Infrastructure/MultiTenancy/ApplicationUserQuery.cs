using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class ApplicationUserQuery : IApplicationUserQuery
	{
		private readonly Func<ICurrentTennantSession> _currentTennantSession;

		//remove "func" when we later move away from list of datasources
		public ApplicationUserQuery(Func<ICurrentTennantSession> currentTennantSession)
		{
			_currentTennantSession = currentTennantSession;
		}

		public ApplicationUserQueryResult FindUserData(string userName)
		{
			var session = _currentTennantSession().Session();
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
			return new ApplicationUserQueryResult
			{
				PersonInfo = readPersonInfo,
				PasswordPolicy = readPasswordPolicy
			};
		}
	}
}