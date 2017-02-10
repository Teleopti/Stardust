using System;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	public class AgentGroupRepositoryTest : RepositoryTest<IAgentGroup>
	{
		protected override IAgentGroup CreateAggregateWithCorrectBusinessUnit()
		{
			return new AgentGroup
			{
				Name = "Test"
			};
		}

		protected override void VerifyAggregateGraphProperties(IAgentGroup loadedAggregateFromDatabase)
		{
			var expected = CreateAggregateWithCorrectBusinessUnit();
			loadedAggregateFromDatabase.Name.Should().Be.EqualTo(expected.Name);
		}

		protected override Repository<IAgentGroup> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new AgentGroupRepository(currentUnitOfWork);
		}
	}
}