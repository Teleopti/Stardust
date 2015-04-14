using System;
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
		[Test]
		public void ShouldAuthenticateUserRetrievingBusinessUnits()
		{
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(new AuthenticateResult
			{
				Successful = true,
				DataSource = new FakeDataSource(),
				Person = new Person()
			});
			var target = new AuthenticationApiController(MockRepository.GenerateMock<IBusinessUnitsViewModelFactory>(), identityLogon, MockRepository.GenerateStub<ILogLogonAttempt>(), null);

			target.BusinessUnits();

			identityLogon.AssertWasCalled(x => x.LogonIdentityUser());
		}

		[Test]
		public void ShouldRetrieveBusinessUnits()
		{
			var authenticationResult = new AuthenticateResult
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
			var target = new AuthenticationApiController(businessUnitViewModelFactory, identityLogon, null, null);

			var result = target.BusinessUnits();

			result.Data.Should().Be.SameInstanceAs(businessUnitViewModels);
		}

		[Test]
		public void ShouldReturnErrorIfAuthenticationUnsuccessfulRetrievingBusinessUnits()
		{
			const string message = "test";
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(new AuthenticateResult { Successful = false, Message = message });
			var target = new StubbingControllerBuilder().CreateController<AuthenticationApiController>(null, identityLogon, null, null);

			var result = target.BusinessUnits();

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(message);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(message);
		}

		[Test]
		public void ShouldAuthenticateUserOnLogon()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var result = new AuthenticateResult
				{
					Successful = true,
					DataSource = new FakeDataSource(),
					Person = person
				};
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(result);
			var log = MockRepository.GenerateMock<ILogLogonAttempt>();
			var target = new AuthenticationApiController(MockRepository.GenerateMock<IBusinessUnitsViewModelFactory>(), identityLogon, log, MockRepository.GenerateMock<IWebLogOn>());
			identityLogon.Stub(x => x.LogonIdentityUser()).Return(result);
			log.Expect(x => x.SaveAuthenticateResult(string.Empty, result.PersonId(), result.Successful));
			target.Logon(Guid.NewGuid());

			identityLogon.AssertWasCalled(x => x.LogonIdentityUser());
		}

		[Test]
		public void ShouldLogon()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var businessUnitId = Guid.NewGuid();
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(new AuthenticateResult
				{
					Successful = true,
					DataSource = new FakeDataSource {DataSourceName = "datasource"},
					Person = person
				});
			var webLogon = MockRepository.GenerateMock<IWebLogOn>();
			var target = new AuthenticationApiController(null, identityLogon, MockRepository.GenerateStub<ILogLogonAttempt>(), webLogon);

			target.Logon(businessUnitId);

			webLogon.AssertWasCalled(x => x.LogOn("datasource", businessUnitId, person.Id.Value));
		}

		[Test]
		public void ShouldReturnErrorIfAuthenticationUnsuccessfulLoggingOn()
		{
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(new AuthenticateResult{Successful = false});
			var target = new StubbingControllerBuilder().CreateController<AuthenticationApiController>(null, identityLogon, MockRepository.GenerateStub<ILogLogonAttempt>(), null);

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
			identityLogon.Expect(x => x.LogonIdentityUser()).Throw(new LicenseMissingException());
			var target = new StubbingControllerBuilder().CreateController<AuthenticationApiController>(null, identityLogon, null, null);

			var result = target.Logon(Guid.NewGuid());

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(Resources.TeleoptiProductActivationKeyException);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(Resources.TeleoptiProductActivationKeyException);
		}

		[Test]
		public void ShouldReturnErrorIfNoPermission()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var businessUnitId = Guid.NewGuid();
			var identityLogon = MockRepository.GenerateMock<IIdentityLogon>();
			identityLogon.Expect(x => x.LogonIdentityUser()).Return(new AuthenticateResult
			{
				Successful = true,
				DataSource = new FakeDataSource { DataSourceName = "datasource" },
				Person = person
			});
			var webLogon = MockRepository.GenerateMock<IWebLogOn>();
			webLogon.Stub(x => x.LogOn("datasource", businessUnitId, person.Id.Value)).Throw(new PermissionException());
			var target = new StubbingControllerBuilder().CreateController<AuthenticationApiController>(null, identityLogon, MockRepository.GenerateStub<ILogLogonAttempt>(), webLogon);

			var result = target.Logon(businessUnitId);

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(Resources.InsufficientPermissionForWeb);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(Resources.InsufficientPermissionForWeb);
			
		}
	}
}