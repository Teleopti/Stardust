using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.Scheduling.Meetings
{
    [TestFixture]
    public class RecurrentWeeklyMeetingTest
    {
        private IRecurrentWeeklyMeeting _target;

        [SetUp]
        public void Setup()
        {
            _target = new RecurrentWeeklyMeeting();
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(1, _target.IncrementCount);
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType(), true));
        }

        [Test]
        public void VerifyProperties()
        {
            _target.IncrementCount = 2; //Repeat every second week
            _target[DayOfWeek.Friday] = true;
            _target[DayOfWeek.Wednesday] = true;

            Assert.AreEqual(2,_target.WeekDays.Count());
            Assert.AreEqual(DayOfWeek.Friday,_target.WeekDays.ElementAt(0));
			Assert.AreEqual(DayOfWeek.Wednesday, _target.WeekDays.ElementAt(1));
            Assert.AreEqual(2, _target.IncrementCount);

            _target[DayOfWeek.Wednesday] = false;
            Assert.AreEqual(1, _target.WeekDays.Count());
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
            _target[DayOfWeek.Friday] = true;
            _target.SetId(Guid.NewGuid());
            var clone = (IRecurrentWeeklyMeeting)_target.Clone();

            Assert.AreEqual(_target.IncrementCount, clone.IncrementCount);
			Assert.AreEqual(_target.WeekDays.ElementAt(0), clone.WeekDays.ElementAt(0));
            Assert.IsTrue(clone.Id.HasValue);

            clone = (IRecurrentWeeklyMeeting)_target.EntityClone();

            Assert.AreEqual(_target.IncrementCount, clone.IncrementCount);
			Assert.AreEqual(_target.WeekDays.ElementAt(0), clone.WeekDays.ElementAt(0));
            Assert.IsTrue(clone.Id.HasValue);

            clone = (IRecurrentWeeklyMeeting)_target.NoneEntityClone();

            Assert.AreEqual(_target.IncrementCount, clone.IncrementCount);
			Assert.AreEqual(_target.WeekDays.ElementAt(0), clone.WeekDays.ElementAt(0));
            Assert.IsFalse(clone.Id.HasValue);
        }

        [Test]
        public void VerifyGetDatesWork()
        {
            var startDate = new DateOnly(2009,10,12);
            var endDate = startDate.AddDays(13);

            _target.IncrementCount = 1;
            _target[DayOfWeek.Monday] = true;
            _target[DayOfWeek.Wednesday] = true;
            IList<DateOnly> meetingDays = _target.GetMeetingDays(startDate, endDate);
            Assert.AreEqual(4,meetingDays.Count);
            Assert.AreEqual(startDate, meetingDays[0]);
            Assert.AreEqual(startDate.AddDays(2), meetingDays[1]);
            Assert.AreEqual(startDate.AddDays(7), meetingDays[2]);
            Assert.AreEqual(startDate.AddDays(9), meetingDays[3]);

            _target.IncrementCount = 2;
            meetingDays = _target.GetMeetingDays(startDate, endDate);
            Assert.AreEqual(2, meetingDays.Count);
            Assert.AreEqual(startDate, meetingDays[0]);
            Assert.AreEqual(startDate.AddDays(2), meetingDays[1]);
        }
    }
}
