﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class ChangePasswordTest
	{
		public AccountController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;

		[Test]
		public void ShouldNotWorkWithUnknownId()
		{
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			var changeModel = new ChangePasswordModel { Id = 65656565 };
			using (TenantUnitOfWork.Start())
			{
				var changeResult = Target.ChangePassword(changeModel).Content;
				changeResult.Success.Should().Be.False();
				changeResult.Message.Should().Be.EqualTo("Can not find the user.");
			}
		}

		[Test]
		public void ShouldNotWorkWithWrongPassword()
		{
			//new fresh
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			int id;
			using (TenantUnitOfWork.Start())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUserModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "passadej", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.Start())
			{
				var result = Target.Login(model).Content;
				id = result.Id;
			}
			var changeModel = new ChangePasswordModel {Id = id,OldPassword = "wrongPassword"};
			using (TenantUnitOfWork.Start())
			{
				var changeResult = Target.ChangePassword(changeModel).Content;
				changeResult.Success.Should().Be.False();
				changeResult.Message.Should().Be.EqualTo("The password is not correct.");
			}
		}

		[Test]
		public void ShouldNotWorkWithWrongConfirmPassword()
		{
			//new fresh
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			int id;
			using (TenantUnitOfWork.Start())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUserModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "passadej", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.Start())
			{
				var result = Target.Login(model).Content;
				id = result.Id;
			}
			var changeModel = new ChangePasswordModel { Id = id, OldPassword = "passadej", NewPassword = "myNewPassword", ConfirmNewPassword = "myNewPasword"};
			using (TenantUnitOfWork.Start())
			{
				var changeResult = Target.ChangePassword(changeModel).Content;
				changeResult.Success.Should().Be.False();
				changeResult.Message.Should().Be.EqualTo("The new password and confirm password does not match.");
			}

			
		}

		[Test]
		public void ShouldWorkWithCorrectPasswords()
		{
			//new fresh
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			int id;
			using (TenantUnitOfWork.Start())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUserModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "passadej", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.Start())
			{
				var result = Target.Login(model).Content;
				id = result.Id;
			}
			var changeModel = new ChangePasswordModel { Id = id, OldPassword = "passadej", NewPassword = "myNewPassword", ConfirmNewPassword = "myNewPassword" };
			using (TenantUnitOfWork.Start())
			{
				var changeResult = Target.ChangePassword(changeModel).Content;
				changeResult.Success.Should().Be.True();
				changeResult.Message.Should().Be.EqualTo("Successfully changed password.");
			}
			model.Password = "myNewPassword";
			using (TenantUnitOfWork.Start())
			{
				var result = Target.Login(model).Content;
				result.Success.Should().Be.True();
			}
		}
	}
}