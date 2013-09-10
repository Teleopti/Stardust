using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
    [TestFixture]
    public class ScheduleDayToShiftCategoryMapperTest
    {
        private MockRepository _mocks;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IPersonAssignment _personAssignment1;
        private IPersonAssignment _personAssignment2;
        private ShiftCategory _shiftCategory1;
        private ShiftCategory _shiftCategory2;
        private IPerson _person1;
        private IPerson _person2;


        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
            _personAssignment1 = _mocks.StrictMock<IPersonAssignment>();
            _personAssignment2 = _mocks.StrictMock<IPersonAssignment>();
            _shiftCategory1 = new ShiftCategory("Test1");
            _shiftCategory2 = new ShiftCategory("Test2");
            _person1 = PersonFactory.CreatePerson("asad");
            _person2 = PersonFactory.CreatePerson("ali");

        }

        [Test]
        public void CheckIfCorrectListIsMapped()
        {
            setUpData();

            var returnList =
                ScheduleDayToShiftCategoryMapper.MapScheduleDay(new List<IScheduleDay> {_scheduleDay1, _scheduleDay2});
            
            Assert.AreEqual(returnList.Count,2 );
        }

        [Test]
        public void CheckIfShiftCategoryIsMapped()
        {
            setUpData();

            var returnList =
                ScheduleDayToShiftCategoryMapper.MapScheduleDay(new List<IScheduleDay> { _scheduleDay1, _scheduleDay2 });

            Assert.AreEqual( returnList[0].ShiftCategory , _shiftCategory1);
            Assert.AreEqual(returnList[1].ShiftCategory, _shiftCategory2);
        }
        
        [Test]
        public void CheckIfDateIsMapped()
        {
            setUpData();

            var returnList =
                ScheduleDayToShiftCategoryMapper.MapScheduleDay(new List<IScheduleDay> { _scheduleDay1, _scheduleDay2 });

            Assert.AreEqual( returnList[0].DateOnlyValue  , DateOnly.Today );
            Assert.AreEqual(returnList[1].DateOnlyValue, DateOnly.Today);
        }
        [Test]
        public void CheckIfPersonIsMapped()
        {
            setUpData();

            var returnList =
                ScheduleDayToShiftCategoryMapper.MapScheduleDay(new List<IScheduleDay> { _scheduleDay1, _scheduleDay2 });

            Assert.AreEqual( returnList[0].PersonValue   , _person1  );
            Assert.AreEqual(returnList[1].PersonValue , _person2 );
        }

        [Test]
        public void CheckIfEmptyListIsReturned()
        {
            using (_mocks.Record())
            {
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
                Expect.Call(_personAssignment1.ShiftCategory).Return(null);
                Expect.Call(_scheduleDay1.DateOnlyAsPeriod)
                      .Return(new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
            }
            var returnList =
                ScheduleDayToShiftCategoryMapper.MapScheduleDay(new List<IScheduleDay> { _scheduleDay1 });

            Assert.AreEqual(returnList.Count , 1);
        }

        private void  setUpData()
        {
            using (_mocks.Record())
            {
                expectCalls(_scheduleDay1, _personAssignment1, _shiftCategory1, _person1);
                expectCalls(_scheduleDay2, _personAssignment2, _shiftCategory2, _person2);
            }
        }


        private static void expectCalls(IScheduleDay scheduleDay1, IPersonAssignment personAssignment1,
                                        IShiftCategory shiftCategory1, IPerson person)
        {
            Expect.Call(scheduleDay1.PersonAssignment()).Return(personAssignment1);
            Expect.Call(personAssignment1.ShiftCategory).Return(shiftCategory1).Repeat.Twice() ;
            Expect.Call(scheduleDay1.DateOnlyAsPeriod)
                  .Return(new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc)).Repeat.Twice() ;
            Expect.Call(scheduleDay1.Person).Return(person);
        }
    }
}
