using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class RemoveLayerFromScheduleServiceTest
    {
        private IRemoveLayerFromSchedule _target;
        private IScheduleDay _schedulePart;

        [SetUp]
        public void Setup()
        {
            _target = new RemoveLayerFromSchedule();
        }

        [Test]
        public void VerifyRemovesMainShiftLayer()
        {
            _schedulePart = new SchedulePartFactoryForDomain().AddMainShiftLayer().CreatePart();
            //Get the first layer in the mainshift..
#pragma warning disable 612,618
	        var mainShift = _schedulePart.AssignmentHighZOrder().ToMainShift();
#pragma warning restore 612,618
            ILayer<IActivity> firstLayer = mainShift.LayerCollection.First();
            Assert.IsTrue(mainShift.LayerCollection.Contains(firstLayer),"Verify contains the layer");
            _target.Remove(_schedulePart, firstLayer);
            Assert.AreEqual(0, _schedulePart.PersonAssignmentCollection().Count);
        }

        [Test]
        public void VerifyRemovesPersonalShiftLayer()
        {
            _schedulePart = new SchedulePartFactoryForDomain().AddMainShiftLayer().AddPersonalLayer().CreatePart();
           
            ILayer<IActivity> firstPersonalLayer =
                _schedulePart.AssignmentHighZOrder().PersonalShiftCollection.First().LayerCollection.First();

            Assert.IsTrue(_schedulePart.AssignmentHighZOrder().PersonalShiftCollection.First().LayerCollection.Contains(firstPersonalLayer), "Verify contains the layer");
            _target.Remove(_schedulePart, firstPersonalLayer);
            Assert.AreEqual(0, _schedulePart.AssignmentHighZOrder().PersonalShiftCollection.Count);
        }

        [Test]
        public void VerifyRemovesPersonAbsence()
        {
            _schedulePart = new SchedulePartFactoryForDomain().AddMainShiftLayer().AddAbsence().CreatePart();

            var firstPersonAbsence = _schedulePart.PersonAbsenceCollection().First();
            IAbsenceLayer firstAbsenceLayer = firstPersonAbsence.Layer;

            Assert.IsTrue(_schedulePart.PersonAbsenceCollection().Contains(firstPersonAbsence), "Verify contains the absence");
            _target.Remove(_schedulePart,firstAbsenceLayer);
            Assert.IsFalse(_schedulePart.PersonAbsenceCollection().Contains(firstPersonAbsence), "The PersonAbsence containing the layer should have been removed");
        }

        [Test]
        public void VerifyRemovesOvertimeLayer()
        {
            _schedulePart = new SchedulePartFactoryForDomain().AddMainShiftLayer().AddOvertime().CreatePart();

            IPersonAssignment assignmnet = _schedulePart.AssignmentHighZOrder();
            IOvertimeShift firstOvertimeShift = assignmnet.OvertimeShiftCollection.First();
            var firstOverTimeLayer = firstOvertimeShift.LayerCollection.First();

            Assert.IsTrue(assignmnet.OvertimeShiftCollection.First().LayerCollection.Contains(firstOverTimeLayer), "Verify contains the overtime");
            _target.Remove(_schedulePart,firstOverTimeLayer);
            Assert.AreEqual(0, _schedulePart.AssignmentHighZOrder().OvertimeShiftCollection.Count, "The OvertimeLayer  should have been removed");
        }

    }
}
