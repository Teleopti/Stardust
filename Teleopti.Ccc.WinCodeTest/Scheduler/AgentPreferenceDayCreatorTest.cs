using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;


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
		public void ShouldCreateTimes()
		{
			var data = new AgentPreferenceData
				{
					MinStart = TimeSpan.FromHours(1),
					MaxStart = TimeSpan.FromHours(2),
					MinEnd = TimeSpan.FromHours(3),
					MaxEnd = TimeSpan.FromHours(4),
					MinLength = TimeSpan.FromHours(2),
					MaxLength = TimeSpan.FromHours(3)
				};

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);	
			}

			using (_mock.Playback())
			{
				var result = _preferenceDayCreator.Create(_scheduleDay, data);
				Assert.IsNotNull(result);
				var restriction = result.Restriction;
				Assert.IsNotNull(restriction);
				Assert.AreEqual(data.MinStart, restriction.StartTimeLimitation.StartTime);
				Assert.AreEqual(data.MaxStart, restriction.StartTimeLimitation.EndTime);
				Assert.AreEqual(data.MinEnd, restriction.EndTimeLimitation.StartTime);
				Assert.AreEqual(data.MaxEnd, restriction.EndTimeLimitation.EndTime);
				Assert.AreEqual(data.MinLength, restriction.WorkTimeLimitation.StartTime);
				Assert.AreEqual(data.MaxLength, restriction.WorkTimeLimitation.EndTime);
			}	
		}

		[Test]
		public void ShouldCreateShiftCategory()
		{
			var data = new AgentPreferenceData
				{
					ShiftCategory = _shiftCategory,
					MinStart = TimeSpan.FromHours(1),
					MaxStart = TimeSpan.FromHours(2),
					MinEnd = TimeSpan.FromHours(3),
					MaxEnd = TimeSpan.FromHours(4),
					MinLength = TimeSpan.FromHours(2),
					MaxLength = TimeSpan.FromHours(3)
				};

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
			}

			using (_mock.Playback())
			{
				var result = _preferenceDayCreator.Create(_scheduleDay, data);
				Assert.IsNotNull(result);
				var restriction = result.Restriction;
				Assert.IsNotNull(restriction);
				Assert.AreEqual(_shiftCategory, restriction.ShiftCategory);
				Assert.AreEqual(data.MinStart, restriction.StartTimeLimitation.StartTime);
				Assert.AreEqual(data.MaxStart, restriction.StartTimeLimitation.EndTime);
				Assert.AreEqual(data.MinEnd, restriction.EndTimeLimitation.StartTime);
				Assert.AreEqual(data.MaxEnd, restriction.EndTimeLimitation.EndTime);
				Assert.AreEqual(data.MinLength, restriction.WorkTimeLimitation.StartTime);
				Assert.AreEqual(data.MaxLength, restriction.WorkTimeLimitation.EndTime);
			}
		}

		[Test]
		public void ShouldCreateActivity()
		{
			var data = new AgentPreferenceData
			{
				Activity = _activity,
				MinStartActivity = TimeSpan.FromHours(1),
				MaxStartActivity = TimeSpan.FromHours(2),
				MinEndActivity = TimeSpan.FromHours(3),
				MaxEndActivity = TimeSpan.FromHours(4),
				MinLengthActivity = TimeSpan.FromHours(2),
				MaxLengthActivity = TimeSpan.FromHours(3)
			};

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
			}

			using (_mock.Playback())
			{
				var result = _preferenceDayCreator.Create(_scheduleDay, data);
				Assert.IsNotNull(result);
				var restriction = result.Restriction;
				Assert.IsNotNull(restriction);
				var activityRestriction = restriction.ActivityRestrictionCollection[0];
				Assert.AreEqual(_activity, activityRestriction.Activity);
				Assert.AreEqual(data.MinStartActivity, activityRestriction.StartTimeLimitation.StartTime);
				Assert.AreEqual(data.MaxStartActivity, activityRestriction.StartTimeLimitation.EndTime);
				Assert.AreEqual(data.MinEndActivity, activityRestriction.EndTimeLimitation.StartTime);
				Assert.AreEqual(data.MaxEndActivity, activityRestriction.EndTimeLimitation.EndTime);
				Assert.AreEqual(data.MinLengthActivity, activityRestriction.WorkTimeLimitation.StartTime);
				Assert.AreEqual(data.MaxLengthActivity, activityRestriction.WorkTimeLimitation.EndTime);
			}
		}

		[Test]
		public void ShouldCreateShiftCategoryAndActivity()
		{
			var data = new AgentPreferenceData
			{
				ShiftCategory = _shiftCategory,
				Activity = _activity,
				MinStart = TimeSpan.FromHours(1),
				MaxStart = TimeSpan.FromHours(2),
				MinEnd = TimeSpan.FromHours(3),
				MaxEnd = TimeSpan.FromHours(4),
				MinLength = TimeSpan.FromHours(2),
				MaxLength = TimeSpan.FromHours(3),

				MinStartActivity = TimeSpan.FromHours(1),
				MaxStartActivity = TimeSpan.FromHours(2),
				MinEndActivity = TimeSpan.FromHours(3),
				MaxEndActivity = TimeSpan.FromHours(4),
				MinLengthActivity = TimeSpan.FromHours(2),
				MaxLengthActivity = TimeSpan.FromHours(3)
			};

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
			}

			using (_mock.Playback())
			{
				var result = _preferenceDayCreator.Create(_scheduleDay, data);
				Assert.IsNotNull(result);
				var restriction = result.Restriction;
				Assert.IsNotNull(restriction);
				Assert.AreEqual(_shiftCategory, restriction.ShiftCategory);
				Assert.AreEqual(data.MinStart, restriction.StartTimeLimitation.StartTime);
				Assert.AreEqual(data.MaxStart, restriction.StartTimeLimitation.EndTime);
				Assert.AreEqual(data.MinEnd, restriction.EndTimeLimitation.StartTime);
				Assert.AreEqual(data.MaxEnd, restriction.EndTimeLimitation.EndTime);
				Assert.AreEqual(data.MinLength, restriction.WorkTimeLimitation.StartTime);
				Assert.AreEqual(data.MaxLength, restriction.WorkTimeLimitation.EndTime);

				var activityRestriction = restriction.ActivityRestrictionCollection[0];
				Assert.AreEqual(_activity, activityRestriction.Activity);
				Assert.AreEqual(data.MinStart, activityRestriction.StartTimeLimitation.StartTime);
				Assert.AreEqual(data.MaxStart, activityRestriction.StartTimeLimitation.EndTime);
				Assert.AreEqual(data.MinEnd, activityRestriction.EndTimeLimitation.StartTime);
				Assert.AreEqual(data.MaxEnd, activityRestriction.EndTimeLimitation.EndTime);
				Assert.AreEqual(data.MinLength, activityRestriction.WorkTimeLimitation.StartTime);
				Assert.AreEqual(data.MaxLength, activityRestriction.WorkTimeLimitation.EndTime);
			}	
		}

		[Test]
		public void ShouldCreateAbsence()
		{
			var data = new AgentPreferenceData
			{
				Absence = _absence,
				MinStartActivity = TimeSpan.FromHours(1),
				MaxStartActivity = TimeSpan.FromHours(2),
				MinEndActivity = TimeSpan.FromHours(3),
				MaxEndActivity = TimeSpan.FromHours(4),
				MinLengthActivity = TimeSpan.FromHours(2),
				MaxLengthActivity = TimeSpan.FromHours(3)
			};

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
			}

			using (_mock.Playback())
			{
				var result = _preferenceDayCreator.Create(_scheduleDay, data);
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
			var data = new AgentPreferenceData
			{
				DayOffTemplate = _dayOffTemplate,
				MinStartActivity = TimeSpan.FromHours(1),
				MaxStartActivity = TimeSpan.FromHours(2),
				MinEndActivity = TimeSpan.FromHours(3),
				MaxEndActivity = TimeSpan.FromHours(4),
				MinLengthActivity = TimeSpan.FromHours(2),
				MaxLengthActivity = TimeSpan.FromHours(3)
			};

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
			}

			using (_mock.Playback())
			{
				var result = _preferenceDayCreator.Create(_scheduleDay, data);
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
			var data = new AgentPreferenceData
			{
				MinStartActivity = TimeSpan.FromHours(2),
				MaxStartActivity = TimeSpan.FromHours(1),
				MinEndActivity = TimeSpan.FromHours(3),
				MaxEndActivity = TimeSpan.FromHours(4),
				MinLengthActivity = TimeSpan.FromHours(2),
				MaxLengthActivity = TimeSpan.FromHours(3)
			};

			var result = _preferenceDayCreator.Create(_scheduleDay, data);
			Assert.IsNull(result);
		}

		[Test]
		public void ShouldThrowExceptionOnEmptyDay()
		{
			var data = new AgentPreferenceData();
			Assert.Throws<ArgumentNullException>(() => _preferenceDayCreator.Create(null, data));	
		}

		[Test]
		public void ShouldThrowExceptionOnEmptyData()
		{
			Assert.Throws<ArgumentNullException>(() => _preferenceDayCreator.Create(_scheduleDay, null));
		}

		[Test]
		public void ShouldThrowExceptionOnEmptyDataCanCreate()
		{
			Assert.Throws<ArgumentNullException>(() => _preferenceDayCreator.CanCreate(null));
		}

		[Test]
		public void ShouldBeAbleToCreateWithValidTimes()
		{
			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(1),
				MaxStart = TimeSpan.FromHours(2),
				MinEnd = TimeSpan.FromHours(3),
				MaxEnd = TimeSpan.FromHours(4),
				MinLength = TimeSpan.FromHours(2),
				MaxLength = TimeSpan.FromHours(3),

				MinStartActivity = TimeSpan.FromHours(1),
				MaxStartActivity = TimeSpan.FromHours(2),
				MinEndActivity = TimeSpan.FromHours(3),
				MaxEndActivity = TimeSpan.FromHours(4),
				MinLengthActivity = TimeSpan.FromHours(2),
				MaxLengthActivity = TimeSpan.FromHours(3)
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);
		}

		[Test]
		public void ShouldHandleEmptyStartTimeMin()
		{
			var data = new AgentPreferenceData
			{
				
				MaxStart = TimeSpan.FromHours(2),
				MinEnd = TimeSpan.FromHours(3),
				MaxEnd = TimeSpan.FromHours(4),
				MinLength = TimeSpan.FromHours(2),
				MaxLength = TimeSpan.FromHours(3),

				MaxStartActivity = TimeSpan.FromHours(2),
				MinEndActivity = TimeSpan.FromHours(3),
				MaxEndActivity = TimeSpan.FromHours(4),
				MinLengthActivity = TimeSpan.FromHours(2),
				MaxLengthActivity = TimeSpan.FromHours(3)
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);
		}

		[Test]
		public void ShouldHandleEmptyStartTimeMax()
		{
			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(1),
				MinEnd = TimeSpan.FromHours(3),
				MaxEnd = TimeSpan.FromHours(4),
				MinLength = TimeSpan.FromHours(2),
				MaxLength = TimeSpan.FromHours(3),

				MinStartActivity = TimeSpan.FromHours(1),
				MinEndActivity = TimeSpan.FromHours(3),
				MaxEndActivity = TimeSpan.FromHours(4),
				MinLengthActivity = TimeSpan.FromHours(2),
				MaxLengthActivity = TimeSpan.FromHours(3)
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);
		}

		[Test]
		public void ShouldHandleEmptyStartTime()
		{
			
			var data = new AgentPreferenceData
			{
				MinEnd = TimeSpan.FromHours(3),
				MaxEnd = TimeSpan.FromHours(4),
				MinLength = TimeSpan.FromHours(2),
				MaxLength = TimeSpan.FromHours(3),

				MinEndActivity = TimeSpan.FromHours(3),
				MaxEndActivity = TimeSpan.FromHours(4),
				MinLengthActivity = TimeSpan.FromHours(2),
				MaxLengthActivity = TimeSpan.FromHours(3)
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);	
		}

		[Test]
		public void ShouldDetectStartTimeMinGreaterThanStartTimeMax()
		{
			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(2),
				MaxStart = TimeSpan.FromHours(1),
				MinEnd = TimeSpan.FromHours(3),
				MaxEnd = TimeSpan.FromHours(4),
				MinLength = TimeSpan.FromHours(2),
				MaxLength = TimeSpan.FromHours(3),

				MinStartActivity = TimeSpan.FromHours(2),
				MaxStartActivity = TimeSpan.FromHours(1),
				MinEndActivity = TimeSpan.FromHours(3),
				MaxEndActivity = TimeSpan.FromHours(4),
				MinLengthActivity = TimeSpan.FromHours(2),
				MaxLengthActivity = TimeSpan.FromHours(3)
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsTrue(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);	
		}

		[Test]
		public void ShouldHandleEmptyEndTimeMin()
		{
			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(1),
				MaxStart = TimeSpan.FromHours(2),
				MaxEnd = TimeSpan.FromHours(4),
				MinLength = TimeSpan.FromHours(2),
				MaxLength = TimeSpan.FromHours(3),

				MinStartActivity = TimeSpan.FromHours(1),
				MaxStartActivity = TimeSpan.FromHours(2),
				MaxEndActivity = TimeSpan.FromHours(4),
				MinLengthActivity = TimeSpan.FromHours(2),
				MaxLengthActivity = TimeSpan.FromHours(3)
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);	
		}

		[Test]
		public void ShouldHandleEmptyEndTimeMax()
		{
			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(1),
				MaxStart = TimeSpan.FromHours(2),
				MinEnd = TimeSpan.FromHours(3),
				MinLength = TimeSpan.FromHours(2),
				MaxLength = TimeSpan.FromHours(3),

				MinStartActivity = TimeSpan.FromHours(1),
				MaxStartActivity = TimeSpan.FromHours(2),
				MinEndActivity = TimeSpan.FromHours(3),
				MinLengthActivity = TimeSpan.FromHours(2),
				MaxLengthActivity = TimeSpan.FromHours(3)
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);	
		}

		[Test]
		public void ShouldHandleEmptyEndTime()
		{
			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(1),
				MaxStart = TimeSpan.FromHours(2),
				MinLength = TimeSpan.FromHours(2),
				MaxLength = TimeSpan.FromHours(3),

				MinStartActivity = TimeSpan.FromHours(1),
				MaxStartActivity = TimeSpan.FromHours(2),
				MinLengthActivity = TimeSpan.FromHours(2),
				MaxLengthActivity = TimeSpan.FromHours(3)
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);
		}

		[Test]
		public void ShouldDetectEndTimeMinGreaterThanEndTimeMax()
		{
			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(1),
				MaxStart = TimeSpan.FromHours(2),
				MinEnd = TimeSpan.FromHours(4),
				MaxEnd = TimeSpan.FromHours(3),
				MinLength = TimeSpan.FromHours(2),
				MaxLength = TimeSpan.FromHours(3),

				MinStartActivity = TimeSpan.FromHours(1),
				MaxStartActivity = TimeSpan.FromHours(2),
				MinEndActivity = TimeSpan.FromHours(4),
				MaxEndActivity = TimeSpan.FromHours(3),
				MinLengthActivity = TimeSpan.FromHours(2),
				MaxLengthActivity = TimeSpan.FromHours(3)
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsFalse(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsTrue(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsTrue(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);
		}

		[Test]
		public void ShouldHandleEndTimeNextDay()
		{
			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(1),
				MaxStart = TimeSpan.FromHours(2),
				MinEnd = TimeSpan.FromHours(25),
				MaxEnd = TimeSpan.FromHours(26),
				MinLength = TimeSpan.FromHours(2),
				MaxLength = TimeSpan.FromHours(24),
			};

			var result = _preferenceDayCreator.CanCreate(data);

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
			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(1),
				MaxStart = TimeSpan.FromHours(2),
				MinEnd = TimeSpan.FromHours(3),
				MaxEnd = TimeSpan.FromHours(4),
				MaxLength = TimeSpan.FromHours(3),

				MinStartActivity = TimeSpan.FromHours(1),
				MaxStartActivity = TimeSpan.FromHours(2),
				MinEndActivity = TimeSpan.FromHours(3),
				MaxEndActivity = TimeSpan.FromHours(4),
				MaxLengthActivity = TimeSpan.FromHours(3)
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);	
		}

		[Test]
		public void ShouldHandleEmptyMaxLength()
		{
			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(1),
				MaxStart = TimeSpan.FromHours(2),
				MinEnd = TimeSpan.FromHours(3),
				MaxEnd = TimeSpan.FromHours(4),
				MinLength = TimeSpan.FromHours(2),

				MinStartActivity = TimeSpan.FromHours(1),
				MaxStartActivity = TimeSpan.FromHours(2),
				MinEndActivity = TimeSpan.FromHours(3),
				MaxEndActivity = TimeSpan.FromHours(4),
				MinLengthActivity = TimeSpan.FromHours(2),
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);
		}

		[Test]
		public void ShouldHandleEmptyLength()
		{
			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(1),
				MaxStart = TimeSpan.FromHours(2),
				MinEnd = TimeSpan.FromHours(3),
				MaxEnd = TimeSpan.FromHours(4),
				
				MinStartActivity = TimeSpan.FromHours(1),
				MaxStartActivity = TimeSpan.FromHours(2),
				MinEndActivity = TimeSpan.FromHours(3),
				MaxEndActivity = TimeSpan.FromHours(4),
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsTrue(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);
		}

		[Test]
		public void ShouldDetectMinLengthGreaterThanMaxLength()
		{
			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(1),
				MaxStart = TimeSpan.FromHours(2),
				MinEnd = TimeSpan.FromHours(3),
				MaxEnd = TimeSpan.FromHours(4),
				MinLength = TimeSpan.FromHours(3),
				MaxLength = TimeSpan.FromHours(2),

				MinStartActivity = TimeSpan.FromHours(1),
				MaxStartActivity = TimeSpan.FromHours(2),
				MinEndActivity = TimeSpan.FromHours(3),
				MaxEndActivity = TimeSpan.FromHours(4),
				MinLengthActivity = TimeSpan.FromHours(3),
				MaxLengthActivity = TimeSpan.FromHours(2)
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsFalse(result.Result);
			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsTrue(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);

			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsTrue(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);	
		}

		[Test]
		public void ShouldDetectStartTimeMinGreaterThanEndTimeMin()
		{
			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(3),
				MinEnd = TimeSpan.FromHours(3),

				MinStartActivity = TimeSpan.FromHours(3),
				MinEndActivity = TimeSpan.FromHours(3),
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsTrue(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);
		}

		[Test]
		public void ShouldDetectStartTimeMinGreaterThanEndTimeMax()
		{

			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(4),
				MaxEnd = TimeSpan.FromHours(4),

				MinStartActivity = TimeSpan.FromHours(4),
				MaxEndActivity = TimeSpan.FromHours(4),
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsTrue(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);
		}

		[Test]
		public void ShouldDetectStartTimeMaxGreaterThanEndTimeMin()
		{
			var data = new AgentPreferenceData
			{
				MaxStart = TimeSpan.FromHours(4),
				MinEnd = TimeSpan.FromHours(3),

				MaxStartActivity = TimeSpan.FromHours(4),
				MinEndActivity = TimeSpan.FromHours(3),
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsFalse(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsTrue(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsTrue(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);
		}

		[Test]
		public void ShouldDetectStartTimeMaxGreaterThanEndTimeMax()
		{
			var data = new AgentPreferenceData
			{
				MaxStart = TimeSpan.FromHours(4),
				MaxEnd = TimeSpan.FromHours(4),

				MaxStartActivity = TimeSpan.FromHours(4),
				MaxEndActivity = TimeSpan.FromHours(4),
				
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsFalse(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsTrue(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsTrue(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);
		}

		[Test]
		public void ShouldDetectCannotFulfillMinLength()
		{
			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(1),
				MaxStart = TimeSpan.FromHours(2),
				MinEnd = TimeSpan.FromHours(3),
				MaxEnd = TimeSpan.FromHours(4),
				MinLength = TimeSpan.FromHours(5),

				MinStartActivity = TimeSpan.FromHours(1),
				MaxStartActivity = TimeSpan.FromHours(2),
				MinEndActivity = TimeSpan.FromHours(3),
				MaxEndActivity = TimeSpan.FromHours(4),
				MinLengthActivity = TimeSpan.FromHours(5)
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsFalse(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsTrue(result.LengthMinError);
			Assert.IsFalse(result.LengthMaxError);

			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsTrue(result.LengthMinErrorActivity);
			Assert.IsFalse(result.LengthMaxErrorActivity);	
		}

		[Test]
		public void ShouldDetectCannotFulfillMaxLength()
		{
			var data = new AgentPreferenceData
			{
				MinStart = TimeSpan.FromHours(1),
				MaxStart = TimeSpan.FromHours(2),
				MinEnd = TimeSpan.FromHours(6),
				MaxLength = TimeSpan.FromHours(1),

				MinStartActivity = TimeSpan.FromHours(1),
				MaxStartActivity = TimeSpan.FromHours(2),
				MinEndActivity = TimeSpan.FromHours(6),
				MaxLengthActivity = TimeSpan.FromHours(1)
			};

			var result = _preferenceDayCreator.CanCreate(data);

			Assert.IsFalse(result.Result);
			Assert.IsFalse(result.StartTimeMinError);
			Assert.IsFalse(result.StartTimeMaxError);
			Assert.IsFalse(result.EndTimeMinError);
			Assert.IsFalse(result.EndTimeMaxError);
			Assert.IsFalse(result.LengthMinError);
			Assert.IsTrue(result.LengthMaxError);

			Assert.IsFalse(result.StartTimeMinErrorActivity);
			Assert.IsFalse(result.StartTimeMaxErrorActivity);
			Assert.IsFalse(result.EndTimeMinErrorActivity);
			Assert.IsFalse(result.EndTimeMaxErrorActivity);
			Assert.IsFalse(result.LengthMinErrorActivity);
			Assert.IsTrue(result.LengthMaxErrorActivity);	
		}

		[Test]
		public void ShouldNotBeAbleToCreateWhenDayOffTemplateAndShiftCategory()
		{
			var data = new AgentPreferenceData
			{
				ShiftCategory = _shiftCategory,
				DayOffTemplate = _dayOffTemplate
			};

			var result = _preferenceDayCreator.CanCreate(data);
			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.ConflictingTypeError);
		}

		[Test]
		public void ShouldNotBeAbleToCreateWhenDayOffTemplateAndAbsence()
		{
			var data = new AgentPreferenceData
			{
				Absence = _absence,
				DayOffTemplate = _dayOffTemplate
			};

			var result = _preferenceDayCreator.CanCreate(data);
			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.ConflictingTypeError);
		}

		[Test]
		public void ShouldNotBeAbleToCreateWhenDayOffTemplateAndActivity()
		{
			var data = new AgentPreferenceData
			{
				Activity = _activity,
				DayOffTemplate = _dayOffTemplate
			};

			var result = _preferenceDayCreator.CanCreate(data);
			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.ConflictingTypeError);
		}

		[Test]
		public void ShouldNotBeAbleToCreateWhenShiftCategoryAndAbsence()
		{
			var data = new AgentPreferenceData
			{
				ShiftCategory = _shiftCategory,
				Absence = _absence
			};

			var result = _preferenceDayCreator.CanCreate(data);
			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.ConflictingTypeError);
		}

		
		[Test]
		public void ShouldNotBeAbleToCreateWhenActivityAndAbsence()
		{
			var data = new AgentPreferenceData
			{
				Absence = _absence,
				Activity = _activity
			};

			var result = _preferenceDayCreator.CanCreate(data);
			Assert.IsFalse(result.Result);
			Assert.IsTrue(result.ConflictingTypeError);
		}
	}
}
