﻿using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class UpdateNoConflictTest : ScheduleRangePersisterIntegrationTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override void Given(ICollection<IPersistableScheduleData> scheduleDataInDatabaseAtStart)
		{
			scheduleDataInDatabaseAtStart.Add(new PersonAssignment(Person, Scenario, date));
		}

		protected override void WhenOtherHasChanged(IScheduleRange othersScheduleRange)
		{
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var day = myScheduleRange.ScheduledDay(new DateOnly(2000, 1, 1));
			day.CreateAndAddActivity(Activity, new DateTimePeriod(start, start.AddHours(2)), ShiftCategory);
			DoModify(day);
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			conflicts.Should().Be.Empty();
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
			myScheduleRange.ScheduledDay(date).PersonAssignment().ShiftLayers.Count()
			               .Should().Be.EqualTo(1);
		}
	}
}