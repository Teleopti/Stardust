using System;
using System.Globalization;
using System.Reflection;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class UserLogonDetailConfigurable :  ITenantUserSetup
	{
		public bool? IsLocked { get; set; }
		public int? LastPasswordChangeXDaysAgo { get; set; }


		public void Apply(Tenant tenant, ICurrentTenantSession tenantSession, IPerson user)
		{
			var applicationUserQuery = new ApplicationUserQuery(tenantSession);
			var personInfo = applicationUserQuery.Find(user.ApplicationAuthenticationInfo.ApplicationLogOnName);
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
	}
}