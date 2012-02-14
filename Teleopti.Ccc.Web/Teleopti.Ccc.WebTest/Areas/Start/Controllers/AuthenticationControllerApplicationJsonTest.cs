using System;
using System.Collections.Specialized;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Models.Shared;
using Teleopti.Ccc.WebTest.Core;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class AuthenticationControllerApplicationJsonTest
	{
		private static AuthenticationController MakeAuthenticationControllerWithAcceptJsonHeader(
			IAuthenticationViewModelFactory viewModelFactory, IAuthenticator authenticator, IWebLogOn logOn,
			IRedirector redirector)
		{
			if (logOn == null)
				logOn = MockRepository.GenerateMock<IWebLogOn>();
			var target = new StubbingControllerBuilder()
				.CreateController<AuthenticationController>(
					viewModelFactory, authenticator, logOn, null, null, redirector);
			target.ControllerContext.HttpContext.Request.Stub(x => x.Headers).Return(new NameValueCollection
			                                                                         	{{"Accept", "application/json"}});
			return target;
		}

		[Test]
		public void ShouldLogOnAndReturnRedirectResultWithSingleBusinessUnit()
		{
			var person = new StubFactory().PersonStub();
			var dataSource = new StubFactory().DataSourceStub("datasource");
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var redirector = MockRepository.GenerateMock<IRedirector>();
			var viewModelFactory = MockRepository.GenerateMock<IAuthenticationViewModelFactory>();
			var viewModel = new SignInBusinessUnitViewModel
			                	{
			                		BusinessUnits = new[]
			                		                	{
			                		                		new BusinessUnitViewModel {Id = Guid.NewGuid(), Name = "businessunit"}
			                		                	}
			                	};

			authenticator.Stub(x => x.AuthenticateApplicationUser("datasource", "username", "password")).Return(
				new AuthenticateResult {HasMessage = false, Person = person, Successful = true, DataSource = dataSource});
			viewModelFactory.Stub(x => x.CreateBusinessUnitViewModel(dataSource, person)).Return(viewModel);
			redirector.Stub(x => x.SignInRedirect()).Return(new RedirectResult("/"));

			var target = MakeAuthenticationControllerWithAcceptJsonHeader(viewModelFactory, authenticator, null, redirector);

			var model = new SignInApplicationModel {DataSourceName = "datasource", UserName = "username", Password = "password"};
			var result = target.Application(model) as RedirectResult;

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnErrorViewModelWithFriendlyMessageWhenLogonFailsWithPermissionException()
		{
			var dataSource = new StubFactory().DataSourceStub("datasource");
			var person = new StubFactory().PersonStub();
			var businessunitid = Guid.NewGuid();
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var logOn = MockRepository.GenerateMock<IWebLogOn>();
			var viewModelFactory = MockRepository.GenerateMock<IAuthenticationViewModelFactory>();

			authenticator.Stub(x => x.AuthenticateApplicationUser("datasource", "username", "password")).Return(
				new AuthenticateResult {HasMessage = false, Person = person, Successful = true, DataSource = dataSource});
			viewModelFactory.Stub(x => x.CreateBusinessUnitViewModel(dataSource, person))
				.Return(new SignInBusinessUnitViewModel
				        	{
				        		BusinessUnits = new[]
				        		                	{
				        		                		new BusinessUnitViewModel
				        		                			{
				        		                				Id = businessunitid,
				        		                				Name = "businessunit"
				        		                			}
				        		                	}
				        	});
			logOn.Stub(x => x.LogOn(businessunitid, "datasource", person.Id.Value)).Throw(new PermissionException());

			var target = MakeAuthenticationControllerWithAcceptJsonHeader(
				viewModelFactory, authenticator, logOn, null);

			var model = new SignInApplicationModel
			            	{DataSourceName = dataSource.DataSourceName, UserName = "username", Password = "password"};
			var result = target.Application(model) as JsonResult;
			var data = result.Data as ErrorViewModel;

			data.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnModelStateError()
		{
			var target =
				MakeAuthenticationControllerWithAcceptJsonHeader(MockRepository.GenerateMock<IAuthenticationViewModelFactory>(),
				                                                 null, null, null);
			target.ModelState.AddModelError("key", "error message");

			var result = target.Application(new SignInApplicationModel()) as JsonResult;
			var data = result.Data as ModelStateResult;

			data.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnModelStateErrorWhenInvalidCredentials()
		{
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var target = MakeAuthenticationControllerWithAcceptJsonHeader(
				MockRepository.GenerateMock<IAuthenticationViewModelFactory>(), authenticator, null, null);

			authenticator.Stub(x => x.AuthenticateApplicationUser("datasource", null, null)).Return(
				new AuthenticateResult {HasMessage = true, Message = "Invalid Win Credentials", Person = null, Successful = false});

			var model = new SignInApplicationModel {DataSourceName = "datasource"};
			var result = target.Application(model) as JsonResult;
			var data = result.Data as ModelStateResult;

			data.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnModelStateErrorWhenNoBusinessUnits()
		{
			var dataSource = new StubFactory().DataSourceStub("datasource");
			var person = new StubFactory().PersonStub();
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var viewModelFactory = MockRepository.GenerateMock<IAuthenticationViewModelFactory>();
			var viewModel = new SignInBusinessUnitViewModel {BusinessUnits = new BusinessUnitViewModel[] {}};

			authenticator.Stub(x => x.AuthenticateApplicationUser("datasource", null, null)).Return(
				new AuthenticateResult {HasMessage = false, Person = person, Successful = true, DataSource = dataSource});
			viewModelFactory.Stub(x => x.CreateBusinessUnitViewModel(dataSource, person)).Return(viewModel);

			var target = MakeAuthenticationControllerWithAcceptJsonHeader(viewModelFactory, authenticator, null, null);

			var model = new SignInApplicationModel {DataSourceName = "datasource"};
			var result = target.Application(model) as JsonResult;
			var data = result.Data as ModelStateResult;

			data.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnSignInBusinessUnitViewModelWhenSuccessfulAuthentication()
		{
			var person = new StubFactory().PersonStub();
			var dataSource = new StubFactory().DataSourceStub("datasource");
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var viewModelFactory = MockRepository.GenerateMock<IAuthenticationViewModelFactory>();
			var viewModel = new SignInBusinessUnitViewModel
			                	{
			                		BusinessUnits = new[]
			                		                	{
			                		                		new BusinessUnitViewModel {Id = Guid.NewGuid(), Name = "businessunit"},
			                		                		new BusinessUnitViewModel {Id = Guid.NewGuid(), Name = "businessunit2"}
			                		                	}
			                	};

			authenticator.Stub(x => x.AuthenticateApplicationUser("datasource", "username", "password")).Return(
				new AuthenticateResult {HasMessage = false, Person = person, Successful = true, DataSource = dataSource});
			viewModelFactory.Stub(x => x.CreateBusinessUnitViewModel(dataSource, person)).Return(viewModel);

			var target = MakeAuthenticationControllerWithAcceptJsonHeader(viewModelFactory, authenticator, null, null);

			var model = new SignInApplicationModel {DataSourceName = "datasource", UserName = "username", Password = "password"};
			var result = target.Application(model) as JsonResult;
			var data = result.Data as SignInBusinessUnitViewModel;

			data.Should().Not.Be.Null();
		}
	}
}