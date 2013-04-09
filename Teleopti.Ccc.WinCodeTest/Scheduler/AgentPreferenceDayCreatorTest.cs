using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentPreferenceDayCreatorTest
	{
		private AgentPreferenceDayCreator _preferenceDayCreator;
		private MockRepository _mock;
		private IPerson _person;
		private DateOnly _dateOnly;
		private IScheduleDay _scheduleDay;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
		private IShiftCategory _shiftCategory;
		private IAbsence _absence;
		private IDayOffTemplate _dayOffTemplate;
		private IActivity _activity;

	
		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_preferenceDayCreator = new AgentPreferenceDayCreator();
			_person = new Person();
			_dateOnly = new DateOnly(2013, 1, 1);
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_dateOnlyAsDateTimePeriod = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
			_shiftCategory = new ShiftCategory("shiftCatgory");
			_absence = new Absence();
			_dayOffTemplate = new DayOffTemplate(new Description("dayOffTemplate"));
			_activity = new Activity("activity");
		}

		[Test]
		public void ShouldCreate()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);	
			}

			using (_mock.Playback())
			{
				var result = _preferenceDayCreator.Create(_scheduleDay, null, null, null, null, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);
				Assert.IsNotNull(result);
				var restriction = result.Restriction;
				Assert.IsNotNull(restriction);
				Assert.AreEqual(minStart, restriction.StartTimeLimitation.StartTime);
				Assert.AreEqual(maxStart,restriction.StartTimeLimitation.EndTime);
				Assert.AreEqual(minEnd, restriction.EndTimeLimitation.StartTime);
				Assert.AreEqual(maxEnd, restriction.EndTimeLimitation.EndTime);
				Assert.AreEqual(minLength, restriction.WorkTimeLimitation.StartTime);
				Assert.AreEqual(maxLength, restriction.WorkTimeLimitation.EndTime);
			}	
		}

		[Test]
		public void ShouldCreateShiftCategory()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
			}

			using (_mock.Playback())
			{
				var result = _preferenceDayCreator.Create(_scheduleDay, _shiftCategory, null, null, null, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);
				Assert.IsNotNull(result);
				var restriction = result.Restriction;
				Assert.IsNotNull(restriction);
				Assert.AreEqual(_shiftCategory, restriction.ShiftCategory);
				Assert.AreEqual(minStart, restriction.StartTimeLimitation.StartTime);
				Assert.AreEqual(maxStart, restriction.StartTimeLimitation.EndTime);
				Assert.AreEqual(minEnd, restriction.EndTimeLimitation.StartTime);
				Assert.AreEqual(maxEnd, restriction.EndTimeLimitation.EndTime);
				Assert.AreEqual(minLength, restriction.WorkTimeLimitation.StartTime);
				Assert.AreEqual(maxLength, restriction.WorkTimeLimitation.EndTime);
			}
		}

		[Test]
		public void ShouldCreateActivity()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
			}

			using (_mock.Playback())
			{
				var result = _preferenceDayCreator.Create(_scheduleDay, null, null, null, _activity, null, null, null, null, null, null, false, minStart, maxStart, minEnd, maxEnd, minLength, maxLength);
				Assert.IsNotNull(result);
				var restriction = result.Restriction;
				Assert.IsNotNull(restriction);
				var activityRestriction = restriction.ActivityRestrictionCollection[0];
				Assert.AreEqual(_activity, activityRestriction.Activity);
				Assert.AreEqual(minStart, activityRestriction.StartTimeLimitation.StartTime);
				Assert.AreEqual(maxStart, activityRestriction.StartTimeLimitation.EndTime);
				Assert.AreEqual(minEnd, activityRestriction.EndTimeLimitation.StartTime);
				Assert.AreEqual(maxEnd, activityRestriction.EndTimeLimitation.EndTime);
				Assert.AreEqual(minLength, activityRestriction.WorkTimeLimitation.StartTime);
				Assert.AreEqual(maxLength, activityRestriction.WorkTimeLimitation.EndTime);
			}
		}

		[Test]
		public void ShouldCreateShiftCategoryAndActivity()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
			}

			using (_mock.Playback())
			{
				var result = _preferenceDayCreator.Create(_scheduleDay, _shiftCategory, null, null, _activity, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false, minStart, maxStart, minEnd, maxEnd, minLength, maxLength);
				Assert.IsNotNull(result);
				var restriction = result.Restriction;
				Assert.IsNotNull(restriction);
				Assert.AreEqual(_shiftCategory, restriction.ShiftCategory);
				Assert.AreEqual(minStart, restriction.StartTimeLimitation.StartTime);
				Assert.AreEqual(maxStart, restriction.StartTimeLimitation.EndTime);
				Assert.AreEqual(minEnd, restriction.EndTimeLimitation.StartTime);
				Assert.AreEqual(maxEnd, restriction.EndTimeLimitation.EndTime);
				Assert.AreEqual(minLength, restriction.WorkTimeLimitation.StartTime);
				Assert.AreEqual(maxLength, restriction.WorkTimeLimitation.EndTime);

				var activityRestriction = restriction.ActivityRestrictionCollection[0];
				Assert.AreEqual(_activity, activityRestriction.Activity);
				Assert.AreEqual(minStart, activityRestriction.StartTimeLimitation.StartTime);
				Assert.AreEqual(maxStart, activityRestriction.StartTimeLimitation.EndTime);
				Assert.AreEqual(minEnd, activityRestriction.EndTimeLimitation.StartTime);
				Assert.AreEqual(maxEnd, activityRestriction.EndTimeLimitation.EndTime);
				Assert.AreEqual(minLength, activityRestriction.WorkTimeLimitation.StartTime);
				Assert.AreEqual(maxLength, activityRestriction.WorkTimeLimitation.EndTime);
			}	
		}

		[Test]
		public void ShouldCreateAbsence()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
			}

			using (_mock.Playback())
			{
				var result = _preferenceDayCreator.Create(_scheduleDay, null, _absence, null, null, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);
				Assert.IsNotNull(result);
				var restriction = result.Restriction;
				Assert.IsNotNull(restriction);
				Assert.AreEqual(_absence, restriction.Absence);
				Assert.AreEqual(null, restriction.StartTimeLimitation.StartTime);
				Assert.AreEqual(null, restriction.StartTimeLimitation.EndTime);
				Assert.AreEqual(null, restriction.EndTimeLimitation.StartTime);
				Assert.AreEqual(null, restriction.EndTimeLimitation.EndTime);
				Assert.AreEqual(null, restriction.WorkTimeLimitation.StartTime);
				Assert.AreEqual(null, restriction.WorkTimeLimitation.EndTime);
			}
		}

		[Test]
		public void ShouldCreateDayOffTemplate()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
			}

			using (_mock.Playback())
			{
				var result = _preferenceDayCreator.Create(_scheduleDay, null, null, _dayOffTemplate, null, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);
				Assert.IsNotNull(result);
				var restriction = result.Restriction;
				Assert.IsNotNull(restriction);
				Assert.AreEqual(_dayOffTemplate, restriction.DayOffTemplate);
				Assert.AreEqual(null, restriction.StartTimeLimitation.StartTime);
				Assert.AreEqual(null, restriction.StartTimeLimitation.EndTime);
				Assert.AreEqual(null, restriction.EndTimeLimitation.StartTime);
				Assert.AreEqual(null, restriction.EndTimeLimitation.EndTime);
				Assert.AreEqual(null, restriction.WorkTimeLimitation.StartTime);
				Assert.AreEqual(null, restriction.WorkTimeLimitation.EndTime);
			}
		}

		[Test]
		public void ShouldReturnNullWhenCannotCreate()
		{
			TimeSpan? minStart = TimeSpan.FromHours(2);
			TimeSpan? maxStart = TimeSpan.FromHours(1);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.Create(_scheduleDay, null, null, null, null, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);
			Assert.IsNull(result);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionOnEmptyDay()
		{
			_preferenceDayCreator.Create(null, null, null, null, null, null, null, null, null, null, null, false, null, null, null, null, null, null);	
		}

		[Test]
		public void ShouldBeAbleToCreateWithValidTimes()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);
		}

		[Test]
		public void ShouldHandleEmptyStartTimeMin()
		{
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, null, maxStart, minEnd, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);
		}

		[Test]
		public void ShouldHandleEmptyStartTimeMax()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, null, minEnd, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);
		}

		[Test]
		public void ShouldHandleEmptyStartTime()
		{
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, null, null, minEnd, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);	
		}

		[Test]
		public void ShouldDetectStartTimeMinGreaterThanStartTimeMax()
		{
			TimeSpan? minStart = TimeSpan.FromHours(2);
			TimeSpan? maxStart = TimeSpan.FromHours(1);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);

			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);	
		}

		[Test]
		public void ShouldHandleEmptyEndTimeMin()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, null, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);	
		}

		[Test]
		public void ShouldHandleEmptyEndTimeMax()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, minEnd, null, minLength, maxLength, false, null, null, null, null, null, null);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);	
		}

		[Test]
		public void ShouldHandleEmptyEndTime()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, null, null, minLength, maxLength, false, null, null, null, null, null, null);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);	
		}

		[Test]
		public void ShouldDetectEndTimeMinGreaterThanEndTimeMax()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(4);
			TimeSpan? maxEnd = TimeSpan.FromHours(3);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);

			Assert.IsFalse(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsTrue(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);
		}

		[Test]
		public void ShouldHandleEndTimeNextDay()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(4);
			TimeSpan? maxEnd = TimeSpan.FromHours(3);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, true, null, null, null, null, null, null);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);	
		}

		[Test]
		public void ShouldHandleEmptyMinLength()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, minEnd, maxEnd, null, maxLength, false, null, null, null, null, null, null);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);	
		}

		[Test]
		public void ShouldHandleEmptyMaxLength()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, minEnd, maxEnd, minLength, null, false, null, null, null, null, null, null);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);
		}

		[Test]
		public void ShouldHandleEmptyLength()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, minEnd, maxEnd, null, null, false, null, null, null, null, null, null);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);
		}

		[Test]
		public void ShouldDetectMinLengthGreaterThanMaxLength()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(3);
			TimeSpan? maxLength = TimeSpan.FromHours(2);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);

			Assert.IsFalse(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsTrue(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);	
		}

		[Test]
		public void ShouldDetectStartTimeMinGreaterThanEndTimeMin()
		{
			TimeSpan? minStart = TimeSpan.FromHours(3);
			TimeSpan? maxStart = TimeSpan.FromHours(4);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);

			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);		
		}

		[Test]
		public void ShouldDetectStartTimeMinGreaterThanEndTimeMax()
		{
			TimeSpan? minStart = TimeSpan.FromHours(4);
			TimeSpan? maxStart = TimeSpan.FromHours(5);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);

			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);
		}

		[Test]
		public void ShouldDetectStartTimeMaxGreaterThanEndTimeMin()
		{
			TimeSpan? minStart = TimeSpan.FromHours(2);
			TimeSpan? maxStart = TimeSpan.FromHours(4);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, minEnd, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);

			Assert.IsFalse(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsTrue(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);
		}

		[Test]
		public void ShouldDetectStartTimeMaxGreaterThanEndTimeMax()
		{
			TimeSpan? minStart = TimeSpan.FromHours(2);
			TimeSpan? maxStart = TimeSpan.FromHours(4);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(2);
			TimeSpan? maxLength = TimeSpan.FromHours(3);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, null, maxEnd, minLength, maxLength, false, null, null, null, null, null, null);

			Assert.IsFalse(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsTrue(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);
		}

		[Test]
		public void ShouldDetectCannotFulfillMinLength()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(3);
			TimeSpan? maxEnd = TimeSpan.FromHours(4);
			TimeSpan? minLength = TimeSpan.FromHours(5);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, minEnd, maxEnd, minLength, null, false, null, null, null, null, null, null);

			Assert.IsFalse(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsTrue(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);		
		}

		[Test]
		public void ShouldDetectCannotFulfillMaxLength()
		{
			TimeSpan? minStart = TimeSpan.FromHours(1);
			TimeSpan? maxStart = TimeSpan.FromHours(2);
			TimeSpan? minEnd = TimeSpan.FromHours(6);
			TimeSpan? maxLength = TimeSpan.FromHours(1);

			var result = _preferenceDayCreator.CanCreate(null, null, null, null, minStart, maxStart, minEnd, null, null, maxLength, false, null, null, null, null, null, null);

			Assert.IsFalse(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsTrue(result.LengthMaxError);	
		}

		[Test]
		public void ShouldNotBeAbleToCreateWhenDayOffTemplateAndShiftCategory()
		{
			var result = _preferenceDayCreator.CanCreate(_shiftCategory, null, _dayOffTemplate, null, null, null, null, null, null, null, false, null, null, null, null, null, null);
			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.ConflictingTypeError);
		}

		[Test]
		public void ShouldNotBeAbleToCreateWhenDayOffTemplateAndAbsence()
		{
			var result = _preferenceDayCreator.CanCreate(null, _absence, _dayOffTemplate, null, null, null, null, null, null, null, false, null, null, null, null, null, null);
			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.ConflictingTypeError);
		}

		[Test]
		public void ShouldNotBeAbleToCreateWhenDayOffTemplateAndActivity()
		{
			var result = _preferenceDayCreator.CanCreate(null, null, _dayOffTemplate, _activity, null, null, null, null, null, null, false, null, null, null, null, null, null);
			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.ConflictingTypeError);
		}

		[Test]
		public void ShouldNotBeAbleToCreateWhenShiftCategoryAndAbsence()
		{
			var result = _preferenceDayCreator.CanCreate(_shiftCategory, _absence, null, null, null, null, null, null, null, null, false, null, null, null, null, null, null);
			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.ConflictingTypeError);
		}

		[Test]
		public void ShouldNotBeAbleToCreateWhenShiftCategoryAndDayOffTemplate()
		{
			var result = _preferenceDayCreator.CanCreate(_shiftCategory, null, _dayOffTemplate, null, null, null, null, null, null, null, false, null, null, null, null, null, null);
			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.ConflictingTypeError);
		}

		[Test]
		public void ShouldNotBeAbleToCreateWhenActivityAndAbsence()
		{
			var result = _preferenceDayCreator.CanCreate(null, _absence, null, _activity, null, null, null, null, null, null, false, null, null, null, null, null, null);
			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.ConflictingTypeError);
		}
	}
}
