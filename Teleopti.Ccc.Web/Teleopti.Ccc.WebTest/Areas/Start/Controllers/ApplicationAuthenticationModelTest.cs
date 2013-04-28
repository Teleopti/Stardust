using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class ApplicationAuthenticationModelTest
	{
		[Test]
		public void ShouldAuthenticateUser()
		{
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var expectedResult = new AuthenticateResult();
			authenticator.Stub(x => x.AuthenticateApplicationUser("mydata", "username", "password")).Return(expectedResult);
			var target = new ApplicationAuthenticationModel(authenticator)
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
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var expectedResult = new AuthenticateResult();
			
			var target = new ApplicationAuthenticationModel(authenticator)
			{
				DataSourceName = "mydata",
				Password = "password",
				UserName = "username"
			};
			authenticator.Stub(x => x.SaveAuthenticateResult("username", expectedResult));

			target.SaveAuthenticateResult(expectedResult);
		}
	}
}