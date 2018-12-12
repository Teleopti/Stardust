using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.WebTest.TestHelper;

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

			result.Result<IEnumerable<BusinessUnitViewModel>>().Should().Be.SameInstanceAs(businessUnitViewModels);
		}

		[Test]
		public void ShouldReturnErrorIfAuthenticationUnsuccessfulRetrievingBusinessUnits()
		{
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(new AuthenticatorResult { Successful = false });
			var target = new AuthenticationApiController(null, identityLogon, null, null, null);

			var result = target.BusinessUnits();
			result.Result<string>().Should().Be.EqualTo("NoUserFound");
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
			target.Logon(new ApiLogonInputModel {BusinessUnitId = Guid.NewGuid()});

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
					TenantPassword = tenantPassword,
					IsPersistent = false
				});
			var webLogon = MockRepository.GenerateMock<IWebLogOn>();
			var target = new AuthenticationApiController(null, identityLogon, MockRepository.GenerateStub<ILogLogonAttempt>(), webLogon, null);

			target.Logon(new ApiLogonInputModel { BusinessUnitId = businessUnitId, IsLogonFromBrowser = true});

			webLogon.AssertWasCalled(x => x.LogOn("datasource", businessUnitId, person, tenantPassword, false, true));
		}

		[Test]
		public void ShouldRememberMeForWfm()
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
				TenantPassword = tenantPassword,
				IsPersistent = true
			});
			var webLogon = MockRepository.GenerateMock<IWebLogOn>();
			var target = new AuthenticationApiController(null, identityLogon, MockRepository.GenerateStub<ILogLogonAttempt>(), webLogon, null);

			target.Logon(new ApiLogonInputModel { BusinessUnitId = businessUnitId });

			webLogon.AssertWasCalled(x => x.LogOn("datasource", businessUnitId, person, tenantPassword, true, false));
		}

		[Test]
		public void ShouldReturnErrorIfAuthenticationUnsuccessfulLoggingOn()
		{
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(new AuthenticatorResult { Successful = false });
			var target = new AuthenticationApiController(null, identityLogon, MockRepository.GenerateStub<ILogLogonAttempt>(), null, null);

			var result = (InvalidModelStateResult)target.Logon(new ApiLogonInputModel { BusinessUnitId = Guid.NewGuid() });

			result.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void ShouldReturnLicenseErrorIfAuthenticationUnsuccessfulLoggingOn()
		{
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(new AuthenticatorResult {Successful = true, DataSource = new FakeDataSource(), Person = PersonFactory.CreatePersonWithGuid(RandomName.Make(), RandomName.Make())});
			var webLogon = MockRepository.GenerateMock<IWebLogOn>();
			webLogon.Expect(x => x.LogOn(null, Guid.Empty, null, null, false, false)).IgnoreArguments().Throw(new LicenseMissingException());
			var target = new AuthenticationApiController(null, identityLogon, MockRepository.GenerateStub<ILogLogonAttempt>(), webLogon, MockRepository.GenerateStub<IDataSourceForTenant>());

			var result = (InvalidModelStateResult)target.Logon(new ApiLogonInputModel { BusinessUnitId = Guid.NewGuid() });

			result.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(Resources.TeleoptiProductActivationKeyException);
		}

		[Test]
		public void ShouldReturnDataSourceIfLicenseIsInvalidSoItIsReReadIfLicenseIsAdded()
		{
			var tenantName = RandomName.Make();
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(new AuthenticatorResult { Successful = true, DataSource = new FakeDataSource(tenantName), Person = PersonFactory.CreatePersonWithGuid(RandomName.Make(), RandomName.Make()) });
			var dataSourceForTenant = MockRepository.GenerateMock<IDataSourceForTenant>();
			var webLogon = MockRepository.GenerateMock<IWebLogOn>();
			webLogon.Expect(x => x.LogOn(null, Guid.Empty, null, null, false, false)).IgnoreArguments().Throw(new LicenseMissingException());
			var target = new AuthenticationApiController(null, identityLogon, MockRepository.GenerateStub<ILogLogonAttempt>(), webLogon, dataSourceForTenant);

			target.Logon(new ApiLogonInputModel { BusinessUnitId = Guid.NewGuid() });

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
			webLogon.Stub(x => x.LogOn("datasource", businessUnitId, person, tenantPassword, false, false)).Throw(new PermissionException());
			var target = new AuthenticationApiController(null, identityLogon, MockRepository.GenerateStub<ILogLogonAttempt>(), webLogon, null);

			var result = (InvalidModelStateResult)target.Logon(new ApiLogonInputModel { BusinessUnitId = businessUnitId });

			result.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(Resources.InsufficientPermissionForWeb);
		}
	}
}