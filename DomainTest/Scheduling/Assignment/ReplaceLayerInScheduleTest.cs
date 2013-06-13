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
	public class ReplaceLayerInScheduleTest
	{
		 [Test]
		 public void ShouldReplaceMainShiftLayer()
		 {
			 var target = new ReplaceLayerInSchedule();
			 var scheduleDay = new SchedulePartFactoryForDomain().AddMainShiftLayer().CreatePart();
			 var newPayload = new Activity("d");
			 var newPeriod = new DateTimePeriod();
#pragma warning disable 612,618
			 var orgLayerCollection = scheduleDay.AssignmentHighZOrder().ToMainShift().LayerCollection;
			 target.Replace(scheduleDay, orgLayerCollection.First(), newPayload, newPeriod)
				 .Should().Be.True();

			 var newLayerCollection = scheduleDay.AssignmentHighZOrder().ToMainShift().LayerCollection;
			 orgLayerCollection.Count.Should().Be.EqualTo(newLayerCollection.Count);
			 var newLayer = newLayerCollection.First();
			 newLayer.Payload.Should().Be.SameInstanceAs(newPayload);
			 newLayer.Period.Should().Be.EqualTo(newPeriod);
#pragma warning restore 612,618
		 }

		[Test]
		public void ShouldNotReplaceMainShiftLayer()
		{
			var target = new ReplaceLayerInSchedule();
			var scheduleDay = new SchedulePartFactoryForDomain().AddMainShiftLayer().CreatePart();
#pragma warning disable 612,618
			var orgLayerCollection = scheduleDay.AssignmentHighZOrder().ToMainShift().LayerCollection;
			target.Replace(scheduleDay, orgLayerCollection.First(), new Activity("#d"), new DateTimePeriod()).Should().Be.True();
			scheduleDay.AssignmentHighZOrder().ToMainShift().LayerCollection
				.Should().Have.SameSequenceAs(orgLayerCollection);
#pragma warning restore 612,618
		}
	}
}