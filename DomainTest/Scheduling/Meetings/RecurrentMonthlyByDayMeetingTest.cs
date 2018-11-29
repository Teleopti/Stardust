using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.Scheduling.Meetings
{
    [TestFixture]
    public class RecurrentMonthlyByDayMeetingTest
    {
        private IRecurrentMonthlyByDayMeeting _target;

        [SetUp]
        public void Setup()
        {
            _target = new RecurrentMonthlyByDayMeeting();
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(1, _target.IncrementCount);
            Assert.AreEqual(1,_target.DayInMonth);
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
            _target.DayInMonth = 4;

            Assert.AreEqual(2, _target.IncrementCount);
        }

        [Test]
        public void VerifyIncrementalDayCountCannotBeLessThanOne()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.IncrementCount = 0);
        }

        [Test]
        public void VerifyDayInMonthCannotBeLessThanOne()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.DayInMonth = 0);
        }

        [Test]
        public void VerifyDayInMonthCannotBeHigherThanNumberOfDaysInLongestMonth()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.DayInMonth = 32);
        }

        [Test]
        public void VerifyClone()
        {
            _target.IncrementCount = 3;
            _target.DayInMonth = 5;
            _target.SetId(Guid.NewGuid());
            IRecurrentMonthlyByDayMeeting clone = (IRecurrentMonthlyByDayMeeting)_target.Clone();

            Assert.AreEqual(_target.IncrementCount, clone.IncrementCount);
            Assert.AreEqual(_target.DayInMonth, clone.DayInMonth);
            Assert.IsTrue(clone.Id.HasValue);

            clone = (IRecurrentMonthlyByDayMeeting)_target.EntityClone();

            Assert.AreEqual(_target.IncrementCount, clone.IncrementCount);
            Assert.AreEqual(_target.DayInMonth, clone.DayInMonth);
            Assert.IsTrue(clone.Id.HasValue);

            clone = (IRecurrentMonthlyByDayMeeting)_target.NoneEntityClone();

            Assert.AreEqual(_target.IncrementCount, clone.IncrementCount);
            Assert.AreEqual(_target.DayInMonth, clone.DayInMonth);
            Assert.IsFalse(clone.Id.HasValue);
        }

        [Test]
        public void VerifyGetDatesWork()
        {
            DateOnly startDate = new DateOnly(2009,10,12);
            DateOnly endDate = startDate.AddDays(50);

            _target.DayInMonth = 6;
            _target.IncrementCount = 1;
            IList<DateOnly> meetingDays = _target.GetMeetingDays(startDate, endDate);
            Assert.AreEqual(1,meetingDays.Count);
            Assert.AreEqual(startDate.AddDays(25), meetingDays[0]);

            _target.IncrementCount = 2;
            meetingDays = _target.GetMeetingDays(startDate, endDate);
            Assert.AreEqual(0, meetingDays.Count);

            _target.DayInMonth = 31;
            _target.IncrementCount = 1;
            meetingDays = _target.GetMeetingDays(startDate, endDate);
            Assert.AreEqual(2, meetingDays.Count);
            Assert.AreEqual(startDate.AddDays(19), meetingDays[0]);
            Assert.AreEqual(startDate.AddDays(49), meetingDays[1]);
        }
    }
}
