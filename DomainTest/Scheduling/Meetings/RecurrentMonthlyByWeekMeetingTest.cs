using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.Scheduling.Meetings
{
    [TestFixture]
    public class RecurrentMonthlyByWeekMeetingTest
    {
        private IRecurrentMonthlyByWeekMeeting _target;

        [SetUp]
        public void Setup()
        {
            _target = new RecurrentMonthlyByWeekMeeting();
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(1, _target.IncrementCount);
            Assert.AreEqual(WeekNumber.First,_target.WeekOfMonth);
            Assert.AreEqual(DayOfWeek.Sunday,_target.DayOfWeek);
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType(), true));
        }

        [Test]
        public void VerifyProperties()
        {
            _target.IncrementCount = 2; //Repeat every second month
            _target.DayOfWeek = DayOfWeek.Friday;
            _target.WeekOfMonth = WeekNumber.Last;

            Assert.AreEqual(2, _target.IncrementCount);
            Assert.AreEqual(DayOfWeek.Friday,_target.DayOfWeek);
            Assert.AreEqual(WeekNumber.Last,_target.WeekOfMonth);
        }

        [Test]
        public void VerifyIncrementalDayCountCannotBeLessThanOne()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.IncrementCount = 0);
        }

        [Test]
        public void VerifyClone()
        {
            _target.IncrementCount = 3;
            _target.DayOfWeek = DayOfWeek.Friday;
            _target.WeekOfMonth = WeekNumber.Last;
            _target.SetId(Guid.NewGuid());
            IRecurrentMonthlyByWeekMeeting clone = (IRecurrentMonthlyByWeekMeeting)_target.Clone();

            Assert.AreEqual(_target.IncrementCount, clone.IncrementCount);
            Assert.AreEqual(_target.DayOfWeek, clone.DayOfWeek);
            Assert.AreEqual(_target.WeekOfMonth,clone.WeekOfMonth);
            Assert.IsTrue(clone.Id.HasValue);

            clone = (IRecurrentMonthlyByWeekMeeting)_target.EntityClone();

            Assert.AreEqual(_target.IncrementCount, clone.IncrementCount);
            Assert.AreEqual(_target.DayOfWeek, clone.DayOfWeek);
            Assert.AreEqual(_target.WeekOfMonth, clone.WeekOfMonth);
            Assert.IsTrue(clone.Id.HasValue);

            clone = (IRecurrentMonthlyByWeekMeeting)_target.NoneEntityClone();

            Assert.AreEqual(_target.IncrementCount, clone.IncrementCount);
            Assert.AreEqual(_target.DayOfWeek, clone.DayOfWeek);
            Assert.AreEqual(_target.WeekOfMonth, clone.WeekOfMonth);
            Assert.IsFalse(clone.Id.HasValue);
        }

        [Test]
        public void VerifyGetDatesWork()
        {
            DateOnly startDate = new DateOnly(2009,10,12);
            DateOnly endDate = startDate.AddDays(65);

            _target.DayOfWeek = DayOfWeek.Friday;
            _target.WeekOfMonth = WeekNumber.First;
            _target.IncrementCount = 1;
            IList<DateOnly> meetingDays = _target.GetMeetingDays(startDate, endDate);
            Assert.AreEqual(2,meetingDays.Count);
            Assert.AreEqual(new DateOnly(2009,11,6), meetingDays[0]);
            Assert.AreEqual(new DateOnly(2009, 12, 4), meetingDays[1]);

            _target.IncrementCount = 2;
            meetingDays = _target.GetMeetingDays(startDate, endDate);
            Assert.AreEqual(new DateOnly(2009, 12, 4), meetingDays[0]);
            Assert.AreEqual(1, meetingDays.Count);

            _target.IncrementCount = 1;
            _target.DayOfWeek = DayOfWeek.Wednesday;
            _target.WeekOfMonth = WeekNumber.Last;
            meetingDays = _target.GetMeetingDays(startDate, endDate);
            Assert.AreEqual(2, meetingDays.Count);
            Assert.AreEqual(new DateOnly(2009, 10, 28), meetingDays[0]);
            Assert.AreEqual(new DateOnly(2009, 11, 25), meetingDays[1]);

            _target.IncrementCount = 1;
            _target.DayOfWeek = DayOfWeek.Wednesday;
            _target.WeekOfMonth = WeekNumber.Second;
            meetingDays = _target.GetMeetingDays(startDate, endDate);
            Assert.AreEqual(3, meetingDays.Count);
            Assert.AreEqual(new DateOnly(2009, 10, 14), meetingDays[0]);
            Assert.AreEqual(new DateOnly(2009, 11, 11), meetingDays[1]);
            Assert.AreEqual(new DateOnly(2009, 12, 9), meetingDays[2]);

            _target.IncrementCount = 1;
            _target.DayOfWeek = DayOfWeek.Wednesday;
            _target.WeekOfMonth = WeekNumber.Third;
            meetingDays = _target.GetMeetingDays(startDate, endDate);
            Assert.AreEqual(3, meetingDays.Count);
            Assert.AreEqual(new DateOnly(2009, 10, 21), meetingDays[0]);
            Assert.AreEqual(new DateOnly(2009, 11, 18), meetingDays[1]);
            Assert.AreEqual(new DateOnly(2009, 12, 16), meetingDays[2]);

            _target.IncrementCount = 1;
            _target.DayOfWeek = DayOfWeek.Saturday;
            _target.WeekOfMonth = WeekNumber.Fourth;
            meetingDays = _target.GetMeetingDays(startDate, endDate);
            Assert.AreEqual(2, meetingDays.Count);
            Assert.AreEqual(new DateOnly(2009, 10, 24), meetingDays[0]);
            Assert.AreEqual(new DateOnly(2009, 11, 28), meetingDays[1]);
        }

		[Test]
	    public void ShouldConsiderLastDateOfMonthWhenGettingLastOccurenceOfDay()
	    {
			var startDate = new DateOnly(2017, 1, 1);
			var endDate = new DateOnly(2017, 1, 31);
			_target.DayOfWeek = DayOfWeek.Tuesday;
			_target.IncrementCount = 1;
			_target.DayOfWeek = DayOfWeek.Tuesday;
			_target.WeekOfMonth = WeekNumber.Last;
			var meetingDays = _target.GetMeetingDays(startDate, endDate);
			Assert.AreEqual(1, meetingDays.Count);
			Assert.AreEqual(endDate, meetingDays[0]);
		}

		[Test]
	    public void ShouldConsiderFirstDateInSelectedPeriodWhenGettingLastCoccurenceOfDay()
	    {
			var startDate = new DateOnly(2017, 1, 31);
			var endDate = new DateOnly(2017, 1, 31);
			_target.DayOfWeek = DayOfWeek.Tuesday;
			_target.IncrementCount = 1;
			_target.DayOfWeek = DayOfWeek.Tuesday;
			_target.WeekOfMonth = WeekNumber.Last;
			var meetingDays = _target.GetMeetingDays(startDate, endDate);
			Assert.AreEqual(1, meetingDays.Count);
			Assert.AreEqual(endDate, meetingDays[0]);
		}
    }
}
