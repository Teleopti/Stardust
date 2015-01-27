using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Tennant.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Authentication.Services
{
	public class MultiTennantAuthenticatorTest
	{
		[Test]
		public void ShouldIncludePasswordExpiredInApplicationLogonResult()
		{
			const string userName = "Roger";
			const string password = "moore";
			const string dataSourceName = "something";

			var applicationAuthentication = MockRepository.GenerateMock<IApplicationAuthentication>();
			var datasourcesProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
			var datasource = MockRepository.GenerateMock<IDataSource>();
			var target = new MultiTennantAuthenticator(datasourcesProvider, null, null, applicationAuthentication, null);
			applicationAuthentication.Stub(x => x.Logon(userName, password)).Return(new ApplicationAuthenticationResult
			{
				PasswordExpired = true
			});
			datasourcesProvider.Stub(x => x.RetrieveDataSourceByName(dataSourceName)).Return(datasource);
			datasource.Stub(x => x.DataSourceName).Return(dataSourceName);

			target.AuthenticateApplicationUser(dataSourceName, userName, password)
				.PasswordExpired.Should().Be.True();
		}
	}
}