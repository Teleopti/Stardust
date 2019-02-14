using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class LoginTest
	{
		public AccountController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession TenantSession;

		[Test]
		public void ShouldReturnFalseIfWrongEmail()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUserModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "passadej", UserName = "olle@teleopti.com" };
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
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
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
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
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

		[Test]
		public void ShouldSetHangfireCookie()
		{
			var hangfireCookie = new FakeHangfireCookie();
			Target = new AccountController(TenantSession, hangfireCookie, new OneWayEncryption(), new[] { new OneWayEncryption(),}, 
				new AdminAccessTokenRepository(new BCryptHashFunction(), new Now()));
			hangfireCookie.CookieIsSet.Should().Be.False();

			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUserModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "passadej", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var result = Target.Login(model).Content;
				result.Success.Should().Be.True();
			}

			hangfireCookie.CookieIsSet.Should().Be.True();
		}

		[Test]
		public void ShouldNotSetHangfireCookie()
		{
			var hangfireCookie = new FakeHangfireCookie();
			Target = new AccountController(TenantSession, hangfireCookie, new OneWayEncryption(), new []{ new OneWayEncryption()},
				new AdminAccessTokenRepository(new BCryptHashFunction(), new Now()));
			hangfireCookie.CookieIsSet.Should().Be.False();

			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUserModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "wrongPassword", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var result = Target.Login(model).Content;
				result.Success.Should().Be.False();
			}

			hangfireCookie.CookieIsSet.Should().Be.False();
		}

		[Test]
		public void ShouldReturnFalseIfNameOrPasswordMissing()
		{
			var model = new LoginModel { GrantType = "password", Password = "", UserName = "ola@teleopti.com" };

			var result = Target.Login(model).Content;
			result.Success.Should().Be.False();
			result.Message.Should().Be.EqualTo("Both user name and password must be provided.");
			model = new LoginModel { GrantType = "password", Password = "password", UserName = "" };
			result = Target.Login(model).Content;
			result.Success.Should().Be.False();
			result.Message.Should().Be.EqualTo("Both user name and password must be provided.");
		}
		
		[Test]
		public void ShouldReturnAccessTokenAfterSuccessfulLogon()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var addUserModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUserModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "passadej", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var result = Target.Login(model).Content;
				result.Success.Should().Be.True();
				result.AccessToken.Should().Not.Be.NullOrEmpty();
			}
		}
	}
}