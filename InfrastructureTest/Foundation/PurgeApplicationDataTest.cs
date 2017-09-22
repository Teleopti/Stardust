using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.InfrastructureTest.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    public class PurgeApplicationDataTest : DatabaseTest
    {
    	private IExecutableCommand target;

		protected override void SetupForRepositoryTest()
		{
			target = new PurgeApplicationData(SetupFixtureForAssembly.DataSource.Application);
		}

		[Test]
		public void ShouldCallPurgeProcedure()
		{
			target.Execute();
		}
    }
}
