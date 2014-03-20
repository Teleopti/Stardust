using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class DayOffToTimeSpanExtractorTest
    {
        private IDayOffToTimeSpanExtractor _target;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IScheduleDay _scheduleDay3;
        private MockRepository _mock;
        private IPersonAssignment _personAssignment1;
        private IPersonAssignment _personAssignment2;
        private IDayOff _dayOff1;
        private IDayOff _dayOff2;
        private IScheduleRange _currentSchedules;
        private IExtractDayOffFromGivenWeek _extractDayOffFromGivenWeek;
        private IScheduleDayWorkShiftTimeExtractor _scheduleDayWorkShiftTimeExtractor;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _extractDayOffFromGivenWeek = _mock.StrictMock<IExtractDayOffFromGivenWeek>();
            _scheduleDayWorkShiftTimeExtractor = _mock.StrictMock<IScheduleDayWorkShiftTimeExtractor>();
            _target = new DayOffToTimeSpanExtractor(_extractDayOffFromGivenWeek,_scheduleDayWorkShiftTimeExtractor);
            
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay3 = _mock.StrictMock<IScheduleDay>();
            _personAssignment1 = _mock.StrictMock<IPersonAssignment>();
            _personAssignment2 = _mock.StrictMock<IPersonAssignment>();
            _dayOff1 = _mock.StrictMock<IDayOff>();
            _dayOff2 = _mock.StrictMock<IDayOff>();
            _currentSchedules = _mock.StrictMock<IScheduleRange>();
        }

        [Test]
        public void ShouldReturnEmptyDictionaryIfNoDayOffFound()
        {
            var scheduleDayList = new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2,_scheduleDay3  };
            var week = new DateOnlyPeriod(2014, 03, 17,2014,03,19);
            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDayCollection(week)).Return(scheduleDayList);
                Expect.Call(_extractDayOffFromGivenWeek.GetDaysOff(scheduleDayList)).Return(new List<DateOnly>(){new DateOnly(2014,03,18 )});

                Expect.Call(_currentSchedules.ScheduledDay(week.StartDate)).Return(_scheduleDay1);
                Expect.Call(_currentSchedules.ScheduledDay(week.EndDate)).Return(_scheduleDay3 );
                Expect.Call(_scheduleDayWorkShiftTimeExtractor.ShiftStartEndTime(_scheduleDay1)).Return(null);
                Expect.Call(_scheduleDayWorkShiftTimeExtractor.ShiftStartEndTime(_scheduleDay3)).Return(null);
            }
            var result = _target.GetDayOffWithTimeSpanAmongAWeek(week,_currentSchedules);
            Assert.AreEqual( 0, result.Count);
        }

        [Test]
        public void ShouldSingleDayOffInPeriod()
        {
            var scheduleDayList = new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2 };
            var week = new DateOnlyPeriod(2014, 03, 18, 2014, 03, 19);
            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDayCollection(week)).Return(scheduleDayList);
                Expect.Call(_extractDayOffFromGivenWeek.GetDaysOff(scheduleDayList)).Return(new List<DateOnly>());
            }
            var result = _target.GetDayOffWithTimeSpanAmongAWeek(week, _currentSchedules);
            Assert.AreEqual(0, result.Count);
        }

        //[Test]
        //public void ShouldReturnFirstDayIfAllDayOffSpanAreEqual()
        //{
        //    var scheduleDayList = new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2 };
        //    var targetTimeSpan = new TimeSpan(0, 10, 0, 0);
        //    using (_mock.Record())
        //    {
        //        Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
        //        Expect.Call(_personAssignment1.Date).Return(new DateOnly(2014, 03, 18));

        //        mockForOneDay(_scheduleDay1, _personAssignment1, _dayOff1, targetTimeSpan, new DateOnly(2014, 03, 18));
        //        mockForOneDay(_scheduleDay2, _personAssignment2, _dayOff2, targetTimeSpan, new DateOnly(2014, 03, 19));
        //    }
        //    var result = _target.GetDayOffWithLongestSpan(scheduleDayList);
        //    Assert.AreEqual(new DateOnly(2014, 03, 18), result);
        //}

        //[Test]
        //public void ShouldReturnLogestSpanAtLastPosition()
        //{
        //    var scheduleDayList = new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2 };
        //    var targetTimeSpan = new TimeSpan(0, 10, 0, 0);
        //    using (_mock.Record())
        //    {
        //        Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
        //        Expect.Call(_personAssignment1.Date).Return(new DateOnly(2014, 03, 18));

        //        mockForOneDay(_scheduleDay1, _personAssignment1, _dayOff1, targetTimeSpan, new DateOnly(2014, 03, 18));
        //        mockForOneDay(_scheduleDay2, _personAssignment2, _dayOff2, targetTimeSpan.Add(TimeSpan.FromHours(2)), new DateOnly(2014, 03, 19));
        //    }
        //    var result = _target.GetDayOffWithLongestSpan(scheduleDayList);
        //    Assert.AreEqual(new DateOnly(2014, 03, 19), result);
        //}

        //[Test]
        //public void ShouldReturnLogestSpanAtFirstPosition()
        //{
        //    var scheduleDayList = new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2 };
        //    var targetTimeSpan = new TimeSpan(0, 10, 0, 0);
        //    using (_mock.Record())
        //    {
        //        Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
        //        Expect.Call(_personAssignment1.Date).Return(new DateOnly(2014, 03, 18));

        //        mockForOneDay(_scheduleDay1, _personAssignment1, _dayOff1, targetTimeSpan.Add(TimeSpan.FromHours(2)), new DateOnly(2014, 03, 18));
        //        mockForOneDay(_scheduleDay2, _personAssignment2, _dayOff2, targetTimeSpan, new DateOnly(2014, 03, 19));
        //    }
        //    var result = _target.GetDayOffWithLongestSpan(scheduleDayList);
        //    Assert.AreEqual(new DateOnly(2014, 03, 18), result);
        //}

        //[Test]
        //public void ShouldReturnNothingIfDayOffDoesNottExists()
        //{
        //    Assert.AreEqual(new DateOnly(), _target.GetDayOffWithLongestSpan(new List<IScheduleDay>()));
        //}

        //private void mockForOneDay(IScheduleDay scheduleDay, IPersonAssignment personAssignment, IDayOff dayOff, TimeSpan targetTimeSpan, DateOnly date)
        //{
        //    Expect.Call(scheduleDay.PersonAssignment()).Return(personAssignment);
        //    Expect.Call(personAssignment.DayOff()).Return(dayOff);
        //    Expect.Call(dayOff.TargetLength).Return(targetTimeSpan);
        //    Expect.Call(personAssignment.Date).Return(date);
        //}

    }


}
