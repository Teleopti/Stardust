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
        private IRemoveLayerFromScheduleService _target;
        private IScheduleDay _schedulePart;

        [SetUp]
        public void Setup()
        {
            _target = new RemoveLayerFromScheduleService();
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
            IScheduleDay result = _target.Remove(_schedulePart, firstLayer);
            Assert.AreEqual(0, result.PersonAssignmentCollection().Count);
            //Assert.IsFalse(result.AssignmentHighZOrder().MainShift.LayerCollection.Contains(firstLayer), "The layer should have been removed");
            
        }

        [Test]
        public void VerifyRemovesPersonalShiftLayer()
        {
            _schedulePart = new SchedulePartFactoryForDomain().AddMainShiftLayer().AddPersonalLayer().CreatePart();
           
            ILayer<IActivity> firstPersonalLayer =
                _schedulePart.AssignmentHighZOrder().PersonalShiftCollection.First().LayerCollection.First();

            Assert.IsTrue(_schedulePart.AssignmentHighZOrder().PersonalShiftCollection.First().LayerCollection.Contains(firstPersonalLayer), "Verify contains the layer");
            IScheduleDay result = _target.Remove(_schedulePart, firstPersonalLayer);
            //Assert.IsFalse(result.AssignmentHighZOrder().PersonalShiftCollection.First().LayerCollection.Contains(firstPersonalLayer), "The layer should have been removed");
            Assert.AreEqual(0, result.AssignmentHighZOrder().PersonalShiftCollection.Count);
        }

        [Test]
        public void VerifyRemovesPersonAbsence()
        {
            _schedulePart = new SchedulePartFactoryForDomain().AddMainShiftLayer().AddAbsence().CreatePart();

            var firstPersonAbsence = _schedulePart.PersonAbsenceCollection().First();
            IAbsenceLayer firstAbsenceLayer = firstPersonAbsence.Layer;

            Assert.IsTrue(_schedulePart.PersonAbsenceCollection().Contains(firstPersonAbsence), "Verify contains the absence");
            IScheduleDay result = _target.Remove(_schedulePart,firstAbsenceLayer);
            Assert.IsFalse(result.PersonAbsenceCollection().Contains(firstPersonAbsence), "The PersonAbsence containing the layer should have been removed");
        }

        [Test]
        public void VerifyRemovesOvertimeLayer()
        {
            _schedulePart = new SchedulePartFactoryForDomain().AddMainShiftLayer().AddOvertime().CreatePart();

            IPersonAssignment assignmnet = _schedulePart.AssignmentHighZOrder();
            IOvertimeShift firstOvertimeShift = assignmnet.OvertimeShiftCollection.First();
            var firstOverTimeLayer = firstOvertimeShift.LayerCollection.First();

            Assert.IsTrue(assignmnet.OvertimeShiftCollection.First().LayerCollection.Contains(firstOverTimeLayer), "Verify contains the overtime");
            IScheduleDay result = _target.Remove(_schedulePart,firstOverTimeLayer);
            //Assert.IsFalse(result.AssignmentHighZOrder().OvertimeShiftCollection.First().LayerCollection.Contains(firstOverTimeLayer), "The OvertimeLayer  should have been removed");
            Assert.AreEqual(0,result.AssignmentHighZOrder().OvertimeShiftCollection.Count, "The OvertimeLayer  should have been removed");
        }

    }
}
