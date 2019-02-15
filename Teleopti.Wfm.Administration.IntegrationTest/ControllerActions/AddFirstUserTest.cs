using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class AddFirstUserTest
	{
		public AccountController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;
		public ILoadAllTenants LoadAllTenants;

		[Test]
		public void ShouldBlockAddFirstUserOnceOneUserExists()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			var tenant = new Tenant("Tenn");

			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				CurrentTenantSession.CurrentSession().Save(tenant);
				var personInfo = new PersonInfo(tenant, Guid.NewGuid());
				personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), "Perra", "passadej", new OneWayEncryption());
				CurrentTenantSession.CurrentSession().Save(personInfo);
			}

			var result = Target.AddFirstUser(new AddUserModel
			{
				Email = "theuser@somecompany.com",
				Name = "TheFirstUser",
				Password = "aGoodPassword12",
				ConfirmPassword = "aGoodPassword12"
			});

			result.Success.Should().Be.True();

			result = Target.AddFirstUser(new AddUserModel
			{
				Email = "auser@somecompany.com",
				Name = "FirstUser",
				Password = "aGoodPassword12",
				ConfirmPassword = "aGoodPassword12"
			});

			result.Success.Should().Be.False();
		}
	}
}