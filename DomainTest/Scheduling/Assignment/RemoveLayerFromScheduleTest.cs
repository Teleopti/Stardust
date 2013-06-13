using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class RemoveLayerFromScheduleTest
    {
	      [Test]
        public void VerifyRemovesMainShiftLayer()
	      {
		      var target = new RemoveLayerFromSchedule();
            var schedulePart = new SchedulePartFactoryForDomain().AddMainShiftLayer().CreatePart();
            //Get the first layer in the mainshift..
#pragma warning disable 612,618
						var mainShift = schedulePart.AssignmentHighZOrder().ToMainShift();
#pragma warning restore 612,618
            ILayer<IActivity> firstLayer = mainShift.LayerCollection.First();
            Assert.IsTrue(mainShift.LayerCollection.Contains(firstLayer),"Verify contains the layer");
						target.Remove(schedulePart, firstLayer);
						Assert.AreEqual(0, schedulePart.PersonAssignmentCollection().Count);
        }

        [Test]
        public void VerifyRemovesPersonalShiftLayer()
        {
					var target = new RemoveLayerFromSchedule();
            var schedulePart = new SchedulePartFactoryForDomain().AddMainShiftLayer().AddPersonalLayer().CreatePart();
           
            ILayer<IActivity> firstPersonalLayer =
								schedulePart.AssignmentHighZOrder().PersonalShiftCollection.First().LayerCollection.First();

						Assert.IsTrue(schedulePart.AssignmentHighZOrder().PersonalShiftCollection.First().LayerCollection.Contains(firstPersonalLayer), "Verify contains the layer");
						target.Remove(schedulePart, firstPersonalLayer);
						Assert.AreEqual(0, schedulePart.AssignmentHighZOrder().PersonalShiftCollection.Count);
        }

        [Test]
        public void VerifyRemovesPersonAbsence()
        {
					var target = new RemoveLayerFromSchedule();
            var schedulePart = new SchedulePartFactoryForDomain().AddMainShiftLayer().AddAbsence().CreatePart();

						var firstPersonAbsence = schedulePart.PersonAbsenceCollection().First();
            IAbsenceLayer firstAbsenceLayer = firstPersonAbsence.Layer;

						Assert.IsTrue(schedulePart.PersonAbsenceCollection().Contains(firstPersonAbsence), "Verify contains the absence");
						target.Remove(schedulePart, firstAbsenceLayer);
						Assert.IsFalse(schedulePart.PersonAbsenceCollection().Contains(firstPersonAbsence), "The PersonAbsence containing the layer should have been removed");
        }

        [Test]
        public void VerifyRemovesOvertimeLayer()
        {
					var target = new RemoveLayerFromSchedule();
            var schedulePart = new SchedulePartFactoryForDomain().AddMainShiftLayer().AddOvertime().CreatePart();

						IPersonAssignment assignmnet = schedulePart.AssignmentHighZOrder();
            IOvertimeShift firstOvertimeShift = assignmnet.OvertimeShiftCollection.First();
            var firstOverTimeLayer = firstOvertimeShift.LayerCollection.First();

            Assert.IsTrue(assignmnet.OvertimeShiftCollection.First().LayerCollection.Contains(firstOverTimeLayer), "Verify contains the overtime");
						target.Remove(schedulePart, firstOverTimeLayer);
						Assert.AreEqual(0, schedulePart.AssignmentHighZOrder().OvertimeShiftCollection.Count, "The OvertimeLayer  should have been removed");
        }

    }
}
