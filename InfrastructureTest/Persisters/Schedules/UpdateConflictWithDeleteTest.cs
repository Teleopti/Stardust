using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class UpdateConflictWithDeleteTest : ScheduleRangeConflictTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);
		
		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			return new[] { new PersonAssignment(Person, Scenario, date) };
		}

		protected override void WhenOtherHasChanged(IScheduleRange othersScheduleRange)
		{
			var day = othersScheduleRange.ScheduledDay(date);
			day.Clear<IPersonAssignment>();
			DoModify(day);
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var day = myScheduleRange.ScheduledDay(date);
			day.CreateAndAddActivity(Activity, new DateTimePeriod(start, start.AddHours(2)), ShiftCategory);
			DoModify(day);
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			var conflict = conflicts.Single();
			conflict.ClientVersion.OriginalItem.Should().Not.Be.Null();
			conflict.ClientVersion.CurrentItem.Should().Not.Be.Null();
			conflict.DatabaseVersion.Should().Be.Null();
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
			myScheduleRange.ScheduledDay(date).PersonAssignment().ShiftLayers.Count()
										 .Should().Be.EqualTo(1);
		}
	}
}