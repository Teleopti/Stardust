using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.WeeklyRest;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WeeklyRest
{
    [TestFixture]
    public class DayOffWithLongestSpanTest
    {
        private IDayOffWithLongestSpan _target;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private MockRepository _mock;
        private IPersonAssignment _personAssignment1;
        private IPersonAssignment _personAssignment2;
        private IDayOff _dayOff1;
        private IDayOff _dayOff2;

        [SetUp]
        public void Setup()
        {
            _target = new DayOffWithLongestSpan();
            _mock = new MockRepository();
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mock.StrictMock<IScheduleDay>();
            _personAssignment1 = _mock.StrictMock<IPersonAssignment>();
            _personAssignment2 = _mock.StrictMock<IPersonAssignment>();
            _dayOff1 = _mock.StrictMock<IDayOff>();
            _dayOff2 = _mock.StrictMock<IDayOff>();
        }

        [Test]
        public void ShouldReturnFirstDayIfAllDayOffSpanAreEqual()
        {
            var scheduleDayList = new List<IScheduleDay>(){_scheduleDay1,_scheduleDay2};
            var targetTimeSpan = new TimeSpan(0,10,0,0);
            using (_mock.Record())
            {
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
                Expect.Call(_personAssignment1.Date).Return(new DateOnly(2014, 03, 18));

                mockForOneDay(_scheduleDay1, _personAssignment1, _dayOff1, targetTimeSpan, new DateOnly(2014, 03, 18));
                mockForOneDay(_scheduleDay2, _personAssignment2, _dayOff2, targetTimeSpan, new DateOnly(2014, 03, 19));
            }
            var result = _target.GetDayOffWithLongestSpan(scheduleDayList);
            Assert.AreEqual(new DateOnly(2014,03,18),result );
        }

        [Test]
        public void ShouldReturnLogestSpanAtLastPosition()
        {
            var scheduleDayList = new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2 };
            var targetTimeSpan = new TimeSpan(0, 10, 0, 0);
            using (_mock.Record())
            {
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
                Expect.Call(_personAssignment1.Date).Return(new DateOnly(2014, 03, 18));

                mockForOneDay(_scheduleDay1, _personAssignment1, _dayOff1, targetTimeSpan, new DateOnly(2014, 03, 18));
                mockForOneDay(_scheduleDay2, _personAssignment2, _dayOff2, targetTimeSpan.Add(TimeSpan.FromHours(2)), new DateOnly(2014, 03, 19));
            }
            var result = _target.GetDayOffWithLongestSpan(scheduleDayList);
            Assert.AreEqual(new DateOnly(2014, 03, 19), result);
        }

        [Test]
        public void ShouldReturnLogestSpanAtFirstPosition()
        {
            var scheduleDayList = new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2 };
            var targetTimeSpan = new TimeSpan(0, 10, 0, 0);
            using (_mock.Record())
            {
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
                Expect.Call(_personAssignment1.Date).Return(new DateOnly(2014, 03, 18));

                mockForOneDay(_scheduleDay1, _personAssignment1, _dayOff1, targetTimeSpan.Add(TimeSpan.FromHours(2)), new DateOnly(2014, 03, 18));
                mockForOneDay(_scheduleDay2, _personAssignment2, _dayOff2, targetTimeSpan, new DateOnly(2014, 03, 19));
            }
            var result = _target.GetDayOffWithLongestSpan(scheduleDayList);
            Assert.AreEqual(new DateOnly(2014, 03, 18), result);
        }

        [Test]
        public void ShouldReturnNothingIfDayOffDoesNottExists()
        {
            Assert.AreEqual( new DateOnly(),_target.GetDayOffWithLongestSpan(new List<IScheduleDay>()));
        }

        private void mockForOneDay(IScheduleDay scheduleDay, IPersonAssignment personAssignment, IDayOff dayOff, TimeSpan targetTimeSpan, DateOnly date)
        {
            Expect.Call(scheduleDay.PersonAssignment()).Return(personAssignment);
            Expect.Call(personAssignment.DayOff()).Return(dayOff);
            Expect.Call(dayOff.TargetLength).Return(targetTimeSpan);
            Expect.Call(personAssignment.Date).Return(date);
        }

    }

    
}
