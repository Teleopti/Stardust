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
		      var ass = schedulePart.PersonAssignment();

            var firstLayer = ass.MainActivities().Single();
            Assert.IsTrue(ass.MainActivities().Contains(firstLayer),"Verify contains the layer");
						target.Remove(schedulePart, firstLayer);
						Assert.AreEqual(0, schedulePart.PersonAssignment().MainActivities().Count());
        }

        [Test]
        public void VerifyRemovesPersonalShiftLayer()
        {
					var target = new RemoveLayerFromSchedule();
            var schedulePart = new SchedulePartFactoryForDomain().AddMainShiftLayer().AddPersonalLayer().CreatePart();
           
            var firstPersonalLayer = schedulePart.PersonAssignment().PersonalActivities().First();

						target.Remove(schedulePart, firstPersonalLayer);
						Assert.AreEqual(0, schedulePart.PersonAssignment().PersonalActivities().Count());
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

						var assignment = schedulePart.PersonAssignment();
            var firstOverTimeLayer = assignment.OvertimeActivities().First();

            Assert.IsTrue(assignment.OvertimeActivities().Contains(firstOverTimeLayer), "Verify contains the overtime");
						target.Remove(schedulePart, firstOverTimeLayer);
						Assert.AreEqual(0, schedulePart.PersonAssignment().OvertimeActivities().Count(), "The OvertimeLayer  should have been removed");
        }

    }
}
