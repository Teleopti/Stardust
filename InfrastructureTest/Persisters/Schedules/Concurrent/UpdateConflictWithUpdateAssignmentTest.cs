using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules.Concurrent
{
	public class UpdateConflictWithUpdateAssignmentTest : ScheduleRangeConcurrentTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			return new[] { new PersonAssignment(Person, Scenario, date) };
		}

		protected override void WhenOtherIsChanging(IScheduleRange othersScheduleRange)
		{
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var day = othersScheduleRange.ScheduledDay(date);
			day.CreateAndAddActivity(Activity, new DateTimePeriod(start, start.AddHours(2)), ShiftCategory);
			DoModify(day);
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var day = myScheduleRange.ScheduledDay(date);
			day.CreateAndAddActivity(Activity, new DateTimePeriod(start, start.AddHours(2)), ShiftCategory);
			day.CreateAndAddActivity(Activity, new DateTimePeriod(start, start.AddHours(2)), ShiftCategory);
			DoModify(day);
		}

		protected override void ThenOneRangeHadConflicts(IScheduleRange unsavedScheduleRangeWithConflicts, IEnumerable<PersistConflict> conflicts,
			bool myScheduleRangeWasTheOneWithConflicts)
		{
			unsavedScheduleRangeWithConflicts.ScheduledDay(date).PersonAssignment().ShiftLayers.Count()
				.Should().Be.EqualTo(myScheduleRangeWasTheOneWithConflicts ? 2 : 1);

			var conflict = conflicts.Single();
			conflict.ClientVersion.OriginalItem.Should().Not.Be.Null();
			conflict.ClientVersion.CurrentItem.Should().Not.Be.Null();
			conflict.DatabaseVersion.Should().Not.Be.Null();
		}
	}
}