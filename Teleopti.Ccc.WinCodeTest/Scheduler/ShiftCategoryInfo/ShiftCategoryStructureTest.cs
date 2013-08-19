using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryInfo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryInfo
{
    [TestFixture]
    public class ShiftCategoryStructureTest
    {
        private ShiftCategoryStructure _target;
        private IScheduleDay _scheduleDay;
        private MockRepository _mocks;
        private IPersonAssignment _personAssignment;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _personAssignment = _mocks.StrictMock<IPersonAssignment>();
        }

        [Test]
        public void VerifyValueBeingSetWithScheduleDay()
        {
            var shiftCategory = new Domain.Scheduling.ShiftCategory("test");
            var person = PersonFactory.CreatePerson("person1");
            using (_mocks.Record())
            {
                Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
                Expect.Call(_personAssignment.ShiftCategory).Return(shiftCategory);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod)
                      .Return(new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));

                Expect.Call(_scheduleDay.Person).Return(person);
            }
            _target = new ShiftCategoryStructure(_scheduleDay);
            Assert.AreEqual(_target.ShiftCategoryValue.Description, shiftCategory.Description);
            Assert.AreEqual(_target.DateOnlyValue, DateOnly.Today);
            Assert.AreEqual(_target.PersonValue, person);

        }

        [Test]
        public void VerifyValueBeingSetWithDirectValues()
        {
            var shiftCategory = new Domain.Scheduling.ShiftCategory("test1");
            var person = PersonFactory.CreatePerson("person2");
            using (_mocks.Record())
            {

            }
            _target = new ShiftCategoryStructure(shiftCategory,DateOnly.Today,person);
            Assert.AreEqual(_target.ShiftCategoryValue.Description, shiftCategory.Description);
            Assert.AreEqual(_target.DateOnlyValue, DateOnly.Today);
            Assert.AreEqual(_target.PersonValue, person);

        }
    }
}
