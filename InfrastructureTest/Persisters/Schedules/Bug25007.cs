﻿using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	//OverwriteWhenHavingTwoNewAssignments
	public class Bug25007 : ScheduleRangePersisterBaseTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);
		
		protected override void Given(ICollection<IPersistableScheduleData> scheduleDataInDatabaseAtStart)
		{
		}

		protected override void WhenOtherHasChanged(IScheduleRange othersScheduleRange)
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

			var conflict = Target.Persist(myScheduleRange).Single();

			((IUnvalidatedScheduleRangeUpdate)myScheduleRange).SolveConflictBecauseOfExternalInsert(conflict.DatabaseVersion, false);
			var dayAfterConflict = myScheduleRange.ScheduledDay(date);
			DoModify(dayAfterConflict);
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			conflicts.Should().Be.Empty();
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
			myScheduleRange.ScheduledDay(date).PersonAssignment().Activities.Count()
			               .Should().Be.EqualTo(2);
		}
	} 
}