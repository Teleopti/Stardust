using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class AddUserTest
	{
		public AccountController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;
		public ILoadAllTenants LoadAllTenants;

		[Test]
		public void ShouldActivateAllTenantsWhenFirstUserIsAdded()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant("Old One") {Active = false};
				CurrentTenantSession.CurrentSession().Save(tenant);
				CurrentTenantSession.CurrentSession().Delete("select u from TenantAdminUser u");

				var inactiveOnes = LoadAllTenants.Tenants().Where(t => t.Active.Equals(false));
				inactiveOnes.Should().Not.Be.Empty();
			}
			Target.AddFirstUser(new AddUserModel
			{
				Email = "auser@somecompany.com",
				Name = "FirstUser",
				Password = "aGoodPassword12",
				ConfirmPassword = "aGoodPassword12"
			});

			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var inactiveOnes = LoadAllTenants.Tenants().Where(t => t.Active.Equals(false));
				inactiveOnes.Should().Be.Empty();
			}
		}

		[Test]
		public void ShouldNotActivateAllTenantsWhenFirstUserIsAddedWithNonMatchingPasswords()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant("Old One") { Active = false };
				CurrentTenantSession.CurrentSession().Save(tenant);
				CurrentTenantSession.CurrentSession().Delete("select u from TenantAdminUser u");

				var inactiveOnes = LoadAllTenants.Tenants().Where(t => t.Active.Equals(false));
				inactiveOnes.Should().Not.Be.Empty();
			}
			Target.AddFirstUser(new AddUserModel
			{
				Email = "auser@somecompany.com",
				Name = "FirstUser",
				Password = "aGoodPassword12",
				ConfirmPassword = "aGodPassword12"
			});

			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var inactiveOnes = LoadAllTenants.Tenants().Where(t => t.Active.Equals(false));
				inactiveOnes.Should().Not.Be.Empty();
			}
		}
	}
}