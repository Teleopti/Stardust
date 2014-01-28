using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class MainShiftLayerTest
	{
		[Test, ExpectedException(typeof(ArgumentException))]
		public void ShouldNotAcceptPeriodWithSeconds()
		{
			var someDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			new MainShiftLayer(new Activity("d"), new DateTimePeriod(someDate, someDate.AddHours(1).AddSeconds(1)));
		}

		[Test]
		public void ShouldKnowItsIndex()
		{
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeMainshiftLayers();
			((MainShiftLayer)ass.MainActivities().Last()).OrderIndex.Should().Be.EqualTo(2);
		}
	}
}