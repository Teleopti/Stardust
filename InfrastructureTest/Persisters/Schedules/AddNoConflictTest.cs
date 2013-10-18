using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class AddNoConflictTest : ScheduleRangePersisterIntegrationTest
	{
		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			return Enumerable.Empty<IPersistableScheduleData>();
		}

		protected override IEnumerable<IScheduleDay> When(IScheduleRange scheduleRange)
		{
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var day = scheduleRange.ScheduledDay(new DateOnly(2000, 1, 1));
			day.CreateAndAddActivity(Activity, new DateTimePeriod(start, start.AddHours(2)), ShiftCategory);
			return new [] {day};
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts, IScheduleRange scheduleRangeInMemory, IScheduleRange scheduleRangeInDatabase)
		{
			conflicts.Should().Be.Empty();
			scheduleRangeInMemory.ScheduledDay(new DateOnly(2000, 1, 1)).PersonAssignment().Should().Not.Be.Null();
			scheduleRangeInDatabase.ScheduledDay(new DateOnly(2000, 1, 1)).PersonAssignment().Should().Not.Be.Null();
		}
	}
}