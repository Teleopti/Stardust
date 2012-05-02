using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Interfaces.Domain;
using System.Globalization;
using System.Collections.Generic;

namespace Teleopti.Ccc.WinCodeTest.Settings
{
	[TestFixture, SetCulture("sv-SE")]
	public class ScheduleRestrictionBaseViewTest
	{
		private TestView _targetView;

		private IRestrictionBase _containedEntity;
		private EndTimeLimitation _endTimeLimit;
		private StartTimeLimitation _startTimeLimit;
		private TimeSpan _minTimeWork;
		private TimeSpan _maxTimeWork;
		private WorkTimeLimitation _workTimePeriod;
		private readonly int _dayCount = 5;
		private StudentAvailabilityDay _studRestriction;
		private IPerson _createPerson;
		private DateTime _createDate;
		private StudentAvailabilityRestriction _studentAvailabilityRestriction;

		[SetUp]
		public void Init()
		{
			_createPerson = PersonFactory.CreatePerson();
			_createPerson.SetId(Guid.NewGuid());
			_createDate = new DateTime(2009, 1, 1);
			_studentAvailabilityRestriction = new StudentAvailabilityRestriction();
			_studRestriction = new StudentAvailabilityDay(_createPerson, new DateOnly(2009, 1, 1), new List<IStudentAvailabilityRestriction> { _studentAvailabilityRestriction });
			_minTimeWork = new TimeSpan(7, 0, 0);
			_maxTimeWork = new TimeSpan(8, 30, 0);
			_endTimeLimit = new EndTimeLimitation();
			_startTimeLimit = new StartTimeLimitation();

			_workTimePeriod = new WorkTimeLimitation(_minTimeWork, _maxTimeWork);
			_containedEntity = new TestDomainClass
			{
				EndTimeLimitation = _endTimeLimit,
				StartTimeLimitation = _startTimeLimit,
				WorkTimeLimitation = _workTimePeriod
			};
			_containedEntity.SetParent(_studRestriction);

			int weekNumber = ScheduleRestrictionBaseView.GetWeek(_dayCount);

			_targetView = new TestView(_containedEntity, weekNumber, _dayCount);
		}

		[Test]
		public void VerifyPropertyValues()
		{
			Assert.IsNotNull(_targetView);

			//Assert.AreEqual(_targetView.IsAvailable, _available);
			Assert.AreEqual(_targetView.EarlyStartTime, _startTimeLimit.StartTimeString);
			Assert.AreEqual(_targetView.LateStartTime, _startTimeLimit.EndTimeString);
			Assert.AreEqual(_targetView.StartTimeLimit(), _startTimeLimit);

			Assert.AreEqual(_targetView.EarlyEndTime, _endTimeLimit.StartTimeString);
			Assert.AreEqual(_targetView.LateEndTime, _endTimeLimit.EndTimeString);
			Assert.AreEqual(_targetView.EndTimeLimit(), _endTimeLimit);

			Assert.AreEqual(_targetView.MinimumWorkTime, _workTimePeriod.StartTimeString);
			Assert.AreEqual(_targetView.MaximumWorkTime, _workTimePeriod.EndTimeString);
			Assert.AreEqual(_targetView.WorkTimeLimit(), _workTimePeriod);

			Assert.AreEqual(_targetView.Week, ScheduleRestrictionBaseView.GetWeek(_dayCount));

			Assert.AreEqual(string.Empty, _targetView.UpdatedBy);
			Assert.AreEqual(string.Empty, _targetView.UpdatedOn);
			Assert.AreEqual(string.Empty, _targetView.CreatedBy);
			Assert.AreEqual(string.Empty, _targetView.CreatedOn);

			ReflectionHelper.SetUpdatedBy(_studRestriction, _createPerson);
			ReflectionHelper.SetCreatedBy(_studRestriction, _createPerson);
			ReflectionHelper.SetCreatedOn(_studRestriction, _createDate);
			ReflectionHelper.SetUpdatedOn(_studRestriction, _createDate.AddHours(1));

			Assert.AreEqual(_createPerson.Name.ToString(), _targetView.UpdatedBy);
			Assert.AreEqual(_createDate.AddHours(1).ToString(CultureInfo.CurrentCulture), _targetView.UpdatedOn);
			Assert.AreEqual(_createPerson.Name.ToString(), _targetView.CreatedBy);
			Assert.AreEqual(_createDate.ToString(CultureInfo.CurrentCulture), _targetView.CreatedOn);

			//int day = ScheduleRestrictionBaseView.GetDayOfWeek(_dayCount);
			//IList<DayOfWeek> daysOfWeek = DateHelper.GetDaysOfWeek(CultureInfo.CurrentUICulture);
			//DayOfWeek dayOfWeek = daysOfWeek[day];

			string dayName = UserTexts.Resources.Day + " " + _dayCount.ToString(CultureInfo.CurrentUICulture);
			Assert.AreEqual(_targetView.Day, dayName);

		}

		[Test]
		public void VerifyStartTimeLimitChange()
		{
			Assert.IsNotNull(_targetView);

			Assert.AreEqual(_targetView.EarlyStartTime, _startTimeLimit.StartTimeString);

			string time = "0825";
			StartTimeLimitation timeLimit = new StartTimeLimitation();
			timeLimit.EndTimeString = time;
			timeLimit.StartTimeString = time;
			_targetView.LateStartTime = timeLimit.EndTimeString;
			_targetView.EarlyStartTime = timeLimit.StartTimeString;

			Assert.AreEqual(_targetView.EarlyStartTime, timeLimit.StartTimeString);
			Assert.AreEqual(_targetView.LateStartTime, timeLimit.EndTimeString);

			//_targetView.MinimumStartTime = _startTimeLimit.StartTimeString;
			//Assert.AreEqual(_targetView.MinimumStartTime, _startTimeLimit.StartTimeString);
		}

		[Test]
		public void VerifyEndTimeLimitChange()
		{
			Assert.IsNotNull(_targetView);

			Assert.AreEqual(_targetView.LateEndTime, _endTimeLimit.EndTimeString);

			string time = "0825";
			EndTimeLimitation timeLimit = new EndTimeLimitation();
			timeLimit.StartTimeString = time;
			timeLimit.EndTimeString = time;
			_targetView.EarlyEndTime = timeLimit.StartTimeString;
			_targetView.LateEndTime = timeLimit.EndTimeString;

			Assert.AreEqual(_targetView.EarlyEndTime, timeLimit.StartTimeString);
			Assert.AreEqual(_targetView.LateEndTime, timeLimit.EndTimeString);

			//_targetView.MaximumEndTime = _endTimeLimit.EndTimeString;
			//Assert.AreEqual(_targetView.MaximumEndTime, _endTimeLimit.EndTimeString);
		}

		[Test]
		public void VerifyMinimumWorkChange()
		{
			Assert.IsNotNull(_targetView);

			_targetView.MinimumWorkTime = "5:00";

			Assert.AreEqual(_targetView.MinimumWorkTime, "5:00");

			_targetView.MinimumWorkTime = _minTimeWork.ToString();
			Assert.AreEqual(_targetView.MinimumWorkTime, _workTimePeriod.StartTimeString);
		}

		[Test]
		public void VerifyMaximumWorkChange()
		{
			Assert.IsNotNull(_targetView);

			_targetView.MaximumWorkTime = "1000";

			Assert.AreEqual("10:00", _targetView.MaximumWorkTime);

			_targetView.MinimumWorkTime = _maxTimeWork.ToString();
			Assert.AreEqual(_targetView.MinimumWorkTime, _workTimePeriod.EndTimeString);
		}

		[Test]
		public void VerifyWorkingTimeChange()
		{
			Assert.IsNotNull(_targetView);

			Assert.Throws<ArgumentOutOfRangeException>(delegate { _targetView.MaximumWorkTime = "0500"; });
			Assert.Throws<ArgumentOutOfRangeException>(delegate { _targetView.MinimumWorkTime = "10"; });

			//_targetView.MaximumWorkTime = "0500";
			// can't set to less than min
			//Assert.AreEqual("08:30", _targetView.MaximumWorkTime);
			//_targetView.MinimumWorkTime = "10";
			//Assert.AreEqual("07:00", _targetView.MinimumWorkTime);

			_targetView.MinimumWorkTime = "500";
			Assert.AreEqual("5:00", _targetView.MinimumWorkTime);
			_targetView.MaximumWorkTime = "10";
			Assert.AreEqual("10:00", _targetView.MaximumWorkTime);

		}

		//[Test]
		//public void VerifyAvailableChange()
		//{
		//    Assert.IsNotNull(_targetView);

		//    Assert.AreEqual(_targetView.IsAvailable, _available);

		//    bool notAvailable = false;
		//    _targetView.IsAvailable = notAvailable;

		//    Assert.AreEqual(_targetView.IsAvailable, notAvailable);
		//    Assert.AreNotEqual(_targetView.IsAvailable, _available);

		//    _targetView.IsAvailable = _available;
		//    Assert.AreEqual(_targetView.IsAvailable, _available);
		//    Assert.AreNotEqual(_targetView.IsAvailable, notAvailable);
		//}

		[Test]
		public void VerifyGetWeek()
		{
			Assert.IsNotNull(_targetView);

			int minDates = 2;
			int maxDates = 10;
			int boundyDates = 6;
			int boundyDates2 = 13;
			int maxDates2 = 14;

			Assert.AreEqual(ScheduleRestrictionBaseView.GetWeek(minDates), 1);
			Assert.AreEqual(ScheduleRestrictionBaseView.GetWeek(boundyDates), 1);
			Assert.AreEqual(ScheduleRestrictionBaseView.GetWeek(maxDates), 2);
			Assert.AreEqual(ScheduleRestrictionBaseView.GetWeek(boundyDates2), 2);
			Assert.AreEqual(ScheduleRestrictionBaseView.GetWeek(maxDates2), 3);

		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TryCrashGetWeek()
		{
			Assert.IsNotNull(_targetView);

			ScheduleRestrictionBaseView.GetWeek(int.MinValue);
		}

		//[Test]
		//public void VerifyGetDayOfWeek()
		//{
		//    Assert.IsNotNull(_targetView);

		//    int minDate = 2;
		//    int boundaryDate = 6;
		//    int upperBoundaryDate = 7;

		//    Assert.AreEqual(minDate, ScheduleRestrictionBaseView.GetDayOfWeek(minDate));
		//    Assert.AreEqual(boundaryDate, ScheduleRestrictionBaseView.GetDayOfWeek(boundaryDate));
		//    Assert.AreEqual(0, ScheduleRestrictionBaseView.GetDayOfWeek(upperBoundaryDate));
		//}

		//[Test]
		//[ExpectedException(typeof(ArgumentOutOfRangeException))]
		//public void TryCrashGetDayOfWeek()
		//{
		//    Assert.IsNotNull(_targetView);

		//    ScheduleRestrictionBaseView.GetDayOfWeek(int.MinValue);
		//}

		[Test]
		public void TryParseAvailabilityRestrictions()
		{
			Assert.IsNotNull(_targetView);

			int days = 14;
			Description desc = new Description("Some Name");

			IAvailabilityRotation availability = new AvailabilityRotation(desc.Name, days);
			List<AvailabilityRestrictionView> list = new List<AvailabilityRestrictionView>();

			list.AddRange(ScheduleRestrictionBaseView.Parse(availability));

			Assert.AreEqual(list.Count, days);

			availability.RemoveDays(7);

			list.Clear();

			Assert.AreEqual(list.Count, 0);

			list.AddRange(ScheduleRestrictionBaseView.Parse(availability));
			Assert.AreEqual(list.Count, 7);
		}

		//[Test]
		//public void TryParseRotationRestrictions()
		//{
		//    Assert.IsNotNull(_targetView);

		//    int days = 14;
		//    Description desc = new Description("Some Name");

		//    IRotation rotation = new Rotation(desc.Name, days);
		//    List<RotationRestrictionView> list = new List<RotationRestrictionView>();
		//    IDayOffTemplate holidayTemplate = new DayOffTemplate(new Description("Holiday"));

		//    list.AddRange(ScheduleRestrictionBaseView.Parse(rotation, holidayTemplate));

		//    Assert.AreEqual(list.Count, days);
		//    Assert.AreEqual(list[5].DayOffTemplate, holidayTemplate);
		//    Assert.AreEqual(list[6].DayOffTemplate, holidayTemplate);
		//    Assert.AreEqual(list[12].DayOffTemplate, holidayTemplate);
		//    Assert.AreEqual(list[13].DayOffTemplate, holidayTemplate);

		//    rotation.RemoveDays(7);

		//    list.Clear();

		//    Assert.AreEqual(list.Count, 0);

		//    list.AddRange(ScheduleRestrictionBaseView.Parse(rotation, holidayTemplate));
		//    Assert.AreEqual(list.Count, 7);
		//    Assert.AreEqual(list[5].DayOffTemplate, holidayTemplate);
		//    Assert.AreEqual(list[6].DayOffTemplate, holidayTemplate);
		//}

		[Test]
		public void TryParseRotationRestrictionsWithoutDayOffTemplates()
		{
			Assert.IsNotNull(_targetView);

			int days = 14;
			Description desc = new Description("Some Name");

			IRotation rotation = new Rotation(desc.Name, days);
			List<RotationRestrictionView> list = new List<RotationRestrictionView>();

			list.AddRange(ScheduleRestrictionBaseView.Parse(rotation));

			foreach (RotationRestrictionView view in list)
			{
				Assert.AreEqual(view.DayOffTemplate, RotationRestrictionView.DefaultDayOff);
			}

			Assert.AreEqual(list.Count, days);

			rotation.RemoveDays(7);

			list.Clear();

			Assert.AreEqual(list.Count, 0);

			list.AddRange(ScheduleRestrictionBaseView.Parse(rotation));
			foreach (RotationRestrictionView view in list)
			{
				Assert.AreEqual(view.DayOffTemplate, RotationRestrictionView.DefaultDayOff);
			}
			Assert.AreEqual(list.Count, 7);
		}

		[Test]
		public void VerifyOvernightChange()
		{
			_endTimeLimit.EndTimeString = "0800";
			string time = ScheduleRestrictionBaseView.ToOvernight(_endTimeLimit.EndTimeString);
			Assert.AreNotEqual(time, _endTimeLimit.EndTimeString);

			_endTimeLimit.EndTimeString = "0000";
			string time1 = ScheduleRestrictionBaseView.ToOvernight(_endTimeLimit.EndTimeString);
			Assert.AreNotEqual(time1, _endTimeLimit.EndTimeString);

			string time2 = ScheduleRestrictionBaseView.ToOvernight(time);
			Assert.AreEqual(time, time2);

			string time3 = ScheduleRestrictionBaseView.ToOvernight("+1");
			Assert.AreEqual(time3, "+1");
		}

		[Test]
		public void VerifyTimePeriodIsValidRange()
		{
			// Normal - From(lo) < To(hi)
			_startTimeLimit.StartTimeString = "0800";
			_endTimeLimit.EndTimeString = "1000";
			Assert.IsTrue(ScheduleRestrictionBaseView.IsValidRange(_startTimeLimit.StartTime, _endTimeLimit.EndTime));
			// Normal - From(lo) < To(hi)Overnight
			_startTimeLimit.StartTimeString = "0800";
			_endTimeLimit.EndTimeString = "1000+1";
			Assert.IsTrue(ScheduleRestrictionBaseView.IsValidRange(_startTimeLimit.StartTime, _endTimeLimit.EndTime));
			// Normal - From == To
			_startTimeLimit.StartTimeString = "0800";
			_endTimeLimit.EndTimeString = _startTimeLimit.StartTimeString;
			Assert.IsTrue(ScheduleRestrictionBaseView.IsValidRange(_startTimeLimit.StartTime, _endTimeLimit.EndTime));

			// From(hi) > To(lo)
			_startTimeLimit.StartTimeString = "1800";
			_endTimeLimit.EndTimeString = "1000";
			Assert.IsFalse(ScheduleRestrictionBaseView.IsValidRange(_startTimeLimit.StartTime, _endTimeLimit.EndTime));
			//// From(hi)Overnight > To(lo)
			//_startTimeLimit.StartTimeString = "0800+1";
			//_endTimeLimit.EndTimeString = "1000";
			//Assert.IsFalse(ScheduleRestrictionBaseView.IsValidRange(_startTimeLimit.StartTime, _endTimeLimit.EndTime));
		}
	}

	public sealed class TestView : ScheduleRestrictionBaseView
	{
		public TestView(IRestrictionBase target, int week, int day)
			: base(target, week, day)
		{
		}
	}

	public sealed class TestDomainClass : RestrictionBase
	{
	}
}
