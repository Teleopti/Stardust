using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.States.Events;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.Service.AgentState
{
	[TestFixture]
	[UnitOfWorkTest]
	public class AgentStatePersisterTest
	{
		public IAgentStatePersister Target;
		public IScheduleProjectionReadOnlyPersister Persister;
		public IScheduleReader Reader;

		[Test]
		public void ShouldPersistModel()
		{
			var state = new AgentStateForUpsert();

			Target.Upsert(state);

			Target.ReadForTest(state.PersonId).Single()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistBusinessUnit()
		{
			var businessUnitId = Guid.NewGuid();
			var state = new AgentStateForUpsert {BusinessUnitId = businessUnitId};

			Target.Upsert(state);

			Target.ReadForTest(state.PersonId).Single()
				.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPersistTeamId()
		{
			var teamId = Guid.NewGuid();
			var state = new AgentStateForUpsert {TeamId = teamId};

			Target.Upsert(state);

			Target.ReadForTest(state.PersonId).Single()
				.TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldPersistSiteId()
		{
			var siteId = Guid.NewGuid();
			var state = new AgentStateForUpsert {SiteId = siteId};

			Target.Upsert(state);

			Target.ReadForTest(state.PersonId).Single()
				.SiteId.Should().Be(siteId);
		}

		[Test]
		public void ShouldPersistModelWithNullValues()
		{
			var personId = Guid.NewGuid();

			Target.Upsert(new AgentStateForUpsert
			{
				PersonId = personId,
				BusinessUnitId = Guid.NewGuid(),
				TeamId = null,
				SiteId = null,
				ReceivedTime = "2015-01-02 10:00".Utc(),
				SnapshotId = null,

				StateGroupId = null,
				StateStartTime = null,

				RuleId = null,
				RuleStartTime = null,
				AlarmStartTime = null,
			});

			Target.ReadForTest(personId).Single()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistAlarmStartTime()
		{
			var state = new AgentStateForUpsert {AlarmStartTime = "2015-12-11 08:00".Utc()};

			Target.Upsert(state);

			Target.ReadForTest(state.PersonId).Single()
				.AlarmStartTime.Should().Be("2015-12-11 08:00".Utc());
		}

		[Test]
		public void ShouldPersistTimeWindowCheckSum()
		{
			var state = new AgentStateForUpsert {TimeWindowCheckSum = 375};

			Target.Upsert(state);

			Target.ReadForTest(state.PersonId).Single()
				.TimeWindowCheckSum.Should().Be(375);
		}

		[Test]
		public void ShouldUpdateTimeWindowCheckSum()
		{
			var state = new AgentStateForUpsert {TimeWindowCheckSum = 375};
			Target.Upsert(state);
			state.TimeWindowCheckSum = 475;

			Target.Upsert(state);

			Target.ReadForTest(state.PersonId).Single()
				.TimeWindowCheckSum.Should().Be(475);
		}

		[Test]
		public void ShouldPersistAdherence()
		{
			var state = new AgentStateForUpsert {Adherence = EventAdherence.Neutral};

			Target.Upsert(state);

			Target.ReadForTest(state.PersonId).Single()
				.Adherence.Should().Be(EventAdherence.Neutral);
		}

		[Test]
		public void ShouldUpdateAdherence()
		{
			var state = new AgentStateForUpsert {Adherence = EventAdherence.Neutral};

			Target.Upsert(state);

			state.Adherence = EventAdherence.In;

			Target.Update(state);

			Target.ReadForTest(state.PersonId).Single()
				.Adherence.Should().Be(EventAdherence.In);
		}

		[Test]
		public void ShouldDelete()
		{
			var personId = Guid.NewGuid();
			var state = new AgentStateForUpsert {PersonId = personId};
			Target.Upsert(state);

			Target.Delete(personId, DeadLockVictim.Yes);

			Target.ReadForTest(state.PersonId)
				.Should().Be.Empty();
		}
	}
}