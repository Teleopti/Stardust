﻿using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class AddUserTest
	{
		public AccountController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;
		public ILoadAllTenants LoadAllTenants;

		[Test]
		public void ShouldActivateAllTenantsWhenFirstUserIsAdded()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(new NoTransactionHooks(), "TestData");
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant("Old One") {Active = false};
				CurrentTenantSession.CurrentSession().Save(tenant);
			}
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
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
				inactiveOnes.Should().Be.Empty();
			}
		}
	}
}