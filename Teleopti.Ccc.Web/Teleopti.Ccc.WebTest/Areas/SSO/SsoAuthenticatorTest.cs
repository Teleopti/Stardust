using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.SSO
{
	public class SsoAuthenticatorTest
	{
		[Test]
		public void ShouldIncludePasswordExpiredInApplicationLogonResult()
		{
			const string userName = "Roger";
			const string password = "moore";
			const string dataSourceName = "something";

			var applicationAuthentication = MockRepository.GenerateMock<IApplicationAuthentication>();
			var applicationData = MockRepository.GenerateMock<IApplicationData>();
			var datasource = MockRepository.GenerateMock<IDataSource>();
			var loadUserUnauthorized = MockRepository.GenerateStub<ILoadUserUnauthorized>();
			var target = new SsoAuthenticator(null, applicationAuthentication, loadUserUnauthorized);
			applicationAuthentication.Stub(x => x.Logon(userName, password)).Return(new TenantAuthenticationResult
			{
				PasswordExpired = true
			});
			applicationData.Stub(x => x.Tenant(dataSourceName)).Return(datasource);
			datasource.Stub(x => x.DataSourceName).Return(dataSourceName);

			target.AuthenticateApplicationUser(userName, password)
				.PasswordExpired.Should().Be.True();
		}
	}
}