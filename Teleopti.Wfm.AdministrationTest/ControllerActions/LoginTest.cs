using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class LoginTest
	{
		public AccountController Target;
		public ITenantUnitOfWork TenantUnitOfWork;

		[Test]
		public void ShouldReturnFalseIfWrongEmail()
		{
			//new fresh
			DataSourceHelper.CreateDatabasesAndDataSource(new NoPersistCallbacks(), "TestData");
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUserModel);
			}
			var model = new LoginModel {GrantType = "password",Password = "passadej", UserName = "olle@teleopti.com"};
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var result = Target.Login(model).Content;
				result.Success.Should().Be.False();
				result.Message.Should().Be.EqualTo("No user found with that email and password.");
			}
		}

		[Test]
		public void ShouldReturnFalseIfWrongPassword()
		{
			//new fresh
			DataSourceHelper.CreateDatabasesAndDataSource(new NoPersistCallbacks(), "TestData");
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUserModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "password", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var result = Target.Login(model).Content;
				result.Success.Should().Be.False();
				result.Message.Should().Be.EqualTo("No user found with that email and password.");
			}
		}

		[Test]
		public void ShouldReturnTrueWhenCorrectLogon()
		{
			//new fresh
			DataSourceHelper.CreateDatabasesAndDataSource(new NoPersistCallbacks(), "TestData");
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var addUserModel = new AddUserModel {ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUserModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "passadej", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var result = Target.Login(model).Content;
				result.Success.Should().Be.True();
				result.AccessToken.Should().Not.Be.Empty();
				result.UserName.Should().Be.EqualTo("Ola");
			}
		}
	}
}