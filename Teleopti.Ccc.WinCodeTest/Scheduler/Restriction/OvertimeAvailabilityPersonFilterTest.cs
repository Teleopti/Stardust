using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Restriction
{
	public class OvertimeAvailabilityPersonFilterTest
	{
		[Test]
		public void ShouldReturnWhenIntersecting()
		{
			var date = DateOnly.Today;
			var existingSchedule = ScheduleDayFactory.Create(date, TimeZoneInfo.Utc);
			var overtimeAvail = new OvertimeAvailability(existingSchedule.Person, date, TimeSpan.FromHours(10), TimeSpan.FromHours(18));
			existingSchedule.Add(overtimeAvail);

			var target = new OvertimeAvailabilityPersonFilter(new FakeTimeZoneGuard(TimeZoneInfo.Utc));
			target.GetFilteredPerson(new[] {existingSchedule}, new TimePeriod(9, 0, 11, 0), true)
				.Should().Have.SameValuesAs(existingSchedule.Person);
		}
	}
}