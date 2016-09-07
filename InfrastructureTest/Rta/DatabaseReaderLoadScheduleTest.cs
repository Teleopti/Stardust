using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[UnitOfWorkTest]
	public class DatabaseReaderLoadScheduleTest
	{
		public IDatabaseReader Reader;
		public MutableNow Now;
		public IScheduleProjectionReadOnlyPersister Persister;

		[Test]
		public void ShouldFindByPersons()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var personId3 = Guid.NewGuid();
			Now.Is("2014-11-07 06:00");
			Persister.AddActivity(new ScheduleProjectionReadOnlyModel
			{
				BelongsToDate = "2014-11-07".Date(),
				PersonId = personId1,
				StartDateTime = "2014-11-07 10:00".Utc(),
				EndDateTime = "2014-11-07 10:00".Utc()
			});
			Persister.AddActivity(new ScheduleProjectionReadOnlyModel
			{
				BelongsToDate = "2014-11-07".Date(),
				PersonId = personId2,
				StartDateTime = "2014-11-07 10:00".Utc(),
				EndDateTime = "2014-11-07 10:00".Utc()
			});
			Persister.AddActivity(new ScheduleProjectionReadOnlyModel
			{
				BelongsToDate = "2014-11-07".Date(),
				PersonId = personId3,
				StartDateTime = "2014-11-07 10:00".Utc(),
				EndDateTime = "2014-11-07 10:00".Utc()
			});

			var result = Reader.GetCurrentSchedules(Now.UtcDateTime(), new[] {personId1, personId3});

			result.Select(x => x.PersonId).Should().Have.SameValuesAs(personId1, personId3);
		}

		[Test]
		public void ShouldReadBelongsToDate()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-07 06:00");
			Persister.AddActivity(new ScheduleProjectionReadOnlyModel
			{
				BelongsToDate = "2014-11-07".Date(),
				ScenarioId = Guid.NewGuid(),
				PersonId = personId,
				StartDateTime = "2014-11-07 10:00".Utc(),
				EndDateTime = "2014-11-07 10:00".Utc()
			});

			var result = Reader.GetCurrentSchedule(Now.UtcDateTime(), personId);

			result.Single().BelongsToDate.Should().Be(new DateOnly("2014-11-07".Utc()));
		}

		[Test]
		public void ShouldReadDateTimes()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-07 06:00");
			Persister.AddActivity(new ScheduleProjectionReadOnlyModel
			{
				BelongsToDate = "2014-11-07".Date(),
				ScenarioId = Guid.NewGuid(),
				PersonId = personId,
				StartDateTime = "2014-11-07 10:00".Utc(),
				EndDateTime = "2014-11-07 10:00".Utc()
			});

			var result = Reader.GetCurrentSchedule(Now.UtcDateTime(), personId);

			result.Single().StartDateTime.Should().Be("2014-11-07 10:00".Utc());
			result.Single().EndDateTime.Should().Be("2014-11-07 10:00".Utc());
		}

		[Test]
		public void ShouldReadDateTimesAsUtc()
		{
			var personId = Guid.NewGuid();
			Now.Is("2014-11-07 06:00");
			Persister.AddActivity(new ScheduleProjectionReadOnlyModel
			{
				BelongsToDate = "2014-11-07".Date(),
				ScenarioId = Guid.NewGuid(),
				PersonId = personId,
				StartDateTime = "2014-11-07 10:00".Utc(),
				EndDateTime = "2014-11-07 10:00".Utc()
			});

			var result = Reader.GetCurrentSchedule(Now.UtcDateTime(), personId);

			result.Single().StartDateTime.Kind.Should().Be(DateTimeKind.Utc);
			result.Single().EndDateTime.Kind.Should().Be(DateTimeKind.Utc);
		}
	}
}