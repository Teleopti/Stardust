using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class MainShiftActivityLayerNewTest
	{
		[Test, ExpectedException(typeof(ArgumentException))]
		public void ShouldNotAcceptPeriodWithSeconds()
		{
			var someDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			new MainShiftActivityLayerNew(new Activity("d"), new DateTimePeriod(someDate, someDate.AddHours(1).AddSeconds(1)));
		}
	}
}