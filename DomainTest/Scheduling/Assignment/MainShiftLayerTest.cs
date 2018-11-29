using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class MainShiftLayerTest
	{
		[Test]
		public void ShouldNotAcceptPeriodWithSeconds()
		{
			var someDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			Assert.Throws<ArgumentException>(() => new MainShiftLayer(new Activity("d"), new DateTimePeriod(someDate, someDate.AddHours(1).AddSeconds(1))));
		}
	}
}