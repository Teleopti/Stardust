﻿using System.Collections.Generic;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class Bug25715 : ScheduleRangeConflictTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override IEnumerable<IAggregateRoot> Given()
		{
			return new[] {new OvertimeAvailability(Person, date, null, null)};
		}

		protected override void WhenOtherHasChanged(IScheduleRange othersScheduleRange)
		{
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(date);
			day.Add(new OvertimeAvailability(Person, date, null, null));
			DoModify(day);
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			conflicts.Should().Be.Empty();
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(date);
			day.OvertimeAvailablityCollection().Count.Should().Be.EqualTo(2);
		}
	}
}