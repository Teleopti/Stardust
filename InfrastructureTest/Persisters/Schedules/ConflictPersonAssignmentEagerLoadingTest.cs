﻿using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class ConflictPersonAssignmentEagerLoadingTest : ScheduleRangePersisterBaseTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override void Given(ICollection<INonversionedPersistableScheduleData> scheduleDataInDatabaseAtStart)
		{
			var ass = new PersonAssignment(Person, Scenario, date);
			scheduleDataInDatabaseAtStart.Add(ass);
		}

		protected override void WhenOtherHasChanged(IScheduleRange othersScheduleRange)
		{
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var day = othersScheduleRange.ScheduledDay(date);
			day.CreateAndAddOvertime(Activity, new DateTimePeriod(start, start.AddHours(2)), DefinitionSet);
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
			var dbConflict = (IPersonAssignment)conflicts.Single().DatabaseVersion;

			LazyLoadingManager.IsInitialized(dbConflict.Person).Should().Be.True();
			LazyLoadingManager.IsInitialized(dbConflict.UpdatedBy).Should().Be.True();
			LazyLoadingManager.IsInitialized(dbConflict.ShiftLayers.Single().Payload).Should().Be.True();
			LazyLoadingManager.IsInitialized(dbConflict.OvertimeLayers().Single().DefinitionSet).Should().Be.True();
			LazyLoadingManager.IsInitialized(dbConflict.ShiftCategory).Should().Be.True();
			LazyLoadingManager.IsInitialized(dbConflict.Scenario).Should().Be.True();
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
		}
	}
}