using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[UnitOfWorkTest]
	public class AgentStatePersisterTest
	{
		public IAgentStatePersister Target;
		public IScheduleProjectionReadOnlyPersister Persister;
		public IScheduleProjectionReadOnlyReader Reader;

		[Test]
		public void ShouldPersistModel()
		{
			var state = new AgentStateForUpsert();

			Target.Upsert(state);

			var result = Target.Get(state.PersonId);
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistBusinessUnit()
		{
			var businessUnitId = Guid.NewGuid();
			var state = new AgentStateForUpsert { BusinessUnitId = businessUnitId};

			Target.Upsert(state);

			Target.Get(state.PersonId)
				.BusinessUnitId.Should().Be(businessUnitId);
		}

		[Test]
		public void ShouldPersistTeamId()
		{
			var teamId = Guid.NewGuid();
			var state = new AgentStateForUpsert { TeamId = teamId};

			Target.Upsert(state);

			Target.Get(state.PersonId)
				.TeamId.Should().Be(teamId);
		}

		[Test]
		public void ShouldPersistSiteId()
		{
			var siteId = Guid.NewGuid();
			var state = new AgentStateForUpsert { SiteId = siteId};

			Target.Upsert(state);

			Target.Get(state.PersonId)
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

			Target.Get(personId)
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldPersistAlarmStartTime()
		{
			var state = new AgentStateForUpsert { AlarmStartTime = "2015-12-11 08:00".Utc()};

			Target.Upsert(state);

			Target.Get(state.PersonId)
				.AlarmStartTime.Should().Be("2015-12-11 08:00".Utc());
		}

		[Test]
		public void ShouldPersistTimeWindowCheckSum()
		{
			var state = new AgentStateForUpsert {TimeWindowCheckSum = 375};

			Target.Upsert(state);

			Target.Get(state.PersonId)
				.TimeWindowCheckSum.Should().Be(375);
		}

		[Test]
		public void ShouldUpdateTimeWindowCheckSum()
		{
			var state = new AgentStateForUpsert { TimeWindowCheckSum = 375 };
			Target.Upsert(state);
			state.TimeWindowCheckSum = 475;

			Target.Upsert(state);

			Target.Get(state.PersonId)
				.TimeWindowCheckSum.Should().Be(475);
		}

		[Test]
		public void ShouldDelete()
		{
			var personId = Guid.NewGuid();
			var model = new AgentStateForUpsert { PersonId = personId };
			Target.Upsert(model);

			Target.Delete(personId);

			Target.Get(personId).Should()
				.Be.Null();
		}

		[Test]
		public void ShouldPersistSchedule()
		{
			var personId = Guid.NewGuid();
			Target.Prepare(new AgentStatePrepare
			{
				PersonId = personId,
				ExternalLogons = new[] {new ExternalLogon {DataSourceId = 1, UserCode = "user"}}
			});
			var expected = new ScheduledActivity
			{
				PersonId = personId,
				PayloadId = Guid.NewGuid(),
				BelongsToDate = "2016-09-07".Date(),
				StartDateTime = "2016-09-07 08:00".Utc(),
				EndDateTime = "2016-09-07 17:00".Utc(),
				Name = "phone",
				ShortName = "ph",
				DisplayColor = Color.Green.ToArgb(),
			};
			Target.Update(new AgentState
			{
				PersonId = personId,
				Schedule = new[]
				{
					expected
				}
			});

			var schedule = Target.Get(personId).Schedule.Single();
			schedule.PersonId.Should().Be(personId);
			schedule.PayloadId.Should().Be(expected.PayloadId);
			schedule.BelongsToDate.Should().Be("2016-09-07".Date());
			schedule.StartDateTime.Should().Be("2016-09-07 08:00".Utc());
			schedule.EndDateTime.Should().Be("2016-09-07 17:00".Utc());
			schedule.Name.Should().Be("phone");
			schedule.ShortName.Should().Be("ph");
			schedule.DisplayColor.Should().Be(Color.Green.ToArgb());
		}

		[Test]
		public void ShouldPersistReadSchedule()
		{
			var personId = Guid.NewGuid();
			var scenarioId = Guid.NewGuid();
			Target.Prepare(new AgentStatePrepare
			{
				PersonId = personId,
				ExternalLogons = new[] { new ExternalLogon { DataSourceId = 1, UserCode = "user" } }
			});
			var expected = new ScheduleProjectionReadOnlyModel
			{
				PersonId = personId,
				PayloadId = Guid.NewGuid(),
				BelongsToDate = "2016-09-07".Date(),
				StartDateTime = "2016-09-07 08:00".Utc(),
				EndDateTime = "2016-09-07 17:00".Utc(),
				Name = "phone",
				ShortName = "ph",
				DisplayColor = Color.Green.ToArgb(),
			};
			Persister.BeginAddingSchedule("2016-09-07".Date(), scenarioId, personId, 1);
			Persister.AddActivity(expected);

			var schedules = Reader.GetCurrentSchedule("2016-09-07 11:00".Utc(), personId);
			Target.Update(new AgentState
			{
				PersonId = personId,
				Schedule = schedules
			});
			var schedule = Target.Get(personId).Schedule.Single();

			schedule.PersonId.Should().Be(personId);
			schedule.PayloadId.Should().Be(expected.PayloadId);
			schedule.BelongsToDate.Should().Be("2016-09-07".Date());
			schedule.StartDateTime.Should().Be("2016-09-07 08:00".Utc());
			schedule.EndDateTime.Should().Be("2016-09-07 17:00".Utc());
			schedule.Name.Should().Be("phone");
			schedule.ShortName.Should().Be("ph");
			schedule.DisplayColor.Should().Be(Color.Green.ToArgb());
		}
	}
}