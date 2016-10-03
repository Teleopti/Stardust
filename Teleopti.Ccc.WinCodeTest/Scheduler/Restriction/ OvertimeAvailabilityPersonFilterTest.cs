using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

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

			var target = new OvertimeAvailabilityPersonFilter();
			target.GetFilteredPerson(new[] {existingSchedule}, date, new TimePeriod(9, 0, 11, 0), TimeZoneInfo.Utc, true)
				.Should().Have.SameValuesAs(existingSchedule.Person);
		}
	}
}