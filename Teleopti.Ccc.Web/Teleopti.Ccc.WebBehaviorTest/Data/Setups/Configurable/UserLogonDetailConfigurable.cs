using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class UserLogonDetailConfigurable : IUserSetup, ITenantUserSetup
	{
		public bool? IsLocked { get; set; }
		public int? LastPasswordChangeXDaysAgo { get; set; }

		public void Apply(ICurrentTenantSession tenantSession, IPerson user, ILogonName logonName)
		{
			var applicationUserQuery = new FindPersonInfo(tenantSession);
			var personInfo = applicationUserQuery.GetById(user.Id.Value);
			if (IsLocked.HasValue && IsLocked.Value)
			{
				personInfo.ApplicationLogonInfo.Lock();
			}
			if (LastPasswordChangeXDaysAgo.HasValue)
			{
				personInfo.ApplicationLogonInfo.SetLastPasswordChange_OnlyUseFromTests(DateTime.UtcNow.AddDays(-LastPasswordChangeXDaysAgo.Value));
			}
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
		}
	}
}