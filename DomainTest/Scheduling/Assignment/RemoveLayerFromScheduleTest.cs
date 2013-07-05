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
		      var ass = schedulePart.AssignmentHighZOrder();

            var firstLayer = ass.MainLayers().Single();
            Assert.IsTrue(ass.MainLayers().Contains(firstLayer),"Verify contains the layer");
						target.Remove(schedulePart, firstLayer);
						Assert.AreEqual(0, schedulePart.PersonAssignmentCollection().Count);
        }

        [Test]
        public void VerifyRemovesPersonalShiftLayer()
        {
					var target = new RemoveLayerFromSchedule();
            var schedulePart = new SchedulePartFactoryForDomain().AddMainShiftLayer().AddPersonalLayer().CreatePart();
           
            var firstPersonalLayer = schedulePart.AssignmentHighZOrder().PersonalLayers().First();

						target.Remove(schedulePart, firstPersonalLayer);
						Assert.AreEqual(0, schedulePart.AssignmentHighZOrder().PersonalLayers().Count());
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

						var assignment = schedulePart.AssignmentHighZOrder();
            var firstOverTimeLayer = assignment.OvertimeLayers().First();

            Assert.IsTrue(assignment.OvertimeLayers().Contains(firstOverTimeLayer), "Verify contains the overtime");
						target.Remove(schedulePart, firstOverTimeLayer);
						Assert.AreEqual(0, schedulePart.AssignmentHighZOrder().OvertimeLayers().Count(), "The OvertimeLayer  should have been removed");
        }

    }
}
