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
			var target = new TennantController(appAuthentication);
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
			var target = new StubbingControllerBuilder().CreateController<TennantController>(appAuthentication);
			appAuthentication.Expect(x => x.Logon(userName, password)).Return(serviceResult);

			var result = ((ApplicationAuthenticationResult)target.ApplicationLogon(userName, password).Data);
			result.Should().Be.SameInstanceAs(serviceResult);
			target.Response.StatusCode.Should().Be.EqualTo(401);
		} 
	}
}