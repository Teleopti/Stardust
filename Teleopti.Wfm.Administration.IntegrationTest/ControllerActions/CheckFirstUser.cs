using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class CheckFirstUser
	{
		public DatabaseController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;

		[Test]
		public void ShouldReturnFalseIfNameIsEmpty()
		{
			var result = Target.CheckFirstUser(new CreateTenantModel ()).Content;
			result.Success.Should().Be.False();
			result.Message.Should().Be.EqualTo("The user name can not be empty.");
		}

		[Test]
		public void ShouldReturnFalseIfPasswordIsEmpty()
		{
			var result = Target.CheckFirstUser(new CreateTenantModel { FirstUser = "Myfirstone" }).Content;
			result.Success.Should().Be.False();
			result.Message.Should().Be.EqualTo("The password can not be empty.");
		}

		[Test]
		public void ShouldReturnFalseIfUserExists()
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

			var result = Target.CheckFirstUser(new CreateTenantModel { FirstUser = "Perra", FirstUserPassword = "passande"}).Content;
			result.Success.Should().Be.False();
			result.Message.Should().Be.EqualTo("The user already exists.");
		}

		[Test]
		public void ShouldReturnTrueIfUserNotExists()
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

			var result = Target.CheckFirstUser(new CreateTenantModel { FirstUser = RandomName.Make(), FirstUserPassword = "passande" }).Content;
			result.Success.Should().Be.True();
			result.Message.Should().Be.EqualTo("The user name and password are ok.");
		}

	}
}