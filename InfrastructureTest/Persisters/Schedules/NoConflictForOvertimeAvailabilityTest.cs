using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	//Has no version number
	public class NoConflictForOvertimeAvailabilityTest : ScheduleRangeConflictTest
	{
		private readonly DateOnly date = new DateOnly(2000, 1, 1);

		protected override IEnumerable<IPersistableScheduleData> Given()
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
	}
}