using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState.Reader
{
	[TestFixture]
	[UnitOfWorkTest]
	public class ForTeamTest
	{
		public IAgentStateReadModelReader Target;
		public IAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldLoadAgentStateByTeamId()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId
			});

			var result = Target.Read(new AgentStateFilter {TeamIds = teamId.AsArray()});

			result.Single().PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldLoadAgentStatesByTeamId()
		{
			var teamId = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest {TeamId = teamId, PersonId = Guid.NewGuid()});
			Persister.Upsert(new AgentStateReadModelForTest {TeamId = teamId, PersonId = Guid.NewGuid()});
			Persister.Upsert(new AgentStateReadModelForTest {TeamId = Guid.Empty, PersonId = Guid.NewGuid()});

			var result = Target.Read(new AgentStateFilter {TeamIds = teamId.AsArray()});

			result.Count().Should().Be(2);
		}

		[Test]
		public void ShouldNotLoadDeletedAgetnsForTeam()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId2
			});
			Persister.UpsertNoAssociation(personId2);

			var result = Target.Read(new AgentStateFilter {TeamIds = teamId.AsArray()});

			result.Single().PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldLoadAgentStatesByTeamIds()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest {TeamId = teamId1, PersonId = Guid.NewGuid()});
			Persister.Upsert(new AgentStateReadModelForTest {TeamId = teamId2, PersonId = Guid.NewGuid()});
			Persister.Upsert(new AgentStateReadModelForTest {TeamId = Guid.Empty, PersonId = Guid.NewGuid()});

			var result = Target.Read(new AgentStateFilter {TeamIds = new[] {teamId1, teamId2}});

			result.Count().Should().Be(2);
		}

	}
}