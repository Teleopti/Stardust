using System;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class AgentGroupRepositoryTest : RepositoryTest<AgentGroup>
	{
		protected override AgentGroup CreateAggregateWithCorrectBusinessUnit()
		{
			return new AgentGroup
			{
				Name = "Test"
			};
		}

		protected override void VerifyAggregateGraphProperties(AgentGroup loadedAggregateFromDatabase)
		{
			var expected = CreateAggregateWithCorrectBusinessUnit();
			loadedAggregateFromDatabase.Name.Should().Be.EqualTo(expected.Name);
		}

		protected override Repository<AgentGroup> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new AgentGroupRepository(currentUnitOfWork);
		}
	}
}