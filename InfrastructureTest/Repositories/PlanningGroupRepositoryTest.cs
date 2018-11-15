using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class PlanningGroupRepositoryTest : RepositoryTest<PlanningGroup>
	{
		protected override PlanningGroup CreateAggregateWithCorrectBusinessUnit()
		{
			return new PlanningGroup("Test");
		}

		protected override void VerifyAggregateGraphProperties(PlanningGroup loadedAggregateFromDatabase)
		{
			var expected = CreateAggregateWithCorrectBusinessUnit();
			loadedAggregateFromDatabase.Name.Should().Be.EqualTo(expected.Name);
		}

		protected override Repository<PlanningGroup> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PlanningGroupRepository(currentUnitOfWork);
		}
	}
}