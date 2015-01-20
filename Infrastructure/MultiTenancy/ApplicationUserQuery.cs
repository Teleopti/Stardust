using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class ApplicationUserQuery : IApplicationUserQuery
	{
		private readonly Func<ICurrentTennantSession> _currentTennantSession;

		private const string hqlPersonInfo = @"
select pi from PersonInfo pi
where pi.applicationLogonName=:userName
and (pi.terminalDate is null or pi.terminalDate>getdate())
";
		private const string hqlPasswordPolicy = @"
select pp from PasswordPolicyForUser pp
where personInfo=:personInfo
";

		//remove "func" when we later move away from list of datasources
		public ApplicationUserQuery(Func<ICurrentTennantSession> currentTennantSession)
		{
			_currentTennantSession = currentTennantSession;
		}

		public ApplicationUserQueryResult FindUserData(string userName)
		{
			var session = _currentTennantSession().Session();
			var readPersonInfo = session.CreateQuery(hqlPersonInfo)
				.SetString("userName", userName)
				.UniqueResult<PersonInfo>();
			if (readPersonInfo == null)
			{
				return null;
			}
			var readPasswordPolicy = session.CreateQuery(hqlPasswordPolicy)
				.SetEntity("personInfo", readPersonInfo)
				.UniqueResult<PasswordPolicyForUser>();
			var ret = new ApplicationUserQueryResult
			{
				PersonInfo = readPersonInfo,
				PasswordPolicy = readPasswordPolicy
			};
			return ret;
		}
	}
}