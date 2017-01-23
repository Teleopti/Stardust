using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[UnitOfWorkTest]
	public class AgentStatePersisterScheduleTest
	{
		public IAgentStatePersister Target;
		
		[Test]
		public void ShouldPersistSchedule()
		{
			var personId = Guid.NewGuid();
			Target.Prepare(new AgentStatePrepare
			{
				PersonId = personId,
				ExternalLogons = new[] {new ExternalLogon {DataSourceId = 1, UserCode = "user"}}
			}, DeadLockVictim.Yes);
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
			}, true);

			var schedule = Target.Find(new ExternalLogon { UserCode = "user", DataSourceId = 1}, DeadLockVictim.Yes)
				.Single().Schedule.Single();
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
			}, DeadLockVictim.Yes);
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
			}, true);

			var schedule = Target.Find(new ExternalLogon { DataSourceId = 1, UserCode = "user" }, DeadLockVictim.Yes).Single().Schedule;
			schedule.Should().Have.Count.EqualTo(expected.Count());
		}

		[Test]
		public void ShouldSkipUpdateSchedule()
		{
			var personId = Guid.NewGuid();
			Target.Prepare(new AgentStatePrepare
			{
				PersonId = personId,
				ExternalLogons = new[] { new ExternalLogon { DataSourceId = 1, UserCode = "user" } }
			}, DeadLockVictim.Yes);
			Target.Update(new AgentState
			{
				PersonId = personId,
				Schedule = new[]
				{
					new ScheduledActivity
					{
						Name = "phone",
					}
				}
			}, true);

			Target.Update(new AgentState
			{
				PersonId = personId
			}, false);

			Target.Find(new ExternalLogon { DataSourceId = 1, UserCode = "user" }, DeadLockVictim.Yes).Single()
				.Schedule.Single().Name.Should().Be("phone");
		}

	}
}