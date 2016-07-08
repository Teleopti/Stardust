using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[UnitOfWorkWithLoginTest]
	[Toggle(Toggles.RTA_Optimize_39667)]
	public class DatabaseOptimizationTest
	{
		public IDatabaseOptimizer Target;
		public MutableNow Now;
		public IScheduleProjectionReadOnlyPersister SchedulePersister;
		public IAgentStatePersister AgentStatePersister;


		[Test]
		public void ShouldOnlyCallDatabaseOnce()
		{
			Now.Is("2014-11-07 06:00");
			var personId = Guid.NewGuid();
			var state = new AgentStateForTest { PersonId = personId };
			AgentStatePersister.Persist(state);
			SchedulePersister.AddActivity(new ScheduleProjectionReadOnlyModel
			{
				BelongsToDate = "2014-11-07".Date(),
				ScenarioId = Guid.NewGuid(),
				PersonId = personId,
				StartDateTime = "2014-11-07 10:00".Utc(),
				EndDateTime = "2014-11-07 10:00".Utc()
			});

			var stateAndSchedule = Target.LoadFor(personId);
			stateAndSchedule.AgentState.Should().Not.Be.Null();
			stateAndSchedule.Schedule.Single().BelongsToDate.Should().Be(new DateOnly("2014-11-07".Utc()));
		}

		[Test]
		public void ShouldGetCurrentActualAgentState()
		{
			var state = new AgentStateForTest { PersonId = Guid.NewGuid() };
			AgentStatePersister.Persist(state);

			var result = Target.LoadFor(state.PersonId);

			result.AgentState.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGetNullCurrentActualAgentStateIfNotFound()
		{
			var result = Target.LoadFor(Guid.NewGuid());

			result.AgentState.Should().Be.Null();
		}


		[Test]
		public void ShouldPersistModel()
		{
			var state = new AgentStateForTest();

			AgentStatePersister.Persist(state);

			Target.LoadFor(state.PersonId)
				.AgentState.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistBusinessUnit()
		{
			var businessUnitId = Guid.NewGuid();
			var state = new AgentStateForTest { BusinessUnitId = businessUnitId };

			AgentStatePersister.Persist(state);

			Target.LoadFor(state.PersonId)
				.AgentState.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPersistTeamId()
		{
			var teamId = Guid.NewGuid();
			var state = new AgentStateForTest { TeamId = teamId };

			AgentStatePersister.Persist(state);

			Target.LoadFor(state.PersonId)
				.AgentState.TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldPersistSiteId()
		{
			var siteId = Guid.NewGuid();
			var state = new AgentStateForTest { SiteId = siteId };

			AgentStatePersister.Persist(state);

			Target.LoadFor(state.PersonId)
				.AgentState.SiteId.Should().Be(siteId);
		}

		[Test]
		public void ShouldPersistModelWithNullValues()
		{
			var personId = Guid.NewGuid();

			AgentStatePersister.Persist(new AgentStateForTest
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
				StaffingEffect = null,
				Adherence = null
			});

			Target.LoadFor(personId)
				.AgentState
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistAlarmStartTime()
		{
			var state = new AgentStateForTest { AlarmStartTime = "2015-12-11 08:00".Utc() };

			AgentStatePersister.Persist(state);

			Target.LoadFor(state.PersonId).AgentState
				.AlarmStartTime.Should().Be("2015-12-11 08:00".Utc());
		}

		[Test]
		public void ShouldPersistTimeWindowCheckSum()
		{
			var state = new AgentStateForTest { TimeWindowCheckSum = 375 };

			AgentStatePersister.Persist(state);

			Target.LoadFor(state.PersonId)
				.AgentState
				.TimeWindowCheckSum.Should().Be(375);
		}
		
		[Test]
		public void ShouldReadBelongsToDate()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-07 06:00");
			SchedulePersister.AddActivity(new ScheduleProjectionReadOnlyModel
			{
				BelongsToDate = "2014-11-07".Date(),
				ScenarioId = Guid.NewGuid(),
				PersonId = personId,
				StartDateTime = "2014-11-07 10:00".Utc(),
				EndDateTime = "2014-11-07 10:00".Utc()
			});

			var result = Target.LoadFor(personId);

			result.Schedule.Single().BelongsToDate.Should().Be(new DateOnly("2014-11-07".Utc()));
		}

		[Test]
		public void ShouldReadDateTimes()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-07 06:00");
			SchedulePersister.AddActivity(new ScheduleProjectionReadOnlyModel
			{
				BelongsToDate = "2014-11-07".Date(),
				ScenarioId = Guid.NewGuid(),
				PersonId = personId,
				StartDateTime = "2014-11-07 10:00".Utc(),
				EndDateTime = "2014-11-07 10:00".Utc()
			});

			var result = Target.LoadFor(personId);

			result.Schedule.Single().StartDateTime.Should().Be("2014-11-07 10:00".Utc());
			result.Schedule.Single().EndDateTime.Should().Be("2014-11-07 10:00".Utc());
		}

		[Test]
		public void ShouldReadDateTimesAsUtc()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-07 06:00");
			SchedulePersister.AddActivity(new ScheduleProjectionReadOnlyModel
			{
				BelongsToDate = "2014-11-07".Date(),
				ScenarioId = Guid.NewGuid(),
				PersonId = personId,
				StartDateTime = "2014-11-07 10:00".Utc(),
				EndDateTime = "2014-11-07 10:00".Utc()
			});

			var result = Target.LoadFor(personId);

			result.Schedule.Single().StartDateTime.Kind.Should().Be(DateTimeKind.Utc);
			result.Schedule.Single().EndDateTime.Kind.Should().Be(DateTimeKind.Utc);
		}
	}

}