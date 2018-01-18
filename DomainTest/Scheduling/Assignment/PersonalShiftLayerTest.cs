using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class PersonalShiftLayerTest
	{
		
		[Test]
		public void ShouldKnowItsIndex()
		{
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var ass = new PersonAssignment(new Person(), new Scenario("d"), new DateOnly(2000, 1, 1));
			ass.AddPersonalActivity(new Activity("act1"), new DateTimePeriod(start, start.AddHours(1)));
			ass.AddPersonalActivity(new Activity("act2"), new DateTimePeriod(start, start.AddHours(1)));
			ass.AddPersonalActivity(new Activity("act3"), new DateTimePeriod(start, start.AddHours(1)));
			
			((PersonalShiftLayer)ass.PersonalActivities().Last()).OrderIndex.Should().Be.EqualTo(2);
		}
	}
}