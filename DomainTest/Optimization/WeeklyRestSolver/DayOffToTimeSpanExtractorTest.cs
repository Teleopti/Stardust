using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;


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
        private IVerifyWeeklyRestAroundDayOffSpecification _verifyWeeklyRestAroundDayOffSpecification;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _extractDayOffFromGivenWeek = _mock.StrictMock<IExtractDayOffFromGivenWeek>();
            _scheduleDayWorkShiftTimeExtractor = _mock.StrictMock<IScheduleDayWorkShiftTimeExtractor>();
            _verifyWeeklyRestAroundDayOffSpecification = _mock.StrictMock<IVerifyWeeklyRestAroundDayOffSpecification>();

            _target = new DayOffToTimeSpanExtractor(_extractDayOffFromGivenWeek,_scheduleDayWorkShiftTimeExtractor,
				_verifyWeeklyRestAroundDayOffSpecification);
            
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay3 = _mock.StrictMock<IScheduleDay>();
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
                Expect.Call(_extractDayOffFromGivenWeek.GetDaysOff(scheduleDayList)).Return(new List<DateOnly>());
            }
			var result = _target.GetDayOffWithTimeSpanAmongAWeek(week, _currentSchedules);
            Assert.AreEqual( 0, result.Count);
        }

        [Test]
        public void ReturnEmptyDictionaryIfVerifyWeeklyRestAroundDayOffNotSatisfied()
        {
            var scheduleDayList = new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2, _scheduleDay3 };
            var week = new DateOnlyPeriod(2014, 03, 17, 2014, 03, 19);
            var dayOffDate = new DateOnly(2014, 03, 18);
            var dayOffList = new List<DateOnly> { dayOffDate };
            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDayCollection(week)).Return(scheduleDayList);
                Expect.Call(_extractDayOffFromGivenWeek.GetDaysOff(scheduleDayList)).Return(dayOffList);
                Expect.Call(_verifyWeeklyRestAroundDayOffSpecification.IsSatisfy(dayOffList, _currentSchedules))
                    .Return(false);
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
            var dayOffList = new List<DateOnly> {dayOffDate};
            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDayCollection(week)).Return(scheduleDayList);
                Expect.Call(_extractDayOffFromGivenWeek.GetDaysOff(scheduleDayList)).Return(dayOffList);
				Expect.Call(_verifyWeeklyRestAroundDayOffSpecification.IsSatisfy(dayOffList, _currentSchedules))
					.Return(true);
                Expect.Call(_currentSchedules.ScheduledDay(week.StartDate)).Return(_scheduleDay1);
                Expect.Call(_currentSchedules.ScheduledDay(week.EndDate)).Return(_scheduleDay3);

                
                Expect.Call(_scheduleDayWorkShiftTimeExtractor.ShiftStartEndTime(_scheduleDay1))
                    .Return(previousShiftPeriod);
                Expect.Call(_scheduleDayWorkShiftTimeExtractor.ShiftStartEndTime(_scheduleDay3))
                    .Return(nextShiftperiod);
            }
			var result = _target.GetDayOffWithTimeSpanAmongAWeek(week, _currentSchedules);
            Assert.AreEqual(1, result.Count);
            
            Assert.AreEqual(TimeSpan.FromHours(36),result[dayOffDate]);
        }
    }
}
