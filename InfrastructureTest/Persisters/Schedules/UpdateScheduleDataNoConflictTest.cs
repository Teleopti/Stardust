using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class UpdateScheduleDataNoConflictTest : ScheduleRangePersisterIntegrationTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			return new[] { new PersonAssignment(Person, Scenario, date) };
		}

		protected override IEnumerable<IScheduleDay> When(IScheduleRange scheduleRange)
		{
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var day = scheduleRange.ScheduledDay(new DateOnly(2000, 1, 1));
			day.CreateAndAddActivity(Activity, new DateTimePeriod(start, start.AddHours(2)), ShiftCategory);
			return new[] { day };
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts, IScheduleRange scheduleRangeInMemory, IScheduleRange scheduleRangeInDatabase)
		{
			conflicts.Should().Be.Empty();
			var ass1 = scheduleRangeInMemory.ScheduledDay(new DateOnly(2000, 1, 1)).PersonAssignment();
			var ass2 = scheduleRangeInDatabase.ScheduledDay(new DateOnly(2000, 1, 1)).PersonAssignment();
			ass1.ShiftLayers.Count().Should().Be.EqualTo(1);
			ass2.ShiftLayers.Count().Should().Be.EqualTo(1);
		}
	}
}