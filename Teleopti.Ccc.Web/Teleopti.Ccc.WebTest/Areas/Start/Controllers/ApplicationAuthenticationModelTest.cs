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
			authenticator.Stub(x => x.AuthenticateApplicationUser("mydata", "username", "password")).Return(expectedResult);
			var target = new ApplicationAuthenticationModel(authenticator, null)
				{
					DataSourceName = "mydata",
					Password = "password",
					UserName = "username"
				};

			var result = target.AuthenticateUser();

			result.Should().Be.SameInstanceAs(expectedResult);
		}

		[Test]
		public void ShouldSaveResult()
		{
			var expectedResult = new AuthenticateResult();
			const string userName = "sdfsdfsd";
			var logLogResult = MockRepository.GenerateMock<ILogLogonAttempt>();
			logLogResult.Expect(x => x.SaveAuthenticateResult(userName, expectedResult));

			var target = new ApplicationAuthenticationModel(null, logLogResult)
			{
				DataSourceName = "mydata",
				Password = "password",
				UserName = userName
			};
			
			target.SaveAuthenticateResult(expectedResult);
		}
	}
}