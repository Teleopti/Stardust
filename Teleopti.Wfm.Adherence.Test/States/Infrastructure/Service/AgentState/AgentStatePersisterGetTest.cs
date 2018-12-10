using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.Service.AgentState
{
	[TestFixture]
	[UnitOfWorkTest]
	public class AgentStatePersisterGetTest
	{
		public IAgentStatePersister Persister;

		[Test]
		public void ShouldGetByPersonIds()
		{
			var state1 = new AgentStateForUpsert {PersonId = Guid.NewGuid()};
			var state2 = new AgentStateForUpsert {PersonId = Guid.NewGuid()};
			var state3 = new AgentStateForUpsert {PersonId = Guid.NewGuid()};
			Persister.Upsert(state1);
			Persister.Upsert(state2);
			Persister.Upsert(state3);

			var result = Persister.ReadForTest(new[] {state1.PersonId, state3.PersonId});

			result.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(state1.PersonId, state3.PersonId);
		}

		[Test]
		public void ShouldGetWithAllData()
		{
			var writer = Persister;
			var state = new AgentStateForUpsert
			{
				PersonId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid(),
				TeamId = Guid.NewGuid(),
				SiteId = Guid.NewGuid(),
				RuleId = Guid.NewGuid(),
				RuleStartTime = "2014-11-11 10:33".Utc(),
				SnapshotId = "2014-11-11 10:34".Utc(),
				SnapshotDataSourceId = 1,
				ReceivedTime = "2014-11-11 10:36".Utc(),
				StateGroupId = Guid.NewGuid(),
				StateStartTime = "2014-11-11 10:37".Utc()
			};
			writer.Upsert(state);

			var result = Persister.ReadForTest(new[] {state.PersonId})
				.Single();

			result.PersonId.Should().Be(state.PersonId);
			result.BusinessUnitId.Should().Be(state.BusinessUnitId);
			result.TeamId.Should().Be(state.TeamId);
			result.SiteId.Should().Be(state.SiteId);
			result.RuleId.Should().Be(state.RuleId);
			result.RuleStartTime.Should().Be(state.RuleStartTime);
			result.SnapshotId.Should().Be(state.SnapshotId);
			result.SnapshotDataSourceId.Should().Be(state.SnapshotDataSourceId);
			result.ReceivedTime.Should().Be(state.ReceivedTime);
			result.StateGroupId.Should().Be(state.StateGroupId);
			result.StateStartTime.Should().Be(state.StateStartTime);
		}
	}
}