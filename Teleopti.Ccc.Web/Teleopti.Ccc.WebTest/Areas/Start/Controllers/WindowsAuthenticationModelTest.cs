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
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var expectedResult = new AuthenticateResult();
			authenticator.Stub(x => x.AuthenticateWindowsUser("mydata")).Return(expectedResult);
			var target = new WindowsAuthenticationModel(authenticator) { DataSourceName = "mydata" };

			var result = target.AuthenticateUser();

			result.Should().Be.SameInstanceAs(expectedResult);
		}
	}
}