﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Tenant;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Ccc.Web.Areas.Tenant.Model;

namespace Teleopti.Ccc.WebTest.Areas.Tenant
{
	public class AuthenticateControllerTest
	{
		[Test]
		public void SuccessfulApplicationLogon()
		{
			const string userName = "Hejhej";
			const string password = "Tjoflöjt";
			var serviceResult = new ApplicationAuthenticationResult
			{
				Success = true,
				PersonId = Guid.NewGuid(),
				Tenant = Guid.NewGuid().ToString()
			};
			var appAuthentication = MockRepository.GenerateMock<IApplicationAuthentication>();
			var target = new AuthenticateController(appAuthentication, null, MockRepository.GenerateMock<ILogLogonAttempt>());
			appAuthentication.Expect(x => x.Logon(userName, password)).Return(serviceResult);

			var webCall = target.ApplicationLogon(new ApplicationLogonModel{UserName = userName, Password = password});
			var result = ((ApplicationAuthenticationResult)webCall.Data);
			result.Should().Be.SameInstanceAs(serviceResult);
		}

		[Test]
		public void FailingApplicationLogon()
		{
			const string userName = "Hejhej";
			const string password = "Tjoflöjt";
			var serviceResult = new ApplicationAuthenticationResult
			{
				Success = false,
				FailReason = "nåt fel"
			};
			var appAuthentication = MockRepository.GenerateMock<IApplicationAuthentication>();
			var target = new StubbingControllerBuilder().CreateController<AuthenticateController>(appAuthentication, null, MockRepository.GenerateMock<ILogLogonAttempt>());
			appAuthentication.Expect(x => x.Logon(userName, password)).Return(serviceResult);

			var result = ((ApplicationAuthenticationResult)target.ApplicationLogon(new ApplicationLogonModel { UserName = userName, Password = password }).Data);
			result.Should().Be.SameInstanceAs(serviceResult);
			//target.Response.StatusCode.Should().Be.EqualTo(401);
		}

		[Test]
		public void SuccessfulIdentityLogon()
		{
			const string identity = "Hejhej\\Tjoflöjt";
			
			var serviceResult = new ApplicationAuthenticationResult
			{
				Success = true,
				PersonId = Guid.NewGuid(),
				Tenant = Guid.NewGuid().ToString()
			};
			var identityAuthentication = MockRepository.GenerateMock<IIdentityAuthentication>();
			var target = new AuthenticateController(null, identityAuthentication, MockRepository.GenerateMock<ILogLogonAttempt>());
			identityAuthentication.Expect(x => x.Logon(identity)).Return(serviceResult);

			var webCall = target.IdentityLogon(new IdentityLogonModel{Identity = identity});
			var result = ((ApplicationAuthenticationResult)webCall.Data);
			result.Should().Be.SameInstanceAs(serviceResult);
		}

		[Test]
		public void FailingIdentityLogon()
		{
			const string identity = "Hejhej\\Tjoflöjt";
			var serviceResult = new ApplicationAuthenticationResult
			{
				Success = false,
				FailReason = "nåt fel"
			};
			var identityAuthentication = MockRepository.GenerateMock<IIdentityAuthentication>();
			var target = new StubbingControllerBuilder().CreateController<AuthenticateController>(null, identityAuthentication, MockRepository.GenerateMock<ILogLogonAttempt>());
			identityAuthentication.Expect(x => x.Logon(identity)).Return(serviceResult);

			var result = ((ApplicationAuthenticationResult)target.IdentityLogon(new IdentityLogonModel { Identity = identity }).Data);
			result.Should().Be.SameInstanceAs(serviceResult);
			//target.Response.StatusCode.Should().Be.EqualTo(401);
		}

		[Test]
		public void ShouldLogApplicationLogon()
		{
			const string userName = "sadfasdf";
			const string password = " löaksdf";
			var serviceResult = new ApplicationAuthenticationResult();
			var appAuthentication = MockRepository.GenerateMock<IApplicationAuthentication>();
			var logger = MockRepository.GenerateMock<ILogLogonAttempt>();
			var target = new StubbingControllerBuilder().CreateController<AuthenticateController>(appAuthentication, null, logger);
			appAuthentication.Expect(x => x.Logon(userName, password)).Return(serviceResult);

			target.ApplicationLogon((new ApplicationLogonModel { UserName = userName, Password = password }));

			logger.AssertWasCalled(x => x.SaveAuthenticateResult(userName, serviceResult.PersonId, serviceResult.Success));
		}

		[Test]
		public void ShouldLogIdentityLogon()
		{
			const string identity = "sadfasdf asdf";
			var serviceResult = new ApplicationAuthenticationResult();
			var idAuthentication = MockRepository.GenerateMock<IIdentityAuthentication>();
			var logger = MockRepository.GenerateMock<ILogLogonAttempt>();
			var target = new StubbingControllerBuilder().CreateController<AuthenticateController>(null, idAuthentication, logger);
			idAuthentication.Expect(x => x.Logon(identity)).Return(serviceResult);

			target.IdentityLogon(new IdentityLogonModel { Identity = identity });

			logger.AssertWasCalled(x => x.SaveAuthenticateResult(identity, serviceResult.PersonId, serviceResult.Success));
		}
	}
}