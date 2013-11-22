using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
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
		public void NoneEntityCloneShouldClearParent()
		{
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var ass = new PersonAssignment(new Person(), new Scenario("d"), new DateOnly(2000, 1, 1));
			ass.AddActivity(new Activity("d"), new DateTimePeriod(start, start.AddHours(1)));
			var layer2check = ass.MainLayers().Single();
			layer2check.Parent.Should().Not.Be.Null();
			var clone = (IMainShiftLayer)layer2check.NoneEntityClone();
			clone.Parent.Should().Be.Null();
		}

		[Test]
		public void ShouldKnowItsIndex()
		{
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeMainshiftLayers();
			((MainShiftLayer)ass.MainLayers().Last()).OrderIndex.Should().Be.EqualTo(2);
		}
	}
}