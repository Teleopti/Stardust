using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

using System.Globalization;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;

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

			Assert.AreEqual(_targetView.EarlyStartTime, _startTimeLimit.StartTime);
			Assert.AreEqual(_targetView.LateStartTime, _startTimeLimit.EndTime);
			Assert.AreEqual(_targetView.StartTimeLimit(), _startTimeLimit);

			Assert.AreEqual(_targetView.EarlyEndTime, _endTimeLimit.StartTime);
			Assert.AreEqual(_targetView.LateEndTime, _endTimeLimit.EndTime);
			Assert.AreEqual(_targetView.EndTimeLimit(), _endTimeLimit);

			Assert.AreEqual(_targetView.MinimumWorkTime, _workTimePeriod.StartTime);
			Assert.AreEqual(_targetView.MaximumWorkTime, _workTimePeriod.EndTime);
			Assert.AreEqual(_targetView.WorkTimeLimit(), _workTimePeriod);

			Assert.AreEqual(_targetView.Week, ScheduleRestrictionBaseView.GetWeek(_dayCount));

			Assert.AreEqual(string.Empty, _targetView.UpdatedBy);
			Assert.AreEqual(string.Empty, _targetView.UpdatedOn);

			ReflectionHelper.SetUpdatedBy(_studRestriction, _createPerson);
			ReflectionHelper.SetUpdatedOn(_studRestriction, _createDate.AddHours(1));

			Assert.AreEqual(_createPerson.Name.ToString(), _targetView.UpdatedBy);
			Assert.AreEqual(_createDate.AddHours(1).ToString(CultureInfo.CurrentCulture), _targetView.UpdatedOn);

			string dayName = UserTexts.Resources.Day + " " + _dayCount.ToString(CultureInfo.CurrentUICulture);
			Assert.AreEqual(_targetView.Day, dayName);

		}

		[Test]
		public void VerifyStartTimeLimitChange()
		{
			Assert.IsNotNull(_targetView);

			Assert.AreEqual(_targetView.EarlyStartTime, _startTimeLimit.StartTime);

			var time = new TimeSpan(8, 25,0);
			StartTimeLimitation timeLimit = new StartTimeLimitation(time, time);
			_targetView.LateStartTime = timeLimit.EndTime;
			_targetView.EarlyStartTime = timeLimit.StartTime;

			Assert.AreEqual(_targetView.EarlyStartTime, timeLimit.StartTime);
			Assert.AreEqual(_targetView.LateStartTime, timeLimit.EndTime);
		}

		[Test]
		public void VerifyEndTimeLimitChange()
		{
			Assert.IsNotNull(_targetView);

			Assert.AreEqual(_targetView.LateEndTime, _endTimeLimit.EndTime);

			var time = new TimeSpan(8, 25, 0);
			EndTimeLimitation timeLimit = new EndTimeLimitation(time, time);
			_targetView.EarlyEndTime = timeLimit.StartTime;
			_targetView.LateEndTime = timeLimit.EndTime;

			Assert.AreEqual(_targetView.EarlyEndTime, timeLimit.StartTime);
			Assert.AreEqual(_targetView.LateEndTime, timeLimit.EndTime);
		}

		[Test]
		public void VerifyMinimumWorkChange()
		{
			Assert.IsNotNull(_targetView);

			_targetView.MinimumWorkTime = TimeSpan.FromHours(5);

			Assert.AreEqual(_targetView.MinimumWorkTime, TimeSpan.FromHours(5));

			_targetView.MinimumWorkTime = _minTimeWork;
			Assert.AreEqual(_targetView.MinimumWorkTime, _workTimePeriod.StartTime);
		}

		[Test]
		public void VerifyMaximumWorkChange()
		{
			Assert.IsNotNull(_targetView);

			_targetView.MaximumWorkTime = TimeSpan.FromHours(10);

			Assert.AreEqual(_targetView.MaximumWorkTime, TimeSpan.FromHours(10));

			_targetView.MinimumWorkTime = _maxTimeWork;
			Assert.AreEqual(_targetView.MinimumWorkTime, _workTimePeriod.EndTime);
		}

		[Test]
		public void VerifyWorkingTimeChange()
		{
			Assert.IsNotNull(_targetView);

			Assert.Throws<ArgumentOutOfRangeException>(delegate { _targetView.MaximumWorkTime = TimeSpan.FromHours(5); });
			Assert.Throws<ArgumentOutOfRangeException>(delegate { _targetView.MinimumWorkTime = TimeSpan.FromHours(10); });
		}

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
		public void TryCrashGetWeek()
		{
			Assert.IsNotNull(_targetView);
			Assert.Throws<ArgumentOutOfRangeException>(() => ScheduleRestrictionBaseView.GetWeek(int.MinValue));
		}

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
		public void VerifyTimePeriodIsValidRange()
		{
			// Normal - From(lo) < To(hi)
			_startTimeLimit = new StartTimeLimitation(new TimeSpan(8,0,0), null);
			_endTimeLimit = new EndTimeLimitation(null, new TimeSpan(10,0,0));
			Assert.IsTrue(ScheduleRestrictionBaseView.IsValidRange(_startTimeLimit.StartTime, _endTimeLimit.EndTime));
			// Normal - From(lo) < To(hi)Overnight
			_startTimeLimit = new StartTimeLimitation(new TimeSpan(8, 0, 0), null);
			_endTimeLimit = new EndTimeLimitation(null, new TimeSpan(34, 0, 0));
			Assert.IsTrue(ScheduleRestrictionBaseView.IsValidRange(_startTimeLimit.StartTime, _endTimeLimit.EndTime));
			// Normal - From == To
			_startTimeLimit = new StartTimeLimitation(new TimeSpan(8, 0, 0), null);
			_endTimeLimit = new EndTimeLimitation(null, new TimeSpan(8, 0, 0));
			Assert.IsTrue(ScheduleRestrictionBaseView.IsValidRange(_startTimeLimit.StartTime, _endTimeLimit.EndTime));

			// From(hi) > To(lo)
			_startTimeLimit = new StartTimeLimitation(new TimeSpan(18, 0, 0), null);
			_endTimeLimit = new EndTimeLimitation(null, new TimeSpan(10, 0, 0));
			Assert.IsFalse(ScheduleRestrictionBaseView.IsValidRange(_startTimeLimit.StartTime, _endTimeLimit.EndTime));
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
