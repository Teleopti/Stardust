using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class WindowsAuthenticationModelTest
	{
		[Test]
		public void ShouldAuthenticateUser()
		{
			var authenticator = MockRepository.GenerateMock<IIdentityLogon>();
			var expectedResult = new AuthenticateResult();
			authenticator.Stub(x => x.LogonIdentityUser()).Return(expectedResult);
			var target = new WindowsAuthenticationModel(authenticator, null);

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

			var target = new WindowsAuthenticationModel(null, logLogResult);

			target.SaveAuthenticateResult(expectedResult);
		}
	}
}