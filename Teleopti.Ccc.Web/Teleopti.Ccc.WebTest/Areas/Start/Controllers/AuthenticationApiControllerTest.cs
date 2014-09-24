using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class AuthenticationApiControllerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldRetrieveDataSources()
		{
			var dataSourcesViewModelFactory = MockRepository.GenerateMock<IDataSourcesViewModelFactory>();
			var target = new AuthenticationApiController(dataSourcesViewModelFactory, null, null);
			var dataSourcees = new[] { new DataSourceViewModel() };
			dataSourcesViewModelFactory.Stub(x => x.DataSources()).Return(dataSourcees);

			var result = target.DataSources();

			var data = result.Data as IEnumerable<DataSourceViewModel>;
			data.Should().Have.SameSequenceAs(dataSourcees);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldAuthenticateUserRetrievingBusinessUnits()
		{
			var target = new AuthenticationApiController(null, MockRepository.GenerateMock<IBusinessUnitsViewModelFactory>(), null);
			var authenticationModel = MockRepository.GenerateMock<IAuthenticationModel>();
			authenticationModel.Stub(x => x.AuthenticateUser()).Return(new AuthenticateResult {Successful = true});

			target.BusinessUnits(authenticationModel);

			authenticationModel.AssertWasCalled(x => x.AuthenticateUser());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldRetrieveBusinessUnits()
		{
			var authenticationResult = new AuthenticateResult
				{
					Successful = true,
					DataSource = new FakeDataSource(),
					Person = new Person()
				};
			var authenticationModel = MockRepository.GenerateMock<IAuthenticationModel>();
			authenticationModel.Stub(x => x.AuthenticateUser()).Return(authenticationResult);
			var businessUnitViewModels = new[] {new BusinessUnitViewModel()};
			var businessUnitViewModelFactory = MockRepository.GenerateMock<IBusinessUnitsViewModelFactory>();
			businessUnitViewModelFactory.Stub(x => x.BusinessUnits(authenticationResult.DataSource, authenticationResult.Person)).Return(businessUnitViewModels);
			var target = new AuthenticationApiController(null, businessUnitViewModelFactory, null);

			var result = target.BusinessUnits(authenticationModel);

			result.Data.Should().Be.SameInstanceAs(businessUnitViewModels);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnErrorIfAuthenticationUnsuccessfulRetrievingBusinessUnits()
		{
			var target = new StubbingControllerBuilder().CreateController<AuthenticationApiController>(null, null, null);
			var authenticationModel = MockRepository.GenerateMock<IAuthenticationModel>();
			const string message = "test";
			authenticationModel.Stub(x => x.AuthenticateUser()).Return(new AuthenticateResult { Successful = false, Message = message });

			var result = target.BusinessUnits(authenticationModel);

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(message);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(message);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldAuthenticateUserOnLogon()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var target = new AuthenticationApiController(null, MockRepository.GenerateMock<IBusinessUnitsViewModelFactory>(), MockRepository.GenerateMock<IWebLogOn>());
			var authenticationModel = MockRepository.GenerateMock<IAuthenticationModel>();
			var result = new AuthenticateResult
				{
					Successful = true,
					DataSource = new FakeDataSource(),
					Person = person
				};
			authenticationModel.Stub(x => x.AuthenticateUser()).Return(result);
			authenticationModel.Stub(x => x.SaveAuthenticateResult(result));
			target.Logon(authenticationModel, Guid.NewGuid());

			authenticationModel.AssertWasCalled(x => x.AuthenticateUser());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldLogon()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var businessUnitId = Guid.NewGuid();
			var authenticationModel = MockRepository.GenerateMock<IAuthenticationModel>();
			authenticationModel.Stub(x => x.AuthenticateUser()).Return(new AuthenticateResult
				{
					Successful = true,
					DataSource = new FakeDataSource {DataSourceName = "datasource"},
					Person = person
				});
			var webLogon = MockRepository.GenerateMock<IWebLogOn>();
			var target = new AuthenticationApiController(null, null, webLogon);

			target.Logon(authenticationModel, businessUnitId);

			webLogon.AssertWasCalled(x => x.LogOn("datasource", businessUnitId, person.Id.Value));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnErrorIfAuthenticationUnsuccessfulLoggingOn()
		{
			var target = new StubbingControllerBuilder().CreateController<AuthenticationApiController>(null, null, null);
			var authenticationModel = MockRepository.GenerateMock<IAuthenticationModel>();
			authenticationModel.Stub(x => x.AuthenticateUser()).Return(new AuthenticateResult { Successful = false });

			var result = target.Logon(authenticationModel, Guid.NewGuid());

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnLicenseErrorIfAuthenticationUnsuccessfulLoggingOn()
		{
			var target = new StubbingControllerBuilder().CreateController<AuthenticationApiController>(null, null, null);
			var authenticationModel = MockRepository.GenerateMock<IAuthenticationModel>();
			authenticationModel.Stub(x => x.AuthenticateUser()).Throw(new LicenseMissingException());

			var result = target.Logon(authenticationModel, Guid.NewGuid());

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(Resources.TeleoptiLicenseException);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(Resources.TeleoptiLicenseException);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnErrorIfNoPermission()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var businessUnitId = Guid.NewGuid();
			var authenticationModel = MockRepository.GenerateMock<IAuthenticationModel>();
			authenticationModel.Stub(x => x.AuthenticateUser()).Return(new AuthenticateResult
			{
				Successful = true,
				DataSource = new FakeDataSource { DataSourceName = "datasource" },
				Person = person
			});
			var webLogon = MockRepository.GenerateMock<IWebLogOn>();
			webLogon.Stub(x => x.LogOn("datasource", businessUnitId, person.Id.Value)).Throw(new PermissionException());
			var target = new StubbingControllerBuilder().CreateController<AuthenticationApiController>(null, null, webLogon);

			var result = target.Logon(authenticationModel, businessUnitId);

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(Resources.InsufficientPermissionForWeb);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(Resources.InsufficientPermissionForWeb);
			
		}

	}
}