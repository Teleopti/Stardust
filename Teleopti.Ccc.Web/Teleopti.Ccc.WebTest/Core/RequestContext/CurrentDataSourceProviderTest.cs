using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class CurrentDataSourceProviderTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			principalProvider = MockRepository.GenerateMock<IPrincipalProvider>();
			target = new CurrentDataSourceProvider(principalProvider);
		}

		#endregion

		private ICurrentDataSourceProvider target;
		private IPrincipalProvider principalProvider;

		[Test]
		public void ShouldReturnCurrentDataSource()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var teleoptiPrincipal = new TeleoptiPrincipal(new TeleoptiIdentity("hej", dataSource, null, null, AuthenticationTypeOption.Unknown), new Person());

			principalProvider.Expect(x => x.Current()).Return(teleoptiPrincipal);

			target.CurrentDataSource()
				.Should().Be.SameInstanceAs(dataSource);
		}

		[Test]
		public void ShouldReturnNullWhenCurrentPrincipalNotDefined()
		{
			principalProvider.Expect(x => x.Current()).Return(null);

			target.CurrentDataSource()
				.Should().Be.Null();
		}
	}
}