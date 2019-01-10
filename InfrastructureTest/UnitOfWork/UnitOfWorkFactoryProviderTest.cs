using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class UnitOfWorkFactoryProviderTest
	{
		private ICurrentUnitOfWorkFactory target;
		private ICurrentTeleoptiPrincipal currentTeleoptiPrincipal;

		[SetUp]
		public void Setup()
		{
			currentTeleoptiPrincipal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			target = new CurrentUnitOfWorkFactory(new CurrentDataSource(new CurrentIdentity(currentTeleoptiPrincipal), new DataSourceState()));
		}

		[Test]
		public void ShouldReturnIdentityApplicationDatasouce()
		{
			var expectedUnitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var dataSource = new FakeDataSource {Application = expectedUnitOfWorkFactory};
			var identity = MockRepository.GenerateMock<ITeleoptiIdentity>();
			var teleoptiPrincipal = new TeleoptiPrincipalForLegacy(identity, new Person());
			currentTeleoptiPrincipal.Expect(x => x.Current()).Return(teleoptiPrincipal);
			identity.Expect(x => x.IsAuthenticated).Return(true);
			identity.Expect(x => x.DataSource).Return(dataSource);

			target.Current().Should().Be.SameInstanceAs(expectedUnitOfWorkFactory);
		}

	}
}