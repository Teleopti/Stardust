using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
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
	public class AuthenticationControllerWindowsTest
	{
		private AuthenticationController _target;
		private MockRepository _mocks;
		private IBusinessUnitProvider _businessUnitProvider;
		private IAuthenticator _authenticator;
		private IDataSourcesProvider _dataSourcesProvider;
		private StubFactory _stubs;
		private IAuthenticationViewModelFactory _viewModelFactory;
		private IWebLogOn _logOn;
		private IRedirector _redirector;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_stubs = new StubFactory();
			_dataSourcesProvider = _mocks.DynamicMock<IDataSourcesProvider>();
			_businessUnitProvider = _mocks.DynamicMock<IBusinessUnitProvider>();
			_logOn = _mocks.DynamicMock<IWebLogOn>();
			_authenticator = _mocks.DynamicMock<IAuthenticator>();
			_viewModelFactory = new AuthenticationViewModelFactory(_dataSourcesProvider, _businessUnitProvider);
			_redirector = MockRepository.GenerateMock<IRedirector>();
			_target = new AuthenticationController(_viewModelFactory, _authenticator, _logOn, null, null, _redirector);
		}

		[TearDown]
		public void Teardown()
		{
			_target.Dispose();
		}

		[Test]
		public void ShouldViewSignInBusinessUnitPartialWhenSuccessfulAuthentication()
		{
			var businessUnitList = new[] { _stubs.BusinessUnitStub("businessunit"), _stubs.BusinessUnitStub("businessunitSecound") };
			var person = _stubs.PersonStub();
			var dataSource = _stubs.DataSourceStub("datasource");
			

			using (_mocks.Record())
			{
				Expect.Call(_authenticator.AuthenticateWindowsUser("datasource")).Return(
					new AuthenticateResult { HasMessage = false, Successful = true, Person = person, DataSource = dataSource });
				
				Expect.Call(_businessUnitProvider.RetrieveBusinessUnitsForPerson(dataSource, person)).Return(businessUnitList);
			}

			using (_mocks.Playback())
			{
				var model = new SignInWindowsModel { DataSourceName = "datasource" };
				var result = _target.Windows(model) as PartialViewResult;
				var viewModel = result.Model as SignInBusinessUnitViewModel;

				result.ViewName.Should().Be.EqualTo("SignInBusinessUnitPartial");
				viewModel.BusinessUnits.Select(x => x.Name).Should().Have.SameValuesAs(businessUnitList.Select(x => x.Name));
				viewModel.BusinessUnits.Select(x => x.Id.ToString()).Should().Have.SameValuesAs(businessUnitList.Select(x => x.Id.ToString()));
				viewModel.SignIn.DataSourceName.Should().Be.EqualTo("datasource");
				viewModel.SignIn.PersonId.Should().Be.EqualTo(person.Id);
			}
		}

		[Test]
		public void ShouldLogOnAndRedirctToRootWithSingleBusinessUnit()
		{

			const string dataSourceName = "dataSourceForTest";

			var businessUnit = _stubs.BusinessUnitStub("businessunit");
			var person = _stubs.PersonStub();
			
			var dataSource = _stubs.DataSourceStub(dataSourceName);
			var editModel = new SignInWindowsModel { DataSourceName = dataSourceName };

			_redirector.Stub(x => x.SignInRedirect()).Return(new RedirectResult("/"));

			using (_mocks.Record())
			{
				
				Expect.Call(_authenticator.AuthenticateWindowsUser(dataSourceName)).Return(
					new AuthenticateResult { HasMessage = false, Successful = true, Person = person, DataSource = dataSource });
				Expect.Call(_businessUnitProvider.RetrieveBusinessUnitsForPerson(dataSource, person)).Return(new[] { businessUnit });
				//Expect.Call(_logOn.LogOn(businessUnitId, dataSourceName,personId));
			}

			using (_mocks.Playback())
			{

				var result = _target.Windows(editModel) as RedirectResult;

				result.Url.Should().Be.EqualTo("/");
				/*
				result.RouteValues["controller"].Should().Equals("home");
				result.RouteValues["action"].Should().Equals("index");*/
			}

		}

		[Test]
		public void ShouldViewWindowsPartialViewWithModelStateErrorWhenNoBusinessUnits()
		{
			var dataSource = _stubs.DataSourceStub("datasource");
			var person = _stubs.PersonStub();

			using (_mocks.Record())
			{
				Expect.Call(_authenticator.AuthenticateWindowsUser("datasource"))
					.Return(new AuthenticateResult { Successful = true, DataSource = dataSource, Person = person});
				Expect.Call(_businessUnitProvider.RetrieveBusinessUnitsForPerson(dataSource, person)).Return(new IBusinessUnit[] { });
				Expect.Call(_dataSourcesProvider.RetrieveDatasourcesForWindows()).Return(new[] { dataSource });
			}
			using (_mocks.Playback())
			{
				var model = new SignInWindowsModel { DataSourceName = "datasource" };
				var result = _target.Windows(model) as PartialViewResult;
				var viewModel = result.Model as SignInWindowsViewModel;

				result.ViewName.Should().Be.EqualTo("SignInWindowsPartial");
				viewModel.DataSources.Single().Name.Should().Be.EqualTo("datasource");
				_target.ModelState.ContainsKey("").Should().Be.True();
			}
		}

		[Test]
		public void ShouldViewWindowsPartialViewWhenModelStateError()
		{
			var dataSource = _stubs.DataSourceStub("datasource");

			using (_mocks.Record())
			{
				Expect.Call(_dataSourcesProvider.RetrieveDatasourcesForWindows()).Return(new[] { dataSource });
			}

			using (_mocks.Playback())
			{
				_target.ModelState.AddModelError("key", "error message");

				var result = _target.Windows(null) as PartialViewResult;
				var viewModel = result.Model as SignInWindowsViewModel;

				result.ViewName.Should().Be.EqualTo("SignInWindowsPartial");
				viewModel.DataSources.Single().Name.Should().Be.EqualTo("datasource");
			}
		}


		[Test]
		public void ShouldViewWindowsPartialViewWithModelStateErrorWhenInvalidCredentials()
		{
			var dataSource = _stubs.DataSourceStub("datasource");

			using (_mocks.Record())
			{
				Expect.Call(_authenticator.AuthenticateWindowsUser("datasource")).Return(
					new AuthenticateResult { HasMessage = true, Message = "Invalid Win Credentials", Person = null, Successful = false });
				Expect.Call(_dataSourcesProvider.RetrieveDatasourcesForWindows()).Return(new[] { dataSource });
			}

			using (_mocks.Playback())
			{
				var model = new SignInWindowsModel { DataSourceName = "datasource" };
				var result = _target.Windows(model) as PartialViewResult;
				var viewModel = result.Model as SignInWindowsViewModel;

				result.ViewName.Should().Be.EqualTo("SignInWindowsPartial");
				viewModel.DataSources.Single().Name.Should().Be.EqualTo("datasource");
				_target.ModelState.ContainsKey("").Should().Be.True();
			}
		}

		[Test]
		public void ShouldViewErrorPartialViewWithFriendlyMessageWhenLogonFailsWithPermissionException()
		{

			var dataSource = _stubs.DataSourceStub("datasource");
			var person = _stubs.PersonStub();
			var businessUnit = _stubs.BusinessUnitStub("bu");

			var model = new SignInWindowsModel { DataSourceName = dataSource.DataSourceName };

			using (_mocks.Record())
			{
				Expect.Call(_authenticator.AuthenticateWindowsUser(model.DataSourceName)).Return(
					new AuthenticateResult { HasMessage = false, Person = person, Successful = true, DataSource = dataSource });
				Expect.Call(_businessUnitProvider.RetrieveBusinessUnitsForPerson(dataSource, person)).Return(new[] { businessUnit });

				Expect.Call(() => _logOn.LogOn(businessUnit.Id.Value, dataSource.DataSourceName, person.Id.Value)).Throw(
					new PermissionException("Permission Exception"));

			}
			using (_mocks.Playback())
			{
				var result = _target.Windows(model) as PartialViewResult;

				result.ViewName.Should().Be.EqualTo("ErrorPartial");
				var viewModel = result.Model as ErrorViewModel;
				// Where will this be translated?
				viewModel.Message.Should().Not.Be.Empty();
			}
		}


	}
}