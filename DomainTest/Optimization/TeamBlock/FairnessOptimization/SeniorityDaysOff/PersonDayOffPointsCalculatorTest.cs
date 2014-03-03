using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    [TestFixture]
    public class PersonDayOffPointsCalculatorTest
    {
        private IPersonDayOffPointsCalculator _target;
        private IWeekDayPoints _weekDayPoints;
        private IScheduleRange _scheduleRange;
        private MockRepository _mock;
        private IScheduleDay _scheduleDay;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _scheduleRange = _mock.StrictMock<IScheduleRange>();
            _weekDayPoints = new WeekDayPoints();
            _target = new PersonDayOffPointsCalculator(_weekDayPoints);    
        }

        [Test]
        public void TestSingleDayOff()
        {
            var requestedPeriod = new DateOnlyPeriod(2014,02,15,2014,02,17);
            using (_mock.Record())
            {
                expectCall(_scheduleDay,new DateOnly(2014,02,15),SchedulePartView.MainShift  );
                expectCall(_scheduleDay,new DateOnly(2014,02,16),SchedulePartView.DayOff   );
                expectCall(_scheduleDay,new DateOnly(2014,02,17),SchedulePartView.MainShift    );
            }
            using (_mock.Playback())
            {
                var result = _target.CalculateDaysOffSeniorityValue(_scheduleRange, requestedPeriod);
                Assert.AreEqual(result,7);
            }
        }

        [Test]
        public void TestMultipleDayOff()
        {
            var requestedPeriod = new DateOnlyPeriod(2014, 02, 15, 2014, 02, 17);
            using (_mock.Record())
            {
                expectCall(_scheduleDay, new DateOnly(2014, 02, 15), SchedulePartView.DayOff);
                expectCall(_scheduleDay, new DateOnly(2014, 02, 16), SchedulePartView.DayOff);
                expectCall(_scheduleDay, new DateOnly(2014, 02, 17), SchedulePartView.DayOff);
            }
            using (_mock.Playback())
            {
                var result = _target.CalculateDaysOffSeniorityValue(_scheduleRange, requestedPeriod);
                Assert.AreEqual(result, 14);
            }
        }

        private void expectCall(IScheduleDay  scheduleDay, DateOnly dateOnly, SchedulePartView schedulePartView)
        {
            Expect.Call(_scheduleRange.ScheduledDay(dateOnly)).Return(scheduleDay);
            Expect.Call(scheduleDay.SignificantPart()).Return(schedulePartView);
        }

    }
}
