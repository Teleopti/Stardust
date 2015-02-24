using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class VerifyWeeklyRestNotLockedAroundDayOffSpecificationTest
    {
        private MockRepository _mock;
        private VerifyWeeklyRestNotLockedAroundDayOffSpecification _target;
        private IScheduleDay _previousScheduleDay;
        private IScheduleDay _nextScheduleDay;
        private IScheduleRange _currentSchedules;
        private DateOnly _dayOff;
	    private IScheduleMatrixPro _scheduleMatrixPro;
			
			
			
		[SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new VerifyWeeklyRestNotLockedAroundDayOffSpecification();
            _previousScheduleDay = _mock.StrictMock<IScheduleDay>();
            _nextScheduleDay = _mock.StrictMock<IScheduleDay>();
            _currentSchedules = _mock.StrictMock<IScheduleRange>();
	        _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _dayOff =  new DateOnly(2014, 04, 13);
        }

		[Test]
		public void ShouldReturnTrueIfNorPreviousNorNextDayIsLocked()
		{

			var unlockedPreviousDayPro = _mock.StrictMock<IScheduleDayPro>();
			var unlockedPreviousDay = _mock.StrictMock<IScheduleDay>();
			var unlockedNextDayPro = _mock.StrictMock<IScheduleDayPro>();
			var unlockedNextDay = _mock.StrictMock<IScheduleDay>();


			var previodusDayPeriod = new DateTimePeriod(new DateTime(2014, 04, 12, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2014, 04, 12, 0, 0, 0, DateTimeKind.Utc).AddDays(1));
			var nextDayPeriod = new DateTimePeriod(new DateTime(2014, 04, 14, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2014, 04, 14, 0, 0, 0, DateTimeKind.Utc).AddDays(1));

			using (_mock.Record())
			{
				Expect.Call(_currentSchedules.ScheduledDay(_dayOff.AddDays(-1)))
					.Return(_previousScheduleDay);
				Expect.Call(_currentSchedules.ScheduledDay(_dayOff.AddDays(1)))
					.Return(_nextScheduleDay);
				Expect.Call(_previousScheduleDay.Period)
					.Return(previodusDayPeriod).Repeat.AtLeastOnce();
				Expect.Call(_nextScheduleDay.Period)
					.Return(nextDayPeriod).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.UnlockedDays)
					.Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { unlockedPreviousDayPro, unlockedNextDayPro }));
				Expect.Call(unlockedPreviousDayPro.DaySchedulePart())
					.Return(unlockedPreviousDay).Repeat.AtLeastOnce();
				Expect.Call(unlockedNextDayPro.DaySchedulePart())
					.Return(unlockedNextDay).Repeat.AtLeastOnce();

				Expect.Call(unlockedNextDay.Period)
					.Return(nextDayPeriod).Repeat.AtLeastOnce();
				Expect.Call(unlockedPreviousDay.Period)
					.Return(previodusDayPeriod).Repeat.AtLeastOnce();
			}
			using (_mock.Playback())
			{
				Assert.IsTrue(_target.IsSatisfy(_dayOff, _currentSchedules, _scheduleMatrixPro));
			}
		}

        [Test]
        public void ShouldReturnFalseIfPreviousDayIsLocked()
        {
	        var unlockedNextDayPro = _mock.StrictMock<IScheduleDayPro>();
			var unlockedNextDay = _mock.StrictMock<IScheduleDay>();


	        var previodusDayPeriod = new DateTimePeriod(new DateTime(2014, 04, 12, 0, 0, 0, DateTimeKind.Utc),
		        new DateTime(2014, 04, 12, 0, 0, 0, DateTimeKind.Utc).AddDays(1));
			var nextDayPeriod = new DateTimePeriod(new DateTime(2014, 04, 14, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2014, 04, 14, 0, 0, 0, DateTimeKind.Utc).AddDays(1));

            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDay(_dayOff.AddDays(-1)))
					.Return(_previousScheduleDay);
                Expect.Call(_currentSchedules.ScheduledDay(_dayOff.AddDays(1)))
					.Return(_nextScheduleDay);
	            Expect.Call(_previousScheduleDay.Period)
					.Return(previodusDayPeriod).Repeat.AtLeastOnce();
	            Expect.Call(_scheduleMatrixPro.UnlockedDays)
					.Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { unlockedNextDayPro }));
				Expect.Call(unlockedNextDayPro.DaySchedulePart())
					.Return(unlockedNextDay).Repeat.AtLeastOnce();

	            Expect.Call(unlockedNextDay.Period)
					.Return(nextDayPeriod).Repeat.AtLeastOnce();
            }
            using (_mock.Playback())
            {
				Assert.IsFalse(_target.IsSatisfy(_dayOff, _currentSchedules, _scheduleMatrixPro));
            }
        }

		[Test]
		public void ShouldReturnFalseIfNextDayIsLocked()
		{
			var unlockedPreviousDayPro = _mock.StrictMock<IScheduleDayPro>();
			var unlockedPreviousDay = _mock.StrictMock<IScheduleDay>();


			var previodusDayPeriod = new DateTimePeriod(new DateTime(2014, 04, 12, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2014, 04, 12, 0, 0, 0, DateTimeKind.Utc).AddDays(1));
			var nextDayPeriod = new DateTimePeriod(new DateTime(2014, 04, 14, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2014, 04, 14, 0, 0, 0, DateTimeKind.Utc).AddDays(1));

			using (_mock.Record())
			{
				Expect.Call(_currentSchedules.ScheduledDay(_dayOff.AddDays(-1)))
					.Return(_previousScheduleDay);
				Expect.Call(_currentSchedules.ScheduledDay(_dayOff.AddDays(1)))
					.Return(_nextScheduleDay);
				Expect.Call(_previousScheduleDay.Period)
					.Return(previodusDayPeriod).Repeat.AtLeastOnce();
				Expect.Call(_nextScheduleDay.Period)
					.Return(nextDayPeriod).Repeat.AtLeastOnce();
				Expect.Call(_scheduleMatrixPro.UnlockedDays)
					.Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { unlockedPreviousDayPro }));
				Expect.Call(unlockedPreviousDayPro.DaySchedulePart())
					.Return(unlockedPreviousDay).Repeat.AtLeastOnce();

				Expect.Call(unlockedPreviousDay.Period)
					.Return(previodusDayPeriod).Repeat.AtLeastOnce();
			}
			using (_mock.Playback())
			{
				Assert.IsFalse(_target.IsSatisfy(_dayOff, _currentSchedules, _scheduleMatrixPro));
			}
		}
    }
}
