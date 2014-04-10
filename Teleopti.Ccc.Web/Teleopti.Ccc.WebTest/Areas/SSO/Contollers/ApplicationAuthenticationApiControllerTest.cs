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
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.SSO.Controllers;
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
		private IDataSourcesProvider dataSourcesProvider;
		private IPerson person;
		private ILoadPasswordPolicyService loadPasswordPolicyService;
		private IUserDetail userDetail;
		private IPersonRepository personRepository;
		private ICurrentPrincipalContext currentPrincipalContext;

		[SetUp]
		public void Setup()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			dataSourcesProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
			dataSourcesProvider.Stub(x => x.RetrieveDataSourceByName(dataSourceName)).Return(dataSource);
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldAuthenticateUser()
		{
			var formsAuthentication = MockRepository.GenerateMock<IFormsAuthentication>();
			var target = new ApplicationAuthenticationApiController(null, null, null, null, formsAuthentication);
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var authenticationModel = new ApplicationAuthenticationModel(authenticator)
			{
				DataSourceName = "data source",
				UserName = "user",
				Password = "pwd"
			};
			var result = new AuthenticateResult {Successful = true};
			authenticator.Stub(
				x => x.AuthenticateApplicationUser(authenticationModel.DataSourceName, authenticationModel.UserName,
					authenticationModel.Password)).Return(result);

			target.CheckPassword(authenticationModel);

			formsAuthentication.AssertWasCalled(x => x.SetAuthCookie(authenticationModel.UserName + "¡ì" + authenticationModel.DataSourceName));
		}

		[Test]
		public void ShouldReturnErrorIfAuthenticationFailed()
		{
			var target = new StubbingControllerBuilder().CreateController<ApplicationAuthenticationApiController>(null, null, null, null, null);
			const string message = "test";
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var authenticationModel = new ApplicationAuthenticationModel(authenticator);
			authenticator.Stub(
				x => x.AuthenticateApplicationUser(authenticationModel.DataSourceName, authenticationModel.UserName,
					authenticationModel.Password)).Return(new AuthenticateResult { Successful = false, Message = message });

			var result = target.CheckPassword(authenticationModel);

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(message);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(message);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnWarningIfPasswordExpired()
		{
			var target = new ApplicationAuthenticationApiController(null, null, null, null, null);
			const string message = "test";
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var authenticationModel = new ApplicationAuthenticationModel(authenticator);
			authenticator.Stub(
				x => x.AuthenticateApplicationUser(authenticationModel.DataSourceName, authenticationModel.UserName,
					authenticationModel.Password)).Return(new AuthenticateResult { Successful = false, Message = message, PasswordExpired = true });

			var result = target.CheckPassword(authenticationModel);

			var warning = result.Data as PasswordWarningViewModel;
			warning.AlreadyExpired.Should().Be.True();
			warning.WillExpireSoon.Should().Be.False();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnWarningIfPasswordWillExpire()
		{
			var target = new ApplicationAuthenticationApiController(null, null, null, null, MockRepository.GenerateMock<IFormsAuthentication>());
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			const string message = "test";
			var authenticationModel = new ApplicationAuthenticationModel(authenticator);
			authenticator.Stub(
				x => x.AuthenticateApplicationUser(authenticationModel.DataSourceName, authenticationModel.UserName,
					authenticationModel.Password)).Return(new AuthenticateResult { Successful = true, HasMessage = true, Message = message });

			var result = target.CheckPassword(authenticationModel);

			var warning = result.Data as PasswordWarningViewModel;
			warning.WillExpireSoon.Should().Be.True();
			warning.AlreadyExpired.Should().Be.False();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnWarningIfPasswordAlreadyExpire()
		{
			var target = new ApplicationAuthenticationApiController(null, null, null, null, null);
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			const string message = "test";
			var authenticationModel = new ApplicationAuthenticationModel(authenticator);
			authenticator.Stub(
				x => x.AuthenticateApplicationUser(authenticationModel.DataSourceName, authenticationModel.UserName,
					authenticationModel.Password)).Return(new AuthenticateResult { Successful = false, HasMessage = true, Message = message, PasswordExpired = true });

			var result = target.CheckPassword(authenticationModel);

			var warning = result.Data as PasswordWarningViewModel;
			warning.AlreadyExpired.Should().Be.True();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldNotReturnWarningIfPasswordWillNotExpire()
		{
			var target = new ApplicationAuthenticationApiController(null, null, null, null, MockRepository.GenerateMock<IFormsAuthentication>());
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var authenticationModel = new ApplicationAuthenticationModel(authenticator);
			authenticator.Stub(
				x => x.AuthenticateApplicationUser(authenticationModel.DataSourceName, authenticationModel.UserName,
					authenticationModel.Password)).Return(new AuthenticateResult { Successful = true, HasMessage = false });

			var result = target.CheckPassword(authenticationModel);

			var warning = result.Data as PasswordWarningViewModel;
			warning.WillExpireSoon.Should().Be.False();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldChangePassword()
		{
			var input = new ChangePasswordInput
				{
					DataSourceName = dataSourceName,
					NewPassword = "new",
					OldPassword = "old",
					UserName = userName
				};
			person.Stub(x => x.ChangePassword(input.OldPassword, input.NewPassword, loadPasswordPolicyService, userDetail)).Return(
				new ChangePasswordResultInfo
				{
					IsSuccessful = true
				});
			personRepository.Stub(x => x.TryFindBasicAuthenticatedPerson(userName)).Return(person);
			var target = new ApplicationAuthenticationApiController(dataSourcesProvider, repositoryFactory, loadPasswordPolicyService, currentPrincipalContext, MockRepository.GenerateMock<IFormsAuthentication>());

			var result = target.ChangePassword(input);

			var resultData = result.Data as IChangePasswordResultInfo;
			resultData.IsSuccessful.Should().Be.True();
		}

		[Test]
		public void ShouldThrowExceptionIfPersonNotFound()
		{
			var input = new ChangePasswordInput
			{
				DataSourceName = dataSourceName,
				NewPassword = "new",
				OldPassword = "old",
				UserName = userName
			};
			person.Stub(x => x.ChangePassword(input.OldPassword, input.NewPassword, loadPasswordPolicyService, userDetail)).Return(
				new ChangePasswordResultInfo
				{
					IsSuccessful = true
				});
			personRepository.Stub(x => x.TryFindBasicAuthenticatedPerson(userName)).Return(null);
			var target = new StubbingControllerBuilder().CreateController<ApplicationAuthenticationApiController>(dataSourcesProvider, repositoryFactory, loadPasswordPolicyService, null, null);

			var exception = Assert.Throws<HttpException>(() => target.ChangePassword(input));
			exception.GetHttpCode().Should().Be(500);
		}

		[Test]
		public void ShouldReturnErrorIfFailedChangePassword()
		{
			var input = new ChangePasswordInput
			{
				DataSourceName = dataSourceName,
				NewPassword = "new",
				OldPassword = "old",
				UserName = userName
			};
			person.Stub(x => x.ChangePassword(input.OldPassword, input.NewPassword, loadPasswordPolicyService, userDetail)).Return(
				new ChangePasswordResultInfo
				{
					IsSuccessful = false
				});
			personRepository.Stub(x => x.TryFindBasicAuthenticatedPerson(userName)).Return(person);
			var target = new StubbingControllerBuilder().CreateController<ApplicationAuthenticationApiController>(dataSourcesProvider, repositoryFactory, loadPasswordPolicyService, currentPrincipalContext, MockRepository.GenerateMock<IFormsAuthentication>());

			var result = target.ChangePassword(input);

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(Resources.InvalidUserNameOrPassword);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(Resources.InvalidUserNameOrPassword);
		}
	}
}