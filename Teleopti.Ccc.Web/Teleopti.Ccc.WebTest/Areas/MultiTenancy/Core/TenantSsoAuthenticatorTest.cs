using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class TenantSsoAuthenticatorTest
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
			var target = new TenantSsoAuthenticator(applicationData, null, applicationAuthentication);
			applicationAuthentication.Stub(x => x.Logon(userName, password)).Return(new ApplicationAuthenticationResult
			{
				PasswordExpired = true
			});
			applicationData.Stub(x => x.DataSource(dataSourceName)).Return(datasource);
			datasource.Stub(x => x.DataSourceName).Return(dataSourceName);

			target.AuthenticateApplicationUser(userName, password)
				.PasswordExpired.Should().Be.True();
		}
	}
}