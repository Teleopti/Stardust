﻿using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class SaveUserTest
	{
		public AccountController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;

		[Test]
		public void ShouldUpdate()
		{
			//new fresh
			DataSourceHelper.CreateDatabasesAndDataSource(new NoPersistCallbacks(), "TestData");
			int id;
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var user = new TenantAdminUser {AccessToken = "dummy",Email = "ola@teleopti.com", Name = "Ola", Password = "vadsomhelst"};
				CurrentTenantSession.CurrentSession().Save(user);
				id = user.Id;
			}
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var loaded = Target.GetOneUser(id);
				loaded.Content.Email.Should().Be.EqualTo("ola@teleopti.com");
				Target.SaveUser(new UpdateUserModel {Email = "olle@teleopti.com", Id = id, Name = "Olle"});
			}

			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var loaded = Target.GetOneUser(id);
				loaded.Content.Email.Should().Be.EqualTo("olle@teleopti.com");
			}
		}

		[Test]
		public void ShouldNotAllowEmptyEmail()
		{
			//new fresh
			DataSourceHelper.CreateDatabasesAndDataSource(new NoPersistCallbacks(), "TestData");
			int id;
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var user = new TenantAdminUser { AccessToken = "dummy", Email = "ola@teleopti.com", Name = "Ola", Password = "vadsomhelst" };
				CurrentTenantSession.CurrentSession().Save(user);
				id = user.Id;
			}
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var loaded = Target.GetOneUser(id);
				loaded.Content.Email.Should().Be.EqualTo("ola@teleopti.com");
				var result = Target.SaveUser(new UpdateUserModel { Email = "", Id = id, Name = "Olle" });
				result.Success.Should().Be.False();
				result.Message.Should().Be.EqualTo("Email can't be empty.");
			}
			
		}

		[Test]
		public void ShouldNotAllowEmptyName()
		{
			//new fresh
			DataSourceHelper.CreateDatabasesAndDataSource(new NoPersistCallbacks(), "TestData");
			int id;
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var user = new TenantAdminUser { AccessToken = "dummy", Email = "ola@teleopti.com", Name = "Ola", Password = "vadsomhelst" };
				CurrentTenantSession.CurrentSession().Save(user);
				id = user.Id;
			}
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var loaded = Target.GetOneUser(id);
				loaded.Content.Email.Should().Be.EqualTo("ola@teleopti.com");
				var result = Target.SaveUser(new UpdateUserModel { Email = "ola@teleopti.com", Id = id, Name = "" });
				result.Success.Should().Be.False();
				result.Message.Should().Be.EqualTo("Name can't be empty.");
			}

		}
	}
}