using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Models.Shared;
using Teleopti.Ccc.WebTest.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class AuthenticationControllerApplicationTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_stubs = new StubFactory();
			_authenticator = _mocks.DynamicMock<IAuthenticator>();
			_businessUnitProvider = _mocks.DynamicMock<IBusinessUnitProvider>();
			_dataSourcesProvider = _mocks.DynamicMock<IDataSourcesProvider>();
			_redirector = MockRepository.GenerateMock<IRedirector>();
			_logOn = _mocks.DynamicMock<IWebLogOn>();
			_viewModelFactory = new AuthenticationViewModelFactory(_dataSourcesProvider, _businessUnitProvider);
			_target = new AuthenticationController(_viewModelFactory, _authenticator, _logOn, null, null, _redirector);
		}

		[TearDown]
		public void Teardown()
		{
			_target.Dispose();
		}

		#endregion

		private MockRepository _mocks;
		private AuthenticationController _target;
		private StubFactory _stubs;
		private IAuthenticator _authenticator;
		private IBusinessUnitProvider _businessUnitProvider;

		private IDataSourcesProvider _dataSourcesProvider;
		private IAuthenticationViewModelFactory _viewModelFactory;
		private IWebLogOn _logOn;
		private IRedirector _redirector;

		[Test]
		public void ShouldLogOnAndRedirctToDefaultWithSingleBusinessUnit()
		{
			const string dataSourceName = "dataSourceForTest";

			var businessUnit = _stubs.BusinessUnitStub("businessunit");
			var person = _stubs.PersonStub();

			var dataSource = _stubs.DataSourceStub(dataSourceName);
			var editModel = new SignInApplicationModel {DataSourceName = "dataSource", UserName = "userName", Password = "pwd"};

			_redirector.Stub(x => x.SignInRedirect()).Return(new RedirectResult("/"));

			using (_mocks.Record())
			{
				Expect.Call(_authenticator.AuthenticateApplicationUser(editModel.DataSourceName, editModel.UserName,
				                                                       editModel.Password)).Return(
				                                                       	new AuthenticateResult
				                                                       		{
				                                                       			HasMessage = false,
				                                                       			Person = person,
				                                                       			Successful = true,
				                                                       			DataSource = dataSource
				                                                       		});
				Expect.Call(_businessUnitProvider.RetrieveBusinessUnitsForPerson(dataSource, person)).Return(new[] {businessUnit});
			}

			using (_mocks.Playback())
			{
				var result = _target.Application(editModel) as RedirectResult;
				result.Url.Should().Be.EqualTo("/");
				//result.RouteValues["controller"].Should().Be.EqualTo(string.Empty);
				//result.RouteValues["action"].Should().Be.EqualTo(string.Empty);
			}
		}

		[Test]
		public void ShouldViewApplicationPartialViewWhenModelStateError()
		{
			var dataSource = _stubs.DataSourceStub("datasource");

			using (_mocks.Record())
			{
				Expect.Call(_dataSourcesProvider.RetrieveDatasourcesForApplication()).Return(new[] {dataSource});
			}

			using (_mocks.Playback())
			{
				_target.ModelState.AddModelError("key", "error message");

				var result = _target.Application(null) as PartialViewResult;
				var viewModel = result.Model as SignInApplicationViewModel;

				result.ViewName.Should().Be.EqualTo("SignInApplicationPartial");
				viewModel.DataSources.Single().Name.Should().Be.EqualTo("datasource");
			}
		}

		[Test]
		public void ShouldViewApplicationPartialViewWithModelStateErrorWhenInvalidCredentials()
		{
			var dataSource = _stubs.DataSourceStub("datasource");

			using (_mocks.Record())
			{
				Expect.Call(_authenticator.AuthenticateApplicationUser("datasource", null, null)).Return(
					new AuthenticateResult {HasMessage = true, Message = "Invalid Win Credentials", Person = null, Successful = false});
				Expect.Call(_dataSourcesProvider.RetrieveDatasourcesForApplication()).Return(new[] {dataSource});
			}
			using (_mocks.Playback())
			{
				var model = new SignInApplicationModel {DataSourceName = "datasource"};
				var result = _target.Application(model) as PartialViewResult;
				var viewModel = result.Model as SignInApplicationViewModel;

				result.ViewName.Should().Be.EqualTo("SignInApplicationPartial");
				viewModel.DataSources.Single().Name.Should().Be.EqualTo("datasource");
				_target.ModelState.ContainsKey("").Should().Be.True();
			}
		}

		[Test]
		public void ShouldViewApplicationPartialViewWithModelStateErrorWhenNoBusinessUnits()
		{
			var dataSource = _stubs.DataSourceStub("datasource");
			var person = _stubs.PersonStub();

			using (_mocks.Record())
			{
				Expect.Call(_authenticator.AuthenticateApplicationUser("datasource", null, null)).Return(
					new AuthenticateResult {HasMessage = false, Person = person, Successful = true, DataSource = dataSource});
				Expect.Call(_businessUnitProvider.RetrieveBusinessUnitsForPerson(dataSource, person)).Return(new IBusinessUnit[] {});
				Expect.Call(_dataSourcesProvider.RetrieveDatasourcesForApplication()).Return(new[] {dataSource});
			}
			using (_mocks.Playback())
			{
				var model = new SignInApplicationModel {DataSourceName = "datasource"};
				var result = _target.Application(model) as PartialViewResult;
				var viewModel = result.Model as SignInApplicationViewModel;

				result.ViewName.Should().Be.EqualTo("SignInApplicationPartial");
				viewModel.DataSources.Single().Name.Should().Be.EqualTo("datasource");
				_target.ModelState.ContainsKey("").Should().Be.True();
			}
		}


		[Test]
		public void ShouldViewErrorPartialViewWithFriendlyMessageWhenLogonFailsWithPermissionException()
		{
			const string password = "Password";
			const string userName = "Username";
			var dataSource = _stubs.DataSourceStub("datasource");
			var person = _stubs.PersonStub();
			var businessUnit = _stubs.BusinessUnitStub("bu");

			var model = new SignInApplicationModel
			            	{DataSourceName = dataSource.DataSourceName, Password = password, UserName = userName};

			using (_mocks.Record())
			{
				Expect.Call(_authenticator.AuthenticateApplicationUser(model.DataSourceName, model.UserName, model.Password)).Return
					(
						new AuthenticateResult {HasMessage = false, Person = person, Successful = true, DataSource = dataSource});
				Expect.Call(_businessUnitProvider.RetrieveBusinessUnitsForPerson(dataSource, person)).Return(new[] {businessUnit});

				Expect.Call(() => _logOn.LogOn(businessUnit.Id.Value, dataSource.DataSourceName, person.Id.Value, AuthenticationTypeOption.Application)).Throw(
					new PermissionException("Permission Exception"));
			}
			using (_mocks.Playback())
			{
				var result = _target.Application(model) as PartialViewResult;

				result.ViewName.Should().Be.EqualTo("ErrorPartial");
				var viewModel = result.Model as ErrorViewModel;
				// Where will this be translated?
				viewModel.Message.Should().Not.Be.Empty();
			}
		}

		[Test]
		public void ShouldViewSignInBusinessUnitPartialViewWhenSuccessfulAuthentication()
		{
			var businessUnitList = new[] {_stubs.BusinessUnitStub("businessunit"), _stubs.BusinessUnitStub("businessunit2")};
			var person = _stubs.PersonStub();
			var dataSource = _stubs.DataSourceStub("datasource");

			using (_mocks.Record())
			{
				Expect.Call(_authenticator.AuthenticateApplicationUser("datasource", "username", "password")).Return(
					new AuthenticateResult {HasMessage = false, Person = person, Successful = true, DataSource = dataSource});
				Expect.Call(_businessUnitProvider.RetrieveBusinessUnitsForPerson(dataSource, person)).Return(businessUnitList);
			}
			using (_mocks.Playback())
			{
				var model = new SignInApplicationModel {DataSourceName = "datasource", UserName = "username", Password = "password"};
				var result = _target.Application(model) as PartialViewResult;
				var viewModel = result.Model as SignInBusinessUnitViewModel;

				result.ViewName.Should().Be.EqualTo("SignInBusinessUnitPartial");
				viewModel.BusinessUnits.Select(x => x.Name).Should().Have.SameValuesAs(businessUnitList.Select(x => x.Name));
				viewModel.BusinessUnits.Select(x => x.Id.ToString()).Should().Have.SameValuesAs(
					businessUnitList.Select(x => x.Id.ToString()));
				viewModel.SignIn.AuthenticationType.Should().Be.EqualTo(AuthenticationTypeOption.Application);
			}
		}
	}
}