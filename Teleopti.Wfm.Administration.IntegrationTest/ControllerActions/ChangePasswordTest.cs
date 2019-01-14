using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class ChangePasswordTest
	{
		public AccountController Target;
		public ITenantUnitOfWork TenantUnitOfWork;

		[Test]
		public void ShouldNotWorkWithUnknownId()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			var changeModel = new ChangePasswordModel { Id = 65656565 };
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
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
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			int id;
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUserModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "passadej", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var result = Target.Login(model).Content;
				id = result.Id;
			}
			var changeModel = new ChangePasswordModel {Id = id,OldPassword = "wrongPassword"};
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
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
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			int id;
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUserModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "passadej", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var result = Target.Login(model).Content;
				id = result.Id;
			}
			var changeModel = new ChangePasswordModel { Id = id, OldPassword = "passadej", NewPassword = "myNewPassword", ConfirmNewPassword = "myNewPasword"};
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
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
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			int id;
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUserModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "passadej", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var result = Target.Login(model).Content;
				id = result.Id;
			}
			var changeModel = new ChangePasswordModel { Id = id, OldPassword = "passadej", NewPassword = "myNewPassword", ConfirmNewPassword = "myNewPassword" };
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var changeResult = Target.ChangePassword(changeModel).Content;
				changeResult.Success.Should().Be.True();
				changeResult.Message.Should().Be.EqualTo("Successfully changed password.");
			}
			model.Password = "myNewPassword";
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var result = Target.Login(model).Content;
				result.Success.Should().Be.True();
			}
		}

		[Test]
		public void ShouldEnforceSameLengthOnChangePasswords()
		{
			//new fresh
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			int id;
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUserModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "passadej", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var result = Target.Login(model).Content;
				id = result.Id;
			}
			var changeModel = new ChangePasswordModel { Id = id, OldPassword = "passadej", NewPassword = "short", ConfirmNewPassword = "short" };
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var changeResult = Target.ChangePassword(changeModel).Content;
				changeResult.Success.Should().Be.False();
				changeResult.Message.Should().Be.EqualTo("Password must be at least 6 characters.");
			}
			
		}

	}
}