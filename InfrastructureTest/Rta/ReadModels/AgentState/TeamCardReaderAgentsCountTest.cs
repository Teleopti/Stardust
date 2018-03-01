using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Rta.Service;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState
{
	[TestFixture]
	[UnitOfWorkTest]
	public class TeamCardReaderAgentsCountTest
	{
		public ICurrentBusinessUnit BusinessUnit;
		public IAgentStateReadModelPersister Persister;
		public ITeamCardReader Target;
		public MutableNow Now;
		public Database Database;

		[Test]
		public void ShouldReadAgentsCount()
		{
			var team = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				BusinessUnitId = BusinessUnit.Current().Id.Value,
				PersonId = Guid.NewGuid(),
				TeamId = team,
				TeamName = "team"
			});

			Target.Read().Single()
				.AgentsCount.Should().Be(1);
		}

		[Test]
		public void ShouldReadAgentsCountForTwoAgents()
		{
			var team = Guid.NewGuid();
			var businessUnitId = BusinessUnit.Current().Id.Value;
			Persister.Upsert(new AgentStateReadModelForTest
			{
				BusinessUnitId = businessUnitId,
				PersonId = Guid.NewGuid(),
				TeamId = team,
				TeamName = "team"
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				BusinessUnitId = businessUnitId,
				PersonId = Guid.NewGuid(),
				TeamId = team,
				TeamName = "team"
			});

			Target.Read().Single()
				.AgentsCount.Should().Be(2);
		}

	}
}