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
		public void ShouldKeepParentWhenEntityClone()
		{
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var ass = new PersonAssignment(new Person(), new Scenario("d"), new DateOnly(2000, 1, 1));
			ass.AddPersonalActivity(new Activity("d"), new DateTimePeriod(start, start.AddHours(1)));
			var layer2check = ass.PersonalActivities().Single();
			layer2check.Parent.Should().Be.SameInstanceAs(ass);
			var clone = (IPersonalShiftLayer)layer2check.NoneEntityClone();
			clone.Parent.Should().Be.Null();
		}

		[Test]
		public void ShouldKnowItsIndex()
		{
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var ass = new PersonAssignment(new Person(), new Scenario("d"), new DateOnly(2000, 1, 1));
			ass.AddPersonalActivity(new Activity("sdf"), new DateTimePeriod(start, start.AddHours(1)));
			ass.AddPersonalActivity(new Activity("sdf"), new DateTimePeriod(start, start.AddHours(1)));
			ass.AddPersonalActivity(new Activity("sdf"), new DateTimePeriod(start, start.AddHours(1)));
			((PersonalShiftLayer)ass.PersonalActivities().Last()).OrderIndex.Should().Be.EqualTo(2);
		}
	}
}