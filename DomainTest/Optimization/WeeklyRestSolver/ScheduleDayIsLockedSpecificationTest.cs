using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;


namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class ScheduleDayIsLockedSpecificationTest
    {
        private MockRepository _mock;
        private ScheduleDayIsLockedSpecification _target;
        private IScheduleDay _scheduleDay;
        private IScheduleRange _currentSchedules;
        private DateOnly _day;
	    private DateTimePeriod _dayDateTimePeriod;
	    private IScheduleMatrixPro _scheduleMatrixPro;

	    [SetUp]
	    public void Setup()
	    {
		    _mock = new MockRepository();
		    _target = new ScheduleDayIsLockedSpecification();
		    _scheduleDay = _mock.StrictMock<IScheduleDay>();
		    _currentSchedules = _mock.StrictMock<IScheduleRange>();
		    _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();

		    _day = new DateOnly(2014, 04, 13);
		    _dayDateTimePeriod = new DateTimePeriod(new DateTime(2014, 04, 13, 0, 0, 0, DateTimeKind.Utc),
			    new DateTime(2014, 04, 14, 0, 0, 0, DateTimeKind.Utc));
	    }

	    [Test]
		public void ShouldReturnFalseIfDayIsUnlocked()
		{
			var unlockedDayPro = _mock.StrictMock<IScheduleDayPro>();
			var unlockedDay = _mock.StrictMock<IScheduleDay>();
		    var unlockedDayPeriod = _dayDateTimePeriod;

			using (_mock.Record())
			{
				Expect.Call(_currentSchedules.ScheduledDay(_day))
					.Return(_scheduleDay);
				Expect.Call(_scheduleDay.Period)
					.Return(_dayDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.UnlockedDays)
					.Return(new HashSet<IScheduleDayPro> { unlockedDayPro });
				Expect.Call(unlockedDayPro.DaySchedulePart())
					.Return(unlockedDay).Repeat.AtLeastOnce();

				Expect.Call(unlockedDay.Period)
					.Return(unlockedDayPeriod).Repeat.AtLeastOnce();
			}
			using (_mock.Playback())
			{
				Assert.IsFalse(_target.IsSatisfy(_currentSchedules.ScheduledDay(_day), _scheduleMatrixPro));
			}
		}

		[Test]
		public void ShouldReturnTrueIfDayIsLocked()
		{
			var unlockedDayPro = _mock.StrictMock<IScheduleDayPro>();
			var unlockedDay = _mock.StrictMock<IScheduleDay>();
			var unlockedDayPeriod = _dayDateTimePeriod.MovePeriod(TimeSpan.FromDays(2));

			using (_mock.Record())
			{
				Expect.Call(_currentSchedules.ScheduledDay(_day))
					.Return(_scheduleDay);
				Expect.Call(_scheduleDay.Period)
					.Return(_dayDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.UnlockedDays)
					.Return(new HashSet<IScheduleDayPro> { unlockedDayPro });
				Expect.Call(unlockedDayPro.DaySchedulePart())
					.Return(unlockedDay).Repeat.AtLeastOnce();
				Expect.Call(unlockedDay.Period)
					.Return(unlockedDayPeriod).Repeat.AtLeastOnce();
			}
			using (_mock.Playback())
			{
				Assert.IsTrue(_target.IsSatisfy(_currentSchedules.ScheduledDay(_day), _scheduleMatrixPro));
			}
		}
    }
}
