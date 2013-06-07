using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Restriction
{
    [TestFixture]
    public class OvertimeAvailabilityCreatorTest
    {
        private MockRepository _mock;
        private IOvertimeAvailabilityCreator _target;
        private IPerson _person;
        private DateOnly _dateOnly;
        private IScheduleDay _scheduleDay;
        private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new OvertimeAvailabilityCreator();
            _person = new Person();
            _dateOnly = new DateOnly(2013, 1, 1);
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _dateOnlyAsDateTimePeriod = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
        }

        [Test]
        public void ShouldCreate()
        {
            var startTime = TimeSpan.FromHours(8);
            var endTime = TimeSpan.FromHours(10);

            using (_mock.Record())
            {
                Expect.Call(_scheduleDay.Person).Return(_person);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
            }

            using (_mock.Playback())
            {
                var result = _target.Create(_scheduleDay, startTime, endTime);
                Assert.IsNotNull(result);
                //var restriction = result.RestrictionCollection.FirstOrDefault();
                //Assert.IsNotNull(restriction);
                //Assert.AreEqual(startTime, restriction.StartTimeLimitation.StartTime);
                //Assert.AreEqual(endTime, restriction.EndTimeLimitation.EndTime);
            }
        }

        [Test]
        public void ShouldCreateWhenEndIsOnNextDay()
        {
            var startTime = TimeSpan.FromHours(8);
            var endTime = TimeSpan.FromHours(25);

            using (_mock.Record())
            {
                Expect.Call(_scheduleDay.Person).Return(_person);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
            }

            using (_mock.Playback())
            {
                var result = _target.Create(_scheduleDay, startTime, endTime);
                Assert.IsNotNull(result);
            }
        }

        [Test]
        public void ShouldNotCreateWhenStartTimeIsMoreOrEqualToEndAndEndSameDay()
        {
            var startTime = TimeSpan.FromHours(8);
            var endTime = TimeSpan.FromHours(8);

            var result = _target.Create(_scheduleDay, startTime, endTime);
            Assert.IsNull(result);

        }

        [Test]
        public void ShouldNotCreateWhenNoStartAndEndTime()
        {
            var result = _target.Create(_scheduleDay, null,null);
            Assert.IsNull(result);
        }

        [Test]
        public void ShouldNotValidateWhenStartIsNull()
        {
            bool startTimeError;
            bool endTimeError;

            var result = _target.CanCreate(null, TimeSpan.FromHours(1), out startTimeError, out endTimeError);
            Assert.IsFalse(result);
            Assert.IsTrue(startTimeError);
        }

        [Test]
        public void ShouldNotValidateWhenEndIsNull()
        {
            bool startTimeError;
            bool endTimeError;

            var result = _target.CanCreate(TimeSpan.FromHours(1), null, out startTimeError, out endTimeError);
            Assert.IsFalse(result);
            Assert.IsTrue(endTimeError);
        }

        [Test]
        public void ShouldNotValidateWhenStartAndEndIsNull()
        {
            bool startTimeError;
            bool endTimeError;

            var result = _target.CanCreate(null, null, out startTimeError, out endTimeError);
            Assert.IsFalse(result);
            Assert.IsTrue(startTimeError);
            Assert.IsTrue(endTimeError);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionOnNullScheduleDay()
        {
            _target.Create(null, null, null);
        }
    }

    public class  OvertimeAvailabilityCreator : IOvertimeAvailabilityCreator
    {
        public IOvertimeAvailability Create(IScheduleDay scheduleDay, TimeSpan? startTime, TimeSpan? endTime)
        {
            if (scheduleDay == null) throw new ArgumentNullException("scheduleDay");

            bool startTimeError;
            bool endTimeError;
            if (!CanCreate(startTime, endTime, out startTimeError, out endTimeError)) return null;


            var overtimeAvailability = new OvertimeAvailability(scheduleDay.Person,
                                                                  scheduleDay.DateOnlyAsPeriod.DateOnly,startTime ,endTime);

            return overtimeAvailability;
        }

        public bool CanCreate(TimeSpan? startTime, TimeSpan? endTime, out bool startTimeError, out bool endTimeError)
        {
            if (startTime == null && endTime == null)
			{
				startTimeError = true;
				endTimeError = true;
				return false;	
			}
				
			if (startTime != null && endTime != null)
			{
				if (startTime.Value >= endTime.Value)
				{
					startTimeError = true;
					endTimeError = false;
					return false;
				}
			}

			if (startTime == null)
			{
				startTimeError = true;
				endTimeError = false;
				return false;
			}

			if (endTime == null)
			{
				startTimeError = false;
				endTimeError = true;
				return false;
			}

			startTimeError = false;
			endTimeError = false;
			return true;
		
        }
    }

    public interface IOvertimeAvailabilityCreator 
    {
        IOvertimeAvailability Create(IScheduleDay scheduleDay, TimeSpan? startTime, TimeSpan? endTime);
        bool CanCreate(TimeSpan? startTime, TimeSpan? endTime, out bool startTimeError, out bool endTimeError);
    }
}
