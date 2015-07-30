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
	public class LoginTest
	{
		public AccountController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		//public ICurrentTenantSession CurrentTenantSession;

		[Test]
		public void ShouldReturnFalseIfWrongEmail()
		{
			//new fresh
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			using (TenantUnitOfWork.Start())
			{
				var addUSerModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUSerModel);
			}
			var model = new LoginModel {GrantType = "password",Password = "passadej", UserName = "olle@teleopti.com"};
			using (TenantUnitOfWork.Start())
			{
				var result = Target.Login(model).Content;
				result.Success.Should().Be.False();
				result.Message.Should().Be.EqualTo("No user with that Email.");
			}
		}

		[Test]
		public void ShouldReturnFalseIfWrongPassword()
		{
			//new fresh
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			using (TenantUnitOfWork.Start())
			{
				var addUSerModel = new AddUserModel { ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUSerModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "password", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.Start())
			{
				var result = Target.Login(model).Content;
				result.Success.Should().Be.False();
				result.Message.Should().Be.EqualTo("The password is not correct.");
			}
		}

		[Test]
		public void ShouldReturnTrueWhenCorrectLogon()
		{
			//new fresh
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			using (TenantUnitOfWork.Start())
			{
				var addUSerModel = new AddUserModel {ConfirmPassword = "passadej", Email = "ola@teleopti.com", Name = "Ola", Password = "passadej" };
				Target.AddUser(addUSerModel);
			}
			var model = new LoginModel { GrantType = "password", Password = "passadej", UserName = "ola@teleopti.com" };
			using (TenantUnitOfWork.Start())
			{
				var result = Target.Login(model).Content;
				result.Success.Should().Be.True();
				result.AccessToken.Should().Not.Be.Empty();
				result.UserName.Should().Be.EqualTo("Ola");
			}
		}
	}
}