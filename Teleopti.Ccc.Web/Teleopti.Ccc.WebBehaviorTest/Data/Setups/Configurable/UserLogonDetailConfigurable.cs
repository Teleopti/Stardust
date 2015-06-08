using System;
using System.Globalization;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class UserLogonDetailConfigurable : ITenantUserSetup
	{
		public bool? IsLocked { get; set; }
		public int? LastPasswordChangeXDaysAgo { get; set; }


		public void Apply(Tenant tenant, ICurrentTenantSession tenantSession, IPerson user)
		{
			var applicationUserQuery = new FindPersonInfo(tenantSession);
			var personInfo = applicationUserQuery.GetById(user.Id.Value);
			if (IsLocked.HasValue && IsLocked.Value)
			{
				personInfo.ApplicationLogonInfo.Lock();
			}
			if (LastPasswordChangeXDaysAgo.HasValue)
			{
				//hack - should be fixed some other way than manipulating data.
				personInfo.ApplicationLogonInfo.SetLastPasswordChange_OnlyUseFromTests(DateTime.UtcNow.AddDays(-LastPasswordChangeXDaysAgo.Value));
			}
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
		}
	}
}