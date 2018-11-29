using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;


namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class ExtractDayOffFromGivenWeekTest
    {
        private MockRepository _mock;
        private IExtractDayOffFromGivenWeek _target;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IScheduleDay _scheduleDay3;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new ExtractDayOffFromGivenWeek();
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay3 = _mock.StrictMock<IScheduleDay>();
        }

        [Test]
        public void ReturnNothingIfNoDayOffFound()
        {
            var scheduleDayList = new List<IScheduleDay> { _scheduleDay1, _scheduleDay2, _scheduleDay3 };
            using (_mock.Record())
            {
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.MainShift);
            }
            using (_mock.Playback())
            {
                var result = _target.GetDaysOff(scheduleDayList);
                Assert.AreEqual(0, result.Count());
            }
        }

        [Test]
        public void ReturnOneDayOffThatIsOfTypeDayOff()
        {
            var scheduleDayList = new List<IScheduleDay> { _scheduleDay1, _scheduleDay2, _scheduleDay3 };
            using (_mock.Record())
            {
                Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
                Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2014, 03, 19),TimeZoneInfo.Utc));
                Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.MainShift);
                Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.MainShift);
            }
            using (_mock.Playback())
            {
                var result = _target.GetDaysOff(scheduleDayList);
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual(new DateOnly(2014, 03, 19), result[0]);
            }
        }
    }
}
