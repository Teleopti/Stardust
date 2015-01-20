using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Tennant;
using Teleopti.Ccc.Web.Areas.Tennant.Core;

namespace Teleopti.Ccc.WebTest.Areas.Tennant
{
	public class TennantControllerTest
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
				Tennant = Guid.NewGuid().ToString()
			};
			var appAuthentication = MockRepository.GenerateMock<IApplicationAuthentication>();
			var identityAuthentication = MockRepository.GenerateMock<IIdentityAuthentication>();
			var target = new TennantController(appAuthentication, identityAuthentication);
			appAuthentication.Expect(x => x.Logon(userName, password)).Return(serviceResult);

			var webCall = target.ApplicationLogon(userName, password);
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
			var identityAuthentication = MockRepository.GenerateMock<IIdentityAuthentication>();
			var target = new StubbingControllerBuilder().CreateController<TennantController>(appAuthentication, identityAuthentication);
			appAuthentication.Expect(x => x.Logon(userName, password)).Return(serviceResult);

			var result = ((ApplicationAuthenticationResult)target.ApplicationLogon(userName, password).Data);
			result.Should().Be.SameInstanceAs(serviceResult);
			target.Response.StatusCode.Should().Be.EqualTo(401);
		}

		[Test]
		public void SuccessfulIdentityLogon()
		{
			const string identity = "Hejhej\\Tjoflöjt";
			
			var serviceResult = new ApplicationAuthenticationResult
			{
				Success = true,
				PersonId = Guid.NewGuid(),
				Tennant = Guid.NewGuid().ToString()
			};
			var appAuthentication = MockRepository.GenerateMock<IApplicationAuthentication>();
			var identityAuthentication = MockRepository.GenerateMock<IIdentityAuthentication>();
			var target = new TennantController(appAuthentication, identityAuthentication);
			identityAuthentication.Expect(x => x.Logon(identity)).Return(serviceResult);

			var webCall = target.IdentityLogon(identity);
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
			var appAuthentication = MockRepository.GenerateMock<IApplicationAuthentication>();
			var identityAuthentication = MockRepository.GenerateMock<IIdentityAuthentication>();
			var target = new StubbingControllerBuilder().CreateController<TennantController>(appAuthentication, identityAuthentication);
			identityAuthentication.Expect(x => x.Logon(identity)).Return(serviceResult);

			var result = ((ApplicationAuthenticationResult)target.IdentityLogon(identity).Data);
			result.Should().Be.SameInstanceAs(serviceResult);
			target.Response.StatusCode.Should().Be.EqualTo(401);
		}
	}
}