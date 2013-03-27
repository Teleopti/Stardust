using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentStudentAvailabilityDayCreatorTest
	{
		private MockRepository _mock;
		private AgentStudentAvailabilityDayCreator _target;
		private IPerson _person;
		private DateOnly _dateOnly;
		private IScheduleDay _scheduleDay;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_target = new AgentStudentAvailabilityDayCreator();
			_person = new Person();
			_dateOnly = new DateOnly(2013, 1 ,1);
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
				var result = _target.Create(_scheduleDay, startTime, endTime, false);
				Assert.IsNotNull(result);
				var restriction = result.RestrictionCollection.FirstOrDefault();
				Assert.IsNotNull(restriction);
				Assert.AreEqual(startTime, restriction.StartTimeLimitation.StartTime);
				Assert.AreEqual(endTime, restriction.EndTimeLimitation.EndTime);
			}	
		}

		[Test]
		public void ShouldCreateWhenEndIsOnNextDay()
		{
			var startTime = TimeSpan.FromHours(8);
			var endTime = TimeSpan.FromHours(7);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
			}

			using (_mock.Playback())
			{
				var result = _target.Create(_scheduleDay, startTime, endTime, true);
				Assert.IsNotNull(result);
			}
		}

		[Test]
		public void ShouldNotCreateWhenStartTimeIsMoreOrEqualToEndAndEndSameDay()
		{
			var startTime = TimeSpan.FromHours(8);
			var endTime = TimeSpan.FromHours(8);

			var result = _target.Create(_scheduleDay, startTime, endTime, false);
			Assert.IsNull(result);
	
		}

		[Test]
		public void ShouldNotCreateWhenNoStartAndEndTime()
		{
			var result = _target.Create(_scheduleDay, null, null, false);
			Assert.IsNull(result);	
		}

		[Test]
		public void ShouldNotValidateWhenStartIsNull()
		{
			bool startTimeError;
			bool endTimeError;

			var result = _target.CanCreate(null, TimeSpan.FromHours(1), false, out startTimeError, out endTimeError);
			Assert.IsFalse(result);
			Assert.IsTrue(startTimeError);
		}

		[Test]
		public void ShouldNotValidateWhenEndIsNull()
		{
			bool startTimeError;
			bool endTimeError;

			var result = _target.CanCreate(TimeSpan.FromHours(1), null, false, out startTimeError, out endTimeError);
			Assert.IsFalse(result);
			Assert.IsTrue(endTimeError);
		}

		[Test]
		public void ShouldNotValidateWhenStartAndEndIsNull()
		{
			bool startTimeError;
			bool endTimeError;

			var result = _target.CanCreate(null, null, false, out startTimeError, out endTimeError);
			Assert.IsFalse(result);
			Assert.IsTrue(startTimeError);
			Assert.IsTrue(endTimeError);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionOnNullScheduleDay()
		{
			_target.Create(null, null, null, false);
		}
	}
}
