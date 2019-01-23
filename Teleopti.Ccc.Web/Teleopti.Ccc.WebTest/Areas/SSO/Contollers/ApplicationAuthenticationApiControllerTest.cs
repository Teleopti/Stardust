using System;
using System.Linq;
using System.Web;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.SSO.Controllers;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.SSO.Models;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.SSO.Contollers
{
	[TestFixture]
	public class ApplicationAuthenticationApiControllerTest
	{
		[Test]
		public void ShouldAuthenticateUser()
		{
			var formsAuthentication = MockRepository.GenerateMock<IFormsAuthentication>();
			var authenticator = MockRepository.GenerateMock<ISsoAuthenticator>();
			var dataSource = new FakeDataSource{DataSourceName = RandomName.Make()};
			var result = new ApplicationUserAuthenticateResult { Successful = true, DataSource = dataSource};
			var target = new ApplicationAuthenticationApiController(formsAuthentication, null, null, authenticator, shouldBeLogged("user", result));
			var authenticationModel = new ApplicationAuthenticationModel
			{
				UserName = "user",
				Password = "pwd",
				RememberMe = false,
				IsLogonFromBrowser = true
			};
			authenticator.Stub(
				x => x.AuthenticateApplicationUser(authenticationModel.UserName, authenticationModel.Password)).Return(result);

			target.CheckPassword(authenticationModel);

			formsAuthentication.AssertWasCalled(x => x.SetAuthCookie(authenticationModel.UserName + TokenIdentityProvider.ApplicationIdentifier, authenticationModel.RememberMe, authenticationModel.IsLogonFromBrowser, dataSource.DataSourceName));
		}

		[Test]
		public void ShouldUseMaximumSessionTimeInMinutesFromConfig()
		{
			var formsAuthentication = MockRepository.GenerateMock<IFormsAuthentication>();
			var authenticator = MockRepository.GenerateMock<ISsoAuthenticator>();
			var dataSource = new FakeDataSource { DataSourceName = RandomName.Make() };
			var result = new ApplicationUserAuthenticateResult { Successful = true, DataSource = dataSource };
			var target = new ApplicationAuthenticationApiController(formsAuthentication, null, null, authenticator, shouldBeLogged("user", result));
			var authenticationModel = new ApplicationAuthenticationModel
			{
				UserName = "user",
				Password = "pwd",
				RememberMe = false,
				IsLogonFromBrowser = true
			};
			authenticator.Stub(
				x => x.AuthenticateApplicationUser(authenticationModel.UserName, authenticationModel.Password)).Return(result);

			target.CheckPassword(authenticationModel);

			formsAuthentication.AssertWasCalled(x => x.SetAuthCookie(authenticationModel.UserName + TokenIdentityProvider.ApplicationIdentifier, authenticationModel.RememberMe, authenticationModel.IsLogonFromBrowser, dataSource.DataSourceName));
		}

		[Test]
		public void ShouldReturnErrorIfAuthenticationFailed()
		{
			const string message = "test";
			var authenticator = MockRepository.GenerateMock<ISsoAuthenticator>();
			var authResult = new ApplicationUserAuthenticateResult {Successful = false, Message = message};
			var target = new ApplicationAuthenticationApiController(null, null, null, authenticator, shouldBeLogged(null, authResult));
			var authenticationModel = new ApplicationAuthenticationModel();
			authenticator.Stub(x => x.AuthenticateApplicationUser(authenticationModel.UserName, authenticationModel.Password)).Return(authResult);

			var result = target.CheckPassword(authenticationModel).Result<PasswordWarningViewModel>();

			result.Failed.Should().Be.True();
			result.Message.Should().Be.EqualTo(message);
		}

		[Test]
		public void ShouldReturnWarningIfPasswordExpired()
		{
			const string message = "test";
			var authenticator = MockRepository.GenerateMock<ISsoAuthenticator>();
			var target = new ApplicationAuthenticationApiController(null, null, null, authenticator, shouldNotBeLogged());
			var authenticationModel = new ApplicationAuthenticationModel();
			authenticator.Stub(x => x.AuthenticateApplicationUser(authenticationModel.UserName,authenticationModel.Password))
				.Return(new ApplicationUserAuthenticateResult { Successful = false, Message = message, PasswordExpired = true });

			var result = target.CheckPassword(authenticationModel).Result<PasswordWarningViewModel>();
			result.AlreadyExpired.Should().Be.True();
			result.WillExpireSoon.Should().Be.False();
			result.Failed.Should().Be.False();
			result.Message.Should().Be.NullOrEmpty();
		}

		[Test]
		public void ShouldReturnWarningIfPasswordWillExpire()
		{
			var authenticator = MockRepository.GenerateMock<ISsoAuthenticator>();
			const string message = "test";
			var dataSource = new FakeDataSource { DataSourceName = RandomName.Make() };
			var authResult = new ApplicationUserAuthenticateResult { Successful = true, HasMessage = true, Message = message, DataSource = dataSource };
			var findTenantByNameWithEnsuredTransaction = new FindTenantByNameWithEnsuredTransactionFake();
			findTenantByNameWithEnsuredTransaction.Has(new Tenant(dataSource.DataSourceName));
			var target = new ApplicationAuthenticationApiController(MockRepository.GenerateMock<IFormsAuthentication>(), null, null, authenticator, shouldBeLogged(null, authResult));
			var authenticationModel = new ApplicationAuthenticationModel();
			authenticator.Stub(x => x.AuthenticateApplicationUser(authenticationModel.UserName,authenticationModel.Password))
				.Return(authResult);

			var result = target.CheckPassword(authenticationModel).Result<PasswordWarningViewModel>();
			result.WillExpireSoon.Should().Be.True();
			result.AlreadyExpired.Should().Be.False();
			result.Failed.Should().Be.False();
			result.Message.Should().Be.NullOrEmpty();
		}

		[Test]
		public void ShouldReturnWarningIfPasswordAlreadyExpire()
		{
			var authenticator = MockRepository.GenerateMock<ISsoAuthenticator>();
			const string message = "test";
			var target = new ApplicationAuthenticationApiController(null, null, null, authenticator, shouldNotBeLogged());
			var authenticationModel = new ApplicationAuthenticationModel();
			authenticator.Stub(x => x.AuthenticateApplicationUser(authenticationModel.UserName, authenticationModel.Password))
				.Return(new ApplicationUserAuthenticateResult { Successful = false, HasMessage = true, Message = message, PasswordExpired = true });

			var result = target.CheckPassword(authenticationModel).Result<PasswordWarningViewModel>();
			result.AlreadyExpired.Should().Be.True();
			result.Failed.Should().Be.False();
			result.Message.Should().Be.NullOrEmpty();
		}

		[Test]
		public void ShouldNotReturnWarningIfPasswordWillNotExpire()
		{
			var authenticator = MockRepository.GenerateMock<ISsoAuthenticator>();
			var dataSource = new FakeDataSource { DataSourceName = RandomName.Make() };
			var authResult = new ApplicationUserAuthenticateResult { Successful = true, HasMessage = false, DataSource = dataSource };
			var target = new ApplicationAuthenticationApiController(MockRepository.GenerateMock<IFormsAuthentication>(), null, null, authenticator, shouldBeLogged(null, authResult));
			var authenticationModel = new ApplicationAuthenticationModel();
			authenticator.Stub(x => x.AuthenticateApplicationUser(authenticationModel.UserName, authenticationModel.Password)).Return(authResult);

			var result = target.CheckPassword(authenticationModel).Result<PasswordWarningViewModel>();
			result.WillExpireSoon.Should().Be.False();
			result.Failed.Should().Be.False();
			result.Message.Should().Be.NullOrEmpty();
		}

		[Test]
		public void ShouldRememberMeForTeleoptiIdentityProvider()
		{
			var formsAuthentication = MockRepository.GenerateMock<IFormsAuthentication>();
			var authenticator = MockRepository.GenerateMock<ISsoAuthenticator>();
			var dataSource = new FakeDataSource { DataSourceName = RandomName.Make() };
			var result = new ApplicationUserAuthenticateResult { Successful = true, DataSource = dataSource };
			var target = new ApplicationAuthenticationApiController(formsAuthentication, null, null, authenticator, shouldBeLogged("user", result));
			var authenticationModel = new ApplicationAuthenticationModel
			{
				UserName = "user",
				Password = "pwd",
				RememberMe = true
			};
			authenticator.Stub(
				x => x.AuthenticateApplicationUser(authenticationModel.UserName, authenticationModel.Password)).Return(result);

			target.CheckPassword(authenticationModel);

			formsAuthentication.AssertWasCalled(x => x.SetAuthCookie(authenticationModel.UserName + TokenIdentityProvider.ApplicationIdentifier, authenticationModel.RememberMe, false, dataSource.DataSourceName));
		}

		[Test]
		public void ShouldChangePassword()
		{
			var input = new ChangePasswordInput
				{
					NewPassword = "new",
					OldPassword = "old",
					UserName = RandomName.Make()
				};
			var changePassword = MockRepository.GenerateStub<IPasswordManager>();
			var applicationUserQuery = MockRepository.GenerateStub<IApplicationUserQuery>();
			var personInfo = new PersonInfo(new Tenant(string.Empty), Guid.NewGuid());
			applicationUserQuery.Expect(x => x.Find(input.UserName)).Return(personInfo);

			var target = new ApplicationAuthenticationApiController(MockRepository.GenerateMock<IFormsAuthentication>(), changePassword, applicationUserQuery, null, null);

			target.ChangePassword(input);
			changePassword.AssertWasCalled(x => x.Modify(personInfo.Id, input.OldPassword, input.NewPassword));
		}

		[Test]
		public void ShouldReturnErrorIfFailedChangePassword()
		{
			var input = new ChangePasswordInput
			{
				NewPassword = "new",
				OldPassword = "old",
				UserName = RandomName.Make()
			};

			var applicationUserQuery = MockRepository.GenerateStub<IApplicationUserQuery>();
			var personInfo = new PersonInfo(new Tenant(string.Empty), Guid.NewGuid());
			applicationUserQuery.Expect(x => x.Find(input.UserName)).Return(personInfo);
			
			var changePassword = MockRepository.GenerateStub<IPasswordManager>();
			changePassword.Expect(x => x.Modify(personInfo.Id, input.OldPassword, input.NewPassword))
				.Throw(new HttpException(403, string.Empty));

			var target = new ApplicationAuthenticationApiController(MockRepository.GenerateMock<IFormsAuthentication>(), changePassword, applicationUserQuery, null, null);

			var result = (InvalidModelStateResult)target.ChangePassword(input);
			result.ModelState.Single().Value.Errors.Single().ErrorMessage.Should().Be(Resources.InvalidUserNameOrPassword);
		}

		private static ILogLogonAttempt shouldNotBeLogged()
		{
			return null;
		}

		private static ILogLogonAttempt shouldBeLogged(string userName, ApplicationUserAuthenticateResult result)
		{
			var mock = MockRepository.GenerateMock<ILogLogonAttempt>();
			mock.Expect(x => x.SaveAuthenticateResult(userName, result.PersonId(), result.Successful));
			return mock;
		}
	}
}