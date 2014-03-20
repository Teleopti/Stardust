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
        private IScheduleRange _currentSchedules;
        private IExtractDayOffFromGivenWeek _extractDayOffFromGivenWeek;
        private IScheduleDayWorkShiftTimeExtractor _scheduleDayWorkShiftTimeExtractor;
        private IScheduleDay _scheduleDay4;

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
            _scheduleDay4 = _mock.StrictMock<IScheduleDay>();
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
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Twice();
                Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Twice();
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

        [Test]
        public void DayoffWithCorrectSpanWhenSingleDayoffInWeek()
        {
            var scheduleDayList = new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2,_scheduleDay3  };
            var week = new DateOnlyPeriod(2014, 03, 17, 2014, 03, 19);
            DateTimePeriod? previousShiftPeriod = new DateTimePeriod(new DateTime(2014, 03, 17, 12, 0, 0, DateTimeKind.Utc), new DateTime(2014, 03, 17, 20, 0, 0, DateTimeKind.Utc));
            DateTimePeriod? nextShiftperiod = new DateTimePeriod(new DateTime(2014, 03, 19, 8, 0, 0, DateTimeKind.Utc), new DateTime(2014, 03, 19, 13, 0, 0, DateTimeKind.Utc));
            var dayOffDate = new DateOnly(2014, 03, 18);
            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDayCollection(week)).Return(scheduleDayList);
                Expect.Call(_extractDayOffFromGivenWeek.GetDaysOff(scheduleDayList)).Return(new List<DateOnly> { dayOffDate });

                Expect.Call(_currentSchedules.ScheduledDay(week.StartDate)).Return(_scheduleDay1);
                Expect.Call(_currentSchedules.ScheduledDay(week.EndDate)).Return(_scheduleDay3);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Twice() ;
                Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Twice();

                
                Expect.Call(_scheduleDayWorkShiftTimeExtractor.ShiftStartEndTime(_scheduleDay1))
                    .Return(previousShiftPeriod);
                Expect.Call(_scheduleDayWorkShiftTimeExtractor.ShiftStartEndTime(_scheduleDay3))
                    .Return(nextShiftperiod);
            }
            var result = _target.GetDayOffWithTimeSpanAmongAWeek(week, _currentSchedules);
            Assert.AreEqual(1, result.Count);
            
            Assert.AreEqual(TimeSpan.FromHours(36),result[dayOffDate]);
        }

        [Test]
        public void NoDayOffReturnedWhenMoreThenOneDayOffInAWeek()
        {
            var scheduleDayList = new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2, _scheduleDay3,_scheduleDay4  };
            var week = new DateOnlyPeriod(2014, 03, 16, 2014, 03, 19);
            var dayOffDate1 = new DateOnly(2014, 03, 17);
            var dayOffDate2 = new DateOnly(2014, 03, 18);
            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDayCollection(week)).Return(scheduleDayList);
                Expect.Call(_extractDayOffFromGivenWeek.GetDaysOff(scheduleDayList)).Return(new List<DateOnly> { dayOffDate1, dayOffDate2 });

                Expect.Call(_currentSchedules.ScheduledDay(week.StartDate)).Return(_scheduleDay1);
                Expect.Call(_currentSchedules.ScheduledDay(dayOffDate2)).Return(_scheduleDay3);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Twice();
                Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Twice();

                Expect.Call(_currentSchedules.ScheduledDay(dayOffDate1)).Return(_scheduleDay2);
                Expect.Call(_currentSchedules.ScheduledDay(week.EndDate)).Return(_scheduleDay4);
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Twice();
                Expect.Call(_scheduleDay4.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Twice();
            }
            var result = _target.GetDayOffWithTimeSpanAmongAWeek(week, _currentSchedules);
            Assert.AreEqual(0, result.Count);
        }

        
        [Test]
        public void ReturnCorrectDayOffWhenAbsenceFound()
        {
            var scheduleDayList = new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2, _scheduleDay3 };
            var week = new DateOnlyPeriod(2014, 03, 17, 2014, 03, 19);
            DateTimePeriod? previousShiftPeriod = new DateTimePeriod(new DateTime(2014, 03, 17, 12, 0, 0, DateTimeKind.Utc), new DateTime(2014, 03, 17, 17, 0, 0, DateTimeKind.Utc));
            DateTimePeriod? nextShiftperiod = new DateTimePeriod(new DateTime(2014, 03, 19, 5, 0, 0, DateTimeKind.Utc), new DateTime(2014, 03, 19, 13, 0, 0, DateTimeKind.Utc));
            var dayOffDate = new DateOnly(2014, 03, 18);
            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDayCollection(week)).Return(scheduleDayList);
                Expect.Call(_extractDayOffFromGivenWeek.GetDaysOff(scheduleDayList)).Return(new List<DateOnly> { dayOffDate });

                Expect.Call(_currentSchedules.ScheduledDay(week.StartDate)).Return(_scheduleDay1);
                Expect.Call(_currentSchedules.ScheduledDay(week.EndDate)).Return(_scheduleDay3);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.Absence).Repeat.Twice();
                Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Twice();
                
                Expect.Call(_scheduleDayWorkShiftTimeExtractor.ShiftStartEndTime(_scheduleDay1))
                    .Return(previousShiftPeriod);
                Expect.Call(_scheduleDayWorkShiftTimeExtractor.ShiftStartEndTime(_scheduleDay3))
                    .Return(nextShiftperiod);
            }
            var result = _target.GetDayOffWithTimeSpanAmongAWeek(week, _currentSchedules);
            Assert.AreEqual(1, result.Count);

            Assert.AreEqual(TimeSpan.FromHours(36), result[dayOffDate]);
        }

        [Test]
        public void ReturnDayOffWhenPreviousDayHasNoShift()
        {
            var scheduleDayList = new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2, _scheduleDay3 };
            var week = new DateOnlyPeriod(2014, 03, 17, 2014, 03, 19);
            var dayOffDate = new DateOnly(2014, 03, 18);
            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDayCollection(week)).Return(scheduleDayList);
                Expect.Call(_extractDayOffFromGivenWeek.GetDaysOff(scheduleDayList)).Return(new List<DateOnly> { dayOffDate });

                Expect.Call(_currentSchedules.ScheduledDay(week.StartDate)).Return(_scheduleDay1);
                Expect.Call(_currentSchedules.ScheduledDay(week.EndDate)).Return(_scheduleDay3);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.None );
                Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.MainShift);

            }
            var result = _target.GetDayOffWithTimeSpanAmongAWeek(week, _currentSchedules);
            Assert.AreEqual(1, result.Count);

            Assert.AreEqual(TimeSpan.Zero, result[dayOffDate]);
        }

        [Test]
        public void ReturnDayOffWhenNextDayHasNoShift()
        {
            var scheduleDayList = new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2, _scheduleDay3 };
            var week = new DateOnlyPeriod(2014, 03, 17, 2014, 03, 19);
            var dayOffDate = new DateOnly(2014, 03, 18);
            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDayCollection(week)).Return(scheduleDayList);
                Expect.Call(_extractDayOffFromGivenWeek.GetDaysOff(scheduleDayList)).Return(new List<DateOnly> { dayOffDate });

                Expect.Call(_currentSchedules.ScheduledDay(week.StartDate)).Return(_scheduleDay1);
                Expect.Call(_currentSchedules.ScheduledDay(week.EndDate)).Return(_scheduleDay3);
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift );
                Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.None );

            }
            var result = _target.GetDayOffWithTimeSpanAmongAWeek(week, _currentSchedules);
            Assert.AreEqual(1, result.Count);

            Assert.AreEqual(TimeSpan.Zero, result[dayOffDate]);
        }

    }


}
