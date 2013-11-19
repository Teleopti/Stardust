﻿using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class DeleteConflictWithUpdateTest : ScheduleRangePersisterBaseTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override void Given(ICollection<IPersistableScheduleData> scheduleDataInDatabaseAtStart)
		{
			scheduleDataInDatabaseAtStart.Add(new PersonAssignment(Person, Scenario, date));
		}

		protected override void WhenOtherHasChanged(IScheduleRange othersScheduleRange)
		{
			var start = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var day = othersScheduleRange.ScheduledDay(date);
			day.PersonAssignment().AssignActivity(Activity, new DateTimePeriod(start, start.AddHours(3)));
			DoModify(day);
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(date);
			day.Clear<IPersonAssignment>();
			DoModify(day);
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			var conflict = conflicts.Single();
			conflict.DatabaseVersion.Should().Not.Be.Null();
			conflict.ClientVersion.CurrentItem.Should().Be.Null();
			conflict.ClientVersion.OriginalItem.Should().Not.Be.Null();
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
			myScheduleRange.ScheduledDay(date).PersonAssignment()
			               .Should().Be.Null();
		}
	}
}