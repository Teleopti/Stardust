using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.SSO.Models;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class ApplicationAuthenticationModelTest
	{
		[Test]
		public void ShouldAuthenticateUser()
		{
			var authenticator = MockRepository.GenerateMock<ISsoAuthenticator>();
			var expectedResult = new AuthenticateResult();
			authenticator.Stub(x => x.AuthenticateApplicationUser("username", "password")).Return(expectedResult);
			var target = new ApplicationAuthenticationModel(authenticator, null)
				{
					Password = "password",
					UserName = "username"
				};

			var result = target.AuthenticateUser();

			result.Should().Be.SameInstanceAs(expectedResult);
		}

		[Test]
		public void ShouldSaveResult()
		{
			var authResult = new AuthenticateResult();
			const string userName = "sdfsdfsd";
			var logLogResult = MockRepository.GenerateMock<ILogLogonAttempt>();

			var target = new ApplicationAuthenticationModel(null, logLogResult)
			{
				Password = "password",
				UserName = userName
			};
			
			target.SaveAuthenticateResult(authResult);
			logLogResult.AssertWasCalled(x => x.SaveAuthenticateResult(userName, authResult.PersonId(), authResult.Successful));
		}
	}
}