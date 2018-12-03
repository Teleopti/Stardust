using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;


namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class VerifyWeeklyRestAroundDayOffSpecificationTest
    {
        private MockRepository _mock;
        private IVerifyWeeklyRestAroundDayOffSpecification _target;
        private IScheduleDay _previousScheduleDay;
        private IScheduleDay _nextScheduleDay;
        private IScheduleRange _currentSchedules;
        private List<DateOnly> _dayOffList;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new VerifyWeeklyRestAroundDayOffSpecification();
            _previousScheduleDay = _mock.StrictMock<IScheduleDay>();
            _nextScheduleDay = _mock.StrictMock<IScheduleDay>();
            _currentSchedules = _mock.StrictMock<IScheduleRange>();
            _dayOffList = new List<DateOnly>() { new DateOnly(2014, 04, 13), new DateOnly(2014, 04, 17) };
        }

        [Test]
        public void ShouldReturnTrueIfGivenAEmptyList()
        {
            var emptyDayOff = new List<DateOnly>();
            Assert.IsTrue(_target.IsSatisfy(emptyDayOff, _currentSchedules));
        }

        [Test]
        public void ShouldReturnTrueIfConsecutiveDayOffNotFound()
        {
            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDay(_dayOffList.First().AddDays(-1))).Return(_previousScheduleDay);
                Expect.Call(_currentSchedules.ScheduledDay(_dayOffList.First().AddDays(1))).Return(_nextScheduleDay);
                Expect.Call(_previousScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Times(2);
                Expect.Call(_nextScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Times(2);
                Expect.Call(_currentSchedules.ScheduledDay(_dayOffList.Last().AddDays(-1))).Return(_previousScheduleDay);
                Expect.Call(_currentSchedules.ScheduledDay(_dayOffList.Last().AddDays(1))).Return(_nextScheduleDay);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfy(_dayOffList, _currentSchedules));
            }
        }

        [Test]
        public void ShouldReturnFalseIfConsecutiveDayOffFound()
        {
            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDay(_dayOffList.First().AddDays(-1))).Return(_previousScheduleDay);
				Expect.Call(_currentSchedules.ScheduledDay(_dayOffList.First().AddDays(1))).Return(_nextScheduleDay);
	            Expect.Call(_currentSchedules.ScheduledDay(_dayOffList.First().AddDays(-2))).Return(_previousScheduleDay);
				Expect.Call(_currentSchedules.ScheduledDay(_dayOffList.First().AddDays(2))).Return(_nextScheduleDay);
				Expect.Call(_previousScheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(2);
                Expect.Call(_nextScheduleDay.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Times(2);     
			}
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfy(_dayOffList, _currentSchedules));
            }
        }

        [Test]
        public void ShouldReturnTrueIfMissingShiftNotFound()
        {
            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDay(_dayOffList.First().AddDays(-1))).Return(_previousScheduleDay);
                Expect.Call(_currentSchedules.ScheduledDay(_dayOffList.First().AddDays(1))).Return(_nextScheduleDay);
                Expect.Call(_previousScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
                Expect.Call(_nextScheduleDay.SignificantPart()).Return(SchedulePartView.Overtime).Repeat.AtLeastOnce();

                Expect.Call(_currentSchedules.ScheduledDay(_dayOffList.Last().AddDays(-1))).Return(_previousScheduleDay);
                Expect.Call(_currentSchedules.ScheduledDay(_dayOffList.Last().AddDays(1))).Return(_nextScheduleDay);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfy(_dayOffList, _currentSchedules));
            }
        }

        [Test]
        public void ShouldReturnFalseIfMissingShiftFound()
        {
            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDay(_dayOffList.First().AddDays(-1))).Return(_previousScheduleDay);
                Expect.Call(_currentSchedules.ScheduledDay(_dayOffList.First().AddDays(1))).Return(_nextScheduleDay);
	            Expect.Call(_currentSchedules.ScheduledDay(_dayOffList.First().AddDays(2))).Return(_nextScheduleDay);
				Expect.Call(_previousScheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
	            Expect.Call(_nextScheduleDay.SignificantPart()).Return(SchedulePartView.None).Repeat.Twice();
			}
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfy(_dayOffList, _currentSchedules));
            }
        }

    }
}
