using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class DataSourceProviderTest
	{
		[Test]
		public void ShouldReturnCurrentDataSource()
		{
			var identityProvider = MockRepository.GenerateMock<IIdentityProvider>();
			var target = new DataSourceProvider(identityProvider);
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var identity = new TeleoptiIdentity("hej", dataSource, null, null, AuthenticationTypeOption.Unknown);

			identityProvider.Stub(x => x.Current()).Return(identity);

			target.CurrentDataSource().Should().Be.SameInstanceAs(dataSource);
		}

		[Test]
		public void ShouldReturnNullWhenCurrentPrincipalNotDefined()
		{
			var identityProvider = MockRepository.GenerateMock<IIdentityProvider>();
			var target = new DataSourceProvider(identityProvider);

			identityProvider.Stub(x => x.Current()).Return(null);

			target.CurrentDataSource().Should().Be.Null();
		}
	}
}