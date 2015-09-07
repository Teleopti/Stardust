using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.SSO.Core;

namespace Teleopti.Ccc.WebTest.Areas.SSO
{
	public class SsoAuthenticatorTest
	{
		[Test]
		public void ShouldIncludePasswordExpiredInApplicationLogonResult()
		{
			const string userName = "Roger";
			const string password = "moore";

			var applicationAuthentication = MockRepository.GenerateMock<IApplicationAuthentication>();
			var loadUserUnauthorized = MockRepository.GenerateStub<ILoadUserUnauthorized>();
			var target = new SsoAuthenticator(null, applicationAuthentication, loadUserUnauthorized);
			applicationAuthentication.Stub(x => x.Logon(userName, password)).Return(new TenantAuthenticationResult
			{
				PasswordExpired = true
			});

			target.AuthenticateApplicationUser(userName, password)
				.PasswordExpired.Should().Be.True();
		}
	}
}