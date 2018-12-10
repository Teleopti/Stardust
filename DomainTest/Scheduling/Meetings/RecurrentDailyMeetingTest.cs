using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.Scheduling.Meetings
{
    [TestFixture]
    public class RecurrentDailyMeetingTest
    {
        private IRecurrentDailyMeeting _target;

        [SetUp]
        public void Setup()
        {
            _target = new RecurrentDailyMeeting();
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
            _target.IncrementCount = 2; //Repeat every second day

            Assert.AreEqual(2, _target.IncrementCount);
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
            _target.SetId(Guid.NewGuid());
            IRecurrentDailyMeeting clone = (IRecurrentDailyMeeting)_target.Clone();

            Assert.AreEqual(_target.IncrementCount, clone.IncrementCount);
            Assert.IsTrue(clone.Id.HasValue);

            clone = (IRecurrentDailyMeeting)_target.EntityClone();

            Assert.AreEqual(_target.IncrementCount, clone.IncrementCount);
            Assert.IsTrue(clone.Id.HasValue);

            clone = (IRecurrentDailyMeeting) _target.NoneEntityClone();

            Assert.AreEqual(_target.IncrementCount, clone.IncrementCount);
            Assert.IsFalse(clone.Id.HasValue);
        }

        [Test]
        public void VerifyGetDatesWork()
        {
            DateOnly startDate = new DateOnly(2009,10,12);
            DateOnly endDate = startDate.AddDays(5);

            _target.IncrementCount = 3;
            IList<DateOnly> meetingDays = _target.GetMeetingDays(startDate, endDate);
            Assert.AreEqual(2,meetingDays.Count);
            Assert.AreEqual(startDate, meetingDays[0]);
            Assert.AreEqual(startDate.AddDays(3), meetingDays[1]);

            _target.IncrementCount = 2;
            meetingDays = _target.GetMeetingDays(startDate, endDate);
            Assert.AreEqual(3, meetingDays.Count);
            Assert.AreEqual(startDate, meetingDays[0]);
            Assert.AreEqual(startDate.AddDays(2), meetingDays[1]);
            Assert.AreEqual(startDate.AddDays(4), meetingDays[2]);

            _target.IncrementCount = 1;
            meetingDays = _target.GetMeetingDays(startDate, endDate);
            Assert.AreEqual(6, meetingDays.Count);
            Assert.AreEqual(startDate, meetingDays[0]);
            Assert.AreEqual(startDate.AddDays(1), meetingDays[1]);
            Assert.AreEqual(startDate.AddDays(5), meetingDays[5]);

            _target.IncrementCount = 1;
            meetingDays = _target.GetMeetingDays(startDate, startDate);
            Assert.AreEqual(1, meetingDays.Count);
            Assert.AreEqual(startDate, meetingDays[0]);
        }
    }
}
