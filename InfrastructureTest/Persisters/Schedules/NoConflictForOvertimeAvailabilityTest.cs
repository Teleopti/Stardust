﻿using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	//Has no version number
	public class NoConflictForOvertimeAvailabilityTest : ScheduleRangePersisterBaseTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override IEnumerable<IAggregateRoot> Given()
		{
			return new[] { new OvertimeAvailability(Person, date, TimeSpan.FromHours(10), TimeSpan.FromHours(12)) };
		}

		protected override void WhenOtherHasChanged(IScheduleRange othersScheduleRange)
		{
			var day = othersScheduleRange.ScheduledDay(date);
			var overAvail = day.OvertimeAvailablityCollection().Single();
			overAvail.NotAvailable = !overAvail.NotAvailable;
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(date);
			var overAvail = day.OvertimeAvailablityCollection().Single();
			overAvail.NotAvailable = !overAvail.NotAvailable;
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			conflicts.Should().Be.Empty();
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
		}
	}
}