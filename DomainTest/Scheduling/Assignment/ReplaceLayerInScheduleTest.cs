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
		 public void ReplaceMainShiftLayer()
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
	}
}