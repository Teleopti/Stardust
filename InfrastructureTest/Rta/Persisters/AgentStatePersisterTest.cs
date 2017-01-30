using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Helper;
using ExternalLogon = Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.ExternalLogon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	public static class AgentStatePersisterExtensions
	{
		public static IEnumerable<AgentState> ReadForTest(this IAgentStatePersister instance)
		{
			return instance.LockNLoad(instance.FindForCheck(), DeadLockVictim.Yes).AgentStates;
		}

		public static IEnumerable<AgentState> ReadForTest(this IAgentStatePersister instance, IEnumerable<Guid> personIds)
		{
			return instance.LockNLoad(personIds, DeadLockVictim.Yes).AgentStates;
		}

		public static IEnumerable<AgentState> ReadForTest(this IAgentStatePersister instance, IEnumerable<ExternalLogon> externalLogons)
		{
			return instance.LockNLoad(externalLogons, DeadLockVictim.Yes).AgentStates;
		}

		public static IEnumerable<AgentState> ReadForTest(this IAgentStatePersister instance, ExternalLogon externalLogon)
		{
			return instance.LockNLoad(new[] { externalLogon }, DeadLockVictim.Yes).AgentStates;
		}
	}

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

			Target.ReadForTest(new ExternalLogon {UserCode = "usercode"}).Single()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistBusinessUnit()
		{
			var businessUnitId = Guid.NewGuid();
			var state = new AgentStateForUpsert { BusinessUnitId = businessUnitId};

			Target.Upsert(state);

			Target.ReadForTest(new ExternalLogon { UserCode = "usercode" }).Single()
				.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPersistTeamId()
		{
			var teamId = Guid.NewGuid();
			var state = new AgentStateForUpsert { TeamId = teamId};

			Target.Upsert(state);

			Target.ReadForTest(new ExternalLogon { UserCode = "usercode" }).Single()
				.TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldPersistSiteId()
		{
			var siteId = Guid.NewGuid();
			var state = new AgentStateForUpsert { SiteId = siteId};

			Target.Upsert(state);

			Target.ReadForTest(new ExternalLogon { UserCode = "usercode" }).Single()
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
				PlatformTypeId = Guid.NewGuid(),
				SourceId = null,
				ReceivedTime = "2015-01-02 10:00".Utc(),
				BatchId = null,

				StateCode = null,
				StateGroupId = null,
				StateStartTime = null,

				RuleId = null,
				RuleStartTime = null,
				AlarmStartTime = null,
			});

			Target.ReadForTest(new ExternalLogon { UserCode = "usercode" }).Single()
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistAlarmStartTime()
		{
			var state = new AgentStateForUpsert { AlarmStartTime = "2015-12-11 08:00".Utc()};

			Target.Upsert(state);

			Target.ReadForTest(new ExternalLogon { UserCode = "usercode" }).Single()
				.AlarmStartTime.Should().Be("2015-12-11 08:00".Utc());
		}

		[Test]
		public void ShouldPersistTimeWindowCheckSum()
		{
			var state = new AgentStateForUpsert {TimeWindowCheckSum = 375};

			Target.Upsert(state);

			Target.ReadForTest(new ExternalLogon { UserCode = "usercode" }).Single()
				.TimeWindowCheckSum.Should().Be(375);
		}

		[Test]
		public void ShouldUpdateTimeWindowCheckSum()
		{
			var state = new AgentStateForUpsert { TimeWindowCheckSum = 375 };
			Target.Upsert(state);
			state.TimeWindowCheckSum = 475;

			Target.Upsert(state);

			Target.ReadForTest(new ExternalLogon { UserCode = "usercode" }).Single()
				.TimeWindowCheckSum.Should().Be(475);
		}

		[Test]
		public void ShouldPersistAdherence()
		{
			var state = new AgentStateForUpsert {Adherence = EventAdherence.Neutral};

			Target.Upsert(state);

			Target.ReadForTest(new ExternalLogon { UserCode = "usercode" }).Single()
				.Adherence.Should().Be(EventAdherence.Neutral);
		}

		[Test]
		public void ShouldUpdateAdherence()
		{
			var state = new AgentStateForUpsert { Adherence = EventAdherence.Neutral };

			Target.Upsert(state);

			state.Adherence = EventAdherence.In;

			Target.Update(state);

			Target.ReadForTest(new ExternalLogon { UserCode = "usercode" }).Single()
				.Adherence.Should().Be(EventAdherence.In);
		}

		[Test]
		public void ShouldDelete()
		{
			var personId = Guid.NewGuid();
			var model = new AgentStateForUpsert { PersonId = personId };
			Target.Upsert(model);

			Target.Delete(personId, DeadLockVictim.Yes);

			Target.ReadForTest(new ExternalLogon { UserCode = "usercode" })
				.Should().Be.Empty();
		}
		
	}
}