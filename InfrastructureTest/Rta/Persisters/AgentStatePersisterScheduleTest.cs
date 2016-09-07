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
	public class AgentStatePersisterScheduleTest
	{
		public IAgentStatePersister Target;
		public IScheduleProjectionReadOnlyPersister Persister;
		public IScheduleProjectionReadOnlyReader Reader;
		
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

		[Test]
		public void ShouldPersistLargeSchedule()
		{
			var personId = Guid.NewGuid();
			Target.Prepare(new AgentStatePrepare
			{
				PersonId = personId,
				ExternalLogons = new[] { new ExternalLogon { DataSourceId = 1, UserCode = "user" } }
			});
			var expected = Enumerable.Range(0, 30)
				.Select(h =>
				{
					var time = "2016-09-07 03:00".Utc().AddHours(h);
					return new ScheduledActivity
					{
						PersonId = personId,
						PayloadId = Guid.NewGuid(),
						BelongsToDate = "2016-09-07".Date(),
						StartDateTime = time,
						EndDateTime = time.AddHours(1),
						Name = "Activity name " + h,
						ShortName = "Short name " + h,
						DisplayColor = Color.Green.ToArgb(),
					};
				});
			Target.Update(new AgentState
			{
				PersonId = personId,
				Schedule = expected
			});

			var schedule = Target.Get(personId).Schedule;
			schedule.Should().Have.Count.EqualTo(expected.Count());
		}

	}
}