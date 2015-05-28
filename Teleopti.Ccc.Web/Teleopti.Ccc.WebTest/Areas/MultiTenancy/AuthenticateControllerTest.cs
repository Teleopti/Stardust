using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	//TODO: tenant replace with "new kind of tests" - most tests in Core namespace should be converted as well
	public class AuthenticateControllerTest
	{
		[Test]
		public void SuccessfulApplicationLogon()
		{
			const string userName = "Hejhej";
			const string password = "Tjoflöjt";
			var serviceResult = new TenantAuthenticationResult
			{
				Success = true,
				PersonId = Guid.NewGuid(),
				Tenant = Guid.NewGuid().ToString()
			};
			var appAuthentication = MockRepository.GenerateMock<IApplicationAuthentication>();
			var target = new AuthenticateController(appAuthentication, null, MockRepository.GenerateMock<ILogLogonAttempt>());
			appAuthentication.Expect(x => x.Logon(userName, password)).Return(serviceResult);

			var webCall = target.ApplicationLogon(new ApplicationLogonModel{UserName = userName, Password = password});
			var result = ((TenantAuthenticationResult)webCall.Data);
			result.Should().Be.SameInstanceAs(serviceResult);
		}

		[Test]
		public void FailingApplicationLogon()
		{
			const string userName = "Hejhej";
			const string password = "Tjoflöjt";
			var serviceResult = new TenantAuthenticationResult
			{
				Success = false,
				FailReason = "nåt fel"
			};
			var appAuthentication = MockRepository.GenerateMock<IApplicationAuthentication>();
			var target = new StubbingControllerBuilder().CreateController<AuthenticateController>(appAuthentication, null, MockRepository.GenerateMock<ILogLogonAttempt>());
			appAuthentication.Expect(x => x.Logon(userName, password)).Return(serviceResult);

			var result = ((TenantAuthenticationResult)target.ApplicationLogon(new ApplicationLogonModel { UserName = userName, Password = password }).Data);
			result.Should().Be.SameInstanceAs(serviceResult);
			//target.Response.StatusCode.Should().Be.EqualTo(401);
		}

		[Test]
		public void ShouldLogApplicationLogon()
		{
			const string userName = "sadfasdf";
			const string password = " löaksdf";
			var serviceResult = new TenantAuthenticationResult();
			var appAuthentication = MockRepository.GenerateMock<IApplicationAuthentication>();
			var logger = MockRepository.GenerateMock<ILogLogonAttempt>();
			var target = new StubbingControllerBuilder().CreateController<AuthenticateController>(appAuthentication, null, logger);
			appAuthentication.Expect(x => x.Logon(userName, password)).Return(serviceResult);

			target.ApplicationLogon((new ApplicationLogonModel { UserName = userName, Password = password }));

			logger.AssertWasCalled(x => x.SaveAuthenticateResult(userName, serviceResult.PersonId, serviceResult.Success));
		}
	}
}