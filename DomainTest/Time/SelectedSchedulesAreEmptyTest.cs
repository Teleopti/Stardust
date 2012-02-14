using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Time
{
    [TestFixture]
    public class SelectedSchedulesAreEmptyTest
    {
        private SelectedSchedulesAreEmpty _target;
        private IList<IScheduleDay> _scheduleDays;
        private IScheduleDay _scheduleDay;

        [SetUp]
        public void Setup()
        {
            _scheduleDays = new List<IScheduleDay>();
            _scheduleDay = new SchedulePartFactoryForDomain().CreatePart();
            _scheduleDays.Add(_scheduleDay);
            _target = new SelectedSchedulesAreEmpty();
        }

        [Test]
        public void VerifyIsTrueIfThereIsOnlyOneScheduleWithNoAssignment()
        {
            Assert.IsTrue(_target.IsSatisfiedBy(_scheduleDays));
        }

        [Test]
        public void VerifyIsFalseIfMultipleSchedulePartsWithNoAssignments()
        {
            _scheduleDays.Add(new SchedulePartFactoryForDomain().CreatePart());
            Assert.IsFalse(_target.IsSatisfiedBy(_scheduleDays));
        }

        [Test]
        public void VerifyIsFalseIfScheduleDayHasMainShift()
        {
            _scheduleDays.Clear();
            _scheduleDays.Add(new SchedulePartFactoryForDomain().CreatePartWithMainShift());
            Assert.IsFalse(_target.IsSatisfiedBy(_scheduleDays));
        }

        [Test]
        public void VerifyIsFalseIfListIsNull()
        {
            Assert.IsTrue(_target.IsSatisfiedBy(null));
        }

        [Test]
        public void VerifyIsFalseIfListIsEmpty()
        {
            Assert.IsTrue(_target.IsSatisfiedBy(new List<IScheduleDay>()));
        }
    }

}
