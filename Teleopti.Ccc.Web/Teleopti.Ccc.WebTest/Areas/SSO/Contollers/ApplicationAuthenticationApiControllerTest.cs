using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.SSO.Controllers;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.SSO.Models;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.SSO.Contollers
{
	[TestFixture]
	public class ApplicationAuthenticationApiControllerTest
	{
		private const string dataSourceName = "dataSource";
		private const string userName = "user";
		private IRepositoryFactory repositoryFactory;
		private IApplicationData applicationData;
		private IPerson person;
		private ILoadPasswordPolicyService loadPasswordPolicyService;
		private IUserDetail userDetail;
		private IPersonRepository personRepository;
		private ICurrentPrincipalContext currentPrincipalContext;

		[SetUp]
		public void Setup()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			applicationData = MockRepository.GenerateMock<IApplicationData>();
			applicationData.Stub(x => x.DataSource(dataSourceName)).Return(dataSource);
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			dataSource.Stub(x => x.Application).Return(uowFactory);
			uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			person = MockRepository.GenerateMock<IPerson>();
			repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRepository);
			var userDetailRepository = MockRepository.GenerateMock<IUserDetailRepository>();
			userDetail = new UserDetail(person);
			userDetailRepository.Stub(x => x.FindByUser(person)).Return(userDetail);
			repositoryFactory.Stub(x => x.CreateUserDetailRepository(uow)).Return(userDetailRepository);
			loadPasswordPolicyService = MockRepository.GenerateMock<ILoadPasswordPolicyService>();
			loadPasswordPolicyService.Stub(x => x.LoadPasswordStrengthRules()).Return(new List<IPasswordStrengthRule>());
			currentPrincipalContext = MockRepository.GenerateMock<ICurrentPrincipalContext>();
		}

		[Test]
		public void ShouldAuthenticateUser()
		{
			var formsAuthentication = MockRepository.GenerateMock<IFormsAuthentication>();
			var target = new ApplicationAuthenticationApiController(null, null, null, null, formsAuthentication, null);
			var authenticator = MockRepository.GenerateMock<ISsoAuthenticator>();
			var result = new AuthenticateResult { Successful = true, DataSource = new FakeDataSource{DataSourceName = dataSourceName}};
			var authenticationModel = new ApplicationAuthenticationModel(authenticator, shouldBeLogged("user", result))
			{
				UserName = "user",
				Password = "pwd"
			};
			authenticator.Stub(
				x => x.AuthenticateApplicationUser(authenticationModel.UserName, authenticationModel.Password)).Return(result);

			target.CheckPassword(authenticationModel);

			formsAuthentication.AssertWasCalled(x => x.SetAuthCookie(authenticationModel.UserName + TokenIdentityProvider.ApplicationIdentifier));
		}

		[Test]
		public void ShouldReturnErrorIfAuthenticationFailed()
		{
			var target = new StubbingControllerBuilder().CreateController<ApplicationAuthenticationApiController>(null, null, null, null, null, null);
			const string message = "test";
			var authenticator = MockRepository.GenerateMock<ISsoAuthenticator>();
			var authResult = new AuthenticateResult {Successful = false, Message = message};
			var authenticationModel = new ApplicationAuthenticationModel(authenticator, shouldBeLogged(null, authResult));
			authenticator.Stub(x => x.AuthenticateApplicationUser(authenticationModel.UserName, authenticationModel.Password)).Return(authResult);

			var result = target.CheckPassword(authenticationModel);

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(message);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(message);
		}

		[Test]
		public void ShouldReturnWarningIfPasswordExpired()
		{
			var target = new ApplicationAuthenticationApiController(null, null, null, null, null, null);
			const string message = "test";
			var authenticator = MockRepository.GenerateMock<ISsoAuthenticator>();
			var authenticationModel = new ApplicationAuthenticationModel(authenticator, shouldNotBeLogged());
			authenticator.Stub(x => x.AuthenticateApplicationUser(authenticationModel.UserName,authenticationModel.Password))
				.Return(new AuthenticateResult { Successful = false, Message = message, PasswordExpired = true });

			var result = target.CheckPassword(authenticationModel);

			var warning = result.Data as PasswordWarningViewModel;
			warning.AlreadyExpired.Should().Be.True();
			warning.WillExpireSoon.Should().Be.False();
		}

		[Test]
		public void ShouldReturnWarningIfPasswordWillExpire()
		{
			var target = new ApplicationAuthenticationApiController(null, null, null, null, MockRepository.GenerateMock<IFormsAuthentication>(), null);
			var authenticator = MockRepository.GenerateMock<ISsoAuthenticator>();
			const string message = "test";
			var authResult = new AuthenticateResult { Successful = true, HasMessage = true, Message = message, DataSource = new FakeDataSource { DataSourceName = dataSourceName } };
			var authenticationModel = new ApplicationAuthenticationModel(authenticator, shouldBeLogged(null, authResult));
			authenticator.Stub(x => x.AuthenticateApplicationUser(authenticationModel.UserName,authenticationModel.Password))
				.Return(authResult);

			var result = target.CheckPassword(authenticationModel);

			var warning = result.Data as PasswordWarningViewModel;
			warning.WillExpireSoon.Should().Be.True();
			warning.AlreadyExpired.Should().Be.False();
		}

		[Test]
		public void ShouldReturnWarningIfPasswordAlreadyExpire()
		{
			var target = new ApplicationAuthenticationApiController(null, null, null, null, null, null);
			var authenticator = MockRepository.GenerateMock<ISsoAuthenticator>();
			const string message = "test";
			var authenticationModel = new ApplicationAuthenticationModel(authenticator, shouldNotBeLogged());
			authenticator.Stub(x => x.AuthenticateApplicationUser(authenticationModel.UserName, authenticationModel.Password))
				.Return(new AuthenticateResult { Successful = false, HasMessage = true, Message = message, PasswordExpired = true });

			var result = target.CheckPassword(authenticationModel);

			var warning = result.Data as PasswordWarningViewModel;
			warning.AlreadyExpired.Should().Be.True();
		}

		[Test]
		public void ShouldNotReturnWarningIfPasswordWillNotExpire()
		{
			var target = new ApplicationAuthenticationApiController(null, null, null, null, MockRepository.GenerateMock<IFormsAuthentication>(), null);
			var authenticator = MockRepository.GenerateMock<ISsoAuthenticator>();
			var authResult = new AuthenticateResult {Successful = true, HasMessage = false, DataSource = new FakeDataSource{DataSourceName = dataSourceName}};
			var authenticationModel = new ApplicationAuthenticationModel(authenticator, shouldBeLogged(null, authResult));
			authenticator.Stub(x => x.AuthenticateApplicationUser(authenticationModel.UserName, authenticationModel.Password)).Return(authResult);

			var result = target.CheckPassword(authenticationModel);

			var warning = result.Data as PasswordWarningViewModel;
			warning.WillExpireSoon.Should().Be.False();
		}

		[Test]
		public void ShouldChangePassword()
		{
			var input = new ChangePasswordInput
				{
					NewPassword = "new",
					OldPassword = "old",
					UserName = userName
				};
			person.Stub(x => x.ChangePassword(input.OldPassword, input.NewPassword, loadPasswordPolicyService, userDetail)).Return(
				new ChangePasswordResultInfo
				{
					IsSuccessful = true
				});
			var pInfo = new PersonInfo(new Tenant(dataSourceName), Guid.NewGuid());
			personRepository.Stub(x => x.LoadPersonAndPermissions(pInfo.Id)).Return(person);
			var applicationUserTenantQuery = MockRepository.GenerateMock<IApplicationUserTenantQuery>();
			applicationUserTenantQuery.Stub(x => x.Find(userName)).Return(pInfo);

			var target = new ApplicationAuthenticationApiController(applicationData, repositoryFactory, loadPasswordPolicyService, currentPrincipalContext, MockRepository.GenerateMock<IFormsAuthentication>(), applicationUserTenantQuery);

			var result = target.ChangePassword(input);

			var resultData = result.Data as IChangePasswordResultInfo;
			resultData.IsSuccessful.Should().Be.True();
		}

		[Test]
		public void ShouldThrowExceptionIfPersonNotFound()
		{
			var input = new ChangePasswordInput
			{
				NewPassword = "new",
				OldPassword = "old",
				UserName = userName
			};
			person.Stub(x => x.ChangePassword(input.OldPassword, input.NewPassword, loadPasswordPolicyService, userDetail)).Return(
				new ChangePasswordResultInfo
				{
					IsSuccessful = true
				});
			var applicationUserTenantQuery = MockRepository.GenerateMock<IApplicationUserTenantQuery>();
			applicationUserTenantQuery.Stub(x => x.Find(userName)).Return(null);
			var target = new StubbingControllerBuilder().CreateController<ApplicationAuthenticationApiController>(applicationData, repositoryFactory, loadPasswordPolicyService, null, null, applicationUserTenantQuery);

			var exception = Assert.Throws<HttpException>(() => target.ChangePassword(input));
			exception.GetHttpCode().Should().Be(500);
		}

		[Test]
		public void ShouldReturnErrorIfFailedChangePassword()
		{
			var input = new ChangePasswordInput
			{
				NewPassword = "new",
				OldPassword = "old",
				UserName = userName
			};
			person.Stub(x => x.ChangePassword(input.OldPassword, input.NewPassword, loadPasswordPolicyService, userDetail)).Return(
				new ChangePasswordResultInfo
				{
					IsSuccessful = false
				});
			var pInfo = new PersonInfo(new Tenant(dataSourceName), Guid.NewGuid());
			personRepository.Stub(x => x.LoadPersonAndPermissions(pInfo.Id)).Return(person);
			var applicationUserTenantQuery = MockRepository.GenerateMock<IApplicationUserTenantQuery>();
			applicationUserTenantQuery.Stub(x => x.Find(userName)).Return(pInfo);

			var target = new StubbingControllerBuilder().CreateController<ApplicationAuthenticationApiController>(applicationData, repositoryFactory, loadPasswordPolicyService, currentPrincipalContext, MockRepository.GenerateMock<IFormsAuthentication>(), applicationUserTenantQuery);

			var result = target.ChangePassword(input);

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(Resources.InvalidUserNameOrPassword);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(Resources.InvalidUserNameOrPassword);
		}

		private static ILogLogonAttempt shouldNotBeLogged()
		{
			return null;
		}

		private static ILogLogonAttempt shouldBeLogged(string userName, AuthenticateResult result)
		{
			var mock = MockRepository.GenerateMock<ILogLogonAttempt>();
			mock.Expect(x => x.SaveAuthenticateResult(userName, result.PersonId(), result.Successful));
			return mock;
		}
	}
}