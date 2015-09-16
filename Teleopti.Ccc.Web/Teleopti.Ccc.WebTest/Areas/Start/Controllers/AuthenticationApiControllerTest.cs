using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class AuthenticationApiControllerTest
	{
		[Test]
		public void ShouldAuthenticateUserRetrievingBusinessUnits()
		{
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(new AuthenticatorResult
			{
				Successful = true,
				DataSource = new FakeDataSource(),
				Person = new Person()
			});
			var target = new AuthenticationApiController(MockRepository.GenerateMock<IBusinessUnitsViewModelFactory>(), identityLogon, MockRepository.GenerateStub<ILogLogonAttempt>(), null, null);

			target.BusinessUnits();

			identityLogon.AssertWasCalled(x => x.LogonIdentityUser());
		}

		[Test]
		public void ShouldRetrieveBusinessUnits()
		{
			var authenticationResult = new AuthenticatorResult
				{
					Successful = true,
					DataSource = new FakeDataSource(),
					Person = new Person()
				};
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(authenticationResult);
			var businessUnitViewModels = new[] {new BusinessUnitViewModel()};
			var businessUnitViewModelFactory = MockRepository.GenerateMock<IBusinessUnitsViewModelFactory>();
			businessUnitViewModelFactory.Stub(x => x.BusinessUnits(authenticationResult.DataSource, authenticationResult.Person)).Return(businessUnitViewModels);
			var target = new AuthenticationApiController(businessUnitViewModelFactory, identityLogon, null, null, null);

			var result = target.BusinessUnits();

			result.Data.Should().Be.SameInstanceAs(businessUnitViewModels);
		}

		[Test]
		public void ShouldReturnErrorIfAuthenticationUnsuccessfulRetrievingBusinessUnits()
		{
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(new AuthenticatorResult { Successful = false });
			var target = new StubbingControllerBuilder().CreateController<AuthenticationApiController>(null, identityLogon, null, null, null);

			var result = target.BusinessUnits();

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Not.Be.Null();
			(result.Data as ModelStateResult).Errors.Single().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldAuthenticateUserOnLogon()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var result = new AuthenticatorResult
				{
					Successful = true,
					DataSource = new FakeDataSource(),
					Person = person
				};
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(result);
			var log = MockRepository.GenerateMock<ILogLogonAttempt>();
			var target = new AuthenticationApiController(MockRepository.GenerateMock<IBusinessUnitsViewModelFactory>(), identityLogon, log, MockRepository.GenerateMock<IWebLogOn>(), null);
			identityLogon.Stub(x => x.LogonIdentityUser()).Return(result);
			log.Expect(x => x.SaveAuthenticateResult(string.Empty, result.PersonId(), result.Successful));
			target.Logon(Guid.NewGuid());

			identityLogon.AssertWasCalled(x => x.LogonIdentityUser());
		}

		[Test]
		public void ShouldLogon()
		{
			var tenantPassword = RandomName.Make();
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var businessUnitId = Guid.NewGuid();
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(new AuthenticatorResult
				{
					Successful = true,
					DataSource = new FakeDataSource {DataSourceName = "datasource"},
					Person = person,
					TenantPassword = tenantPassword
				});
			var webLogon = MockRepository.GenerateMock<IWebLogOn>();
			var target = new AuthenticationApiController(null, identityLogon, MockRepository.GenerateStub<ILogLogonAttempt>(), webLogon, null);

			target.Logon(businessUnitId);

			webLogon.AssertWasCalled(x => x.LogOn("datasource", businessUnitId, person.Id.Value, tenantPassword));
		}

		[Test]
		public void ShouldReturnErrorIfAuthenticationUnsuccessfulLoggingOn()
		{
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(new AuthenticatorResult { Successful = false });
			var target = new StubbingControllerBuilder().CreateController<AuthenticationApiController>(null, identityLogon, MockRepository.GenerateStub<ILogLogonAttempt>(), null, null);

			var result = target.Logon(Guid.NewGuid());

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void ShouldReturnLicenseErrorIfAuthenticationUnsuccessfulLoggingOn()
		{
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(new AuthenticatorResult {Successful = true, DataSource = new FakeDataSource(), Person = PersonFactory.CreatePersonWithGuid(RandomName.Make(), RandomName.Make())});
			var webLogon = MockRepository.GenerateMock<IWebLogOn>();
			webLogon.Expect(x => x.LogOn(null, Guid.Empty, Guid.Empty, null)).IgnoreArguments().Throw(new LicenseMissingException());
			var target = new StubbingControllerBuilder().CreateController<AuthenticationApiController>(null, identityLogon, MockRepository.GenerateStub<ILogLogonAttempt>(), webLogon, MockRepository.GenerateStub<IDataSourceForTenant>());

			var result = target.Logon(Guid.NewGuid());

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(Resources.TeleoptiProductActivationKeyException);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(Resources.TeleoptiProductActivationKeyException);
		}

		[Test]
		public void ShouldReturnDataSourceIfLicenseIsInvalidSoItIsReReadIfLicenseIsAdded()
		{
			var tenantName = RandomName.Make();
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(new AuthenticatorResult { Successful = true, DataSource = new FakeDataSource(tenantName), Person = PersonFactory.CreatePersonWithGuid(RandomName.Make(), RandomName.Make()) });
			var dataSourceForTenant = MockRepository.GenerateMock<IDataSourceForTenant>();
			var webLogon = MockRepository.GenerateMock<IWebLogOn>();
			webLogon.Expect(x => x.LogOn(null, Guid.Empty, Guid.Empty, null)).IgnoreArguments().Throw(new LicenseMissingException());
			var target = new StubbingControllerBuilder().CreateController<AuthenticationApiController>(null, identityLogon, MockRepository.GenerateStub<ILogLogonAttempt>(), webLogon, dataSourceForTenant);

			target.Logon(Guid.NewGuid());

			dataSourceForTenant.AssertWasCalled(x => x.RemoveDataSource(tenantName));
		}

		[Test]
		public void ShouldReturnErrorIfNoPermission()
		{
			var tenantPassword = RandomName.Make();
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var businessUnitId = Guid.NewGuid();
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(new AuthenticatorResult
			{
				Successful = true,
				DataSource = new FakeDataSource { DataSourceName = "datasource" },
				Person = person,
				TenantPassword = tenantPassword
			});
			var webLogon = MockRepository.GenerateMock<IWebLogOn>();
			webLogon.Stub(x => x.LogOn("datasource", businessUnitId, person.Id.Value, tenantPassword)).Throw(new PermissionException());
			var target = new StubbingControllerBuilder().CreateController<AuthenticationApiController>(null, identityLogon, MockRepository.GenerateStub<ILogLogonAttempt>(), webLogon, null);

			var result = target.Logon(businessUnitId);

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(Resources.InsufficientPermissionForWeb);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(Resources.InsufficientPermissionForWeb);
			
		}
	}
}