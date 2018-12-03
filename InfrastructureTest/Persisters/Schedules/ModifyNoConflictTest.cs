using System;
using System.Collections.Generic;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class ModifyNoConflictTest : ScheduleRangeConflictTest
	{
		private readonly DateOnly date = new DateOnly(2001, 1, 1);

		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			return new[] { new PersonAssignment(Person, Scenario, date) };
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var start = new DateTime(2001, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var day = myScheduleRange.ScheduledDay(date);
			day.CreateAndAddActivity(Activity, new DateTimePeriod(start, start.AddHours(2)), ShiftCategory);
			DoModify(day);
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			conflicts.Should().Be.Empty();
		}
	}
}