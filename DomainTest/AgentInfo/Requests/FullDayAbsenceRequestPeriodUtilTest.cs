using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
    [TestFixture]
    public class FullDayAbsenceRequestPeriodUtilTest
    {
	    private IScenario scenario;
        private IGlobalSettingDataRepository globalSettingDataRepository;

        [SetUp]
        public void Setup()
        {
            scenario = new Scenario("-");
            globalSettingDataRepository = new FakeGlobalSettingDataRepository();
        }

		[Test]
		[Category("AdjustFullDayAbsencePeriodIfRequired")]
		public void ShouldApplyHiddenGlobalSettingsToFullDayAbsence()
		{
			var person = PersonFactory.CreatePerson(TimeZoneInfo.Utc);

			var today = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(today, today.AddHours(23).AddMinutes(59));

			var startTimeSpan = new TimeSpan(6, 0, 0);
			var endTimeSpan = new TimeSpan(22, 0, 0);

			var scheduleDay = new fakeScheduleDay(null);
				
			setGlobalFullDayAbsencePeriod(new TimeSpanSetting(startTimeSpan), new TimeSpanSetting(endTimeSpan));

			var adjustedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(period, person,
				scheduleDay, scheduleDay,
				globalSettingDataRepository);

			Assert.IsTrue(adjustedPeriod.StartDateTime.TimeOfDay == startTimeSpan);
			Assert.IsTrue(adjustedPeriod.EndDateTime.TimeOfDay == endTimeSpan);
		}

		[Test]
		[Category("AdjustFullDayAbsencePeriodIfRequired")]
		public void ShouldApplyHiddenGlobalSettingsToFullDayAbsenceWithDayOff()
		{
			var person = PersonFactory.CreatePerson(TimeZoneInfo.Utc);

			var today = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var scheduledPeriod = new DateTimePeriod(today, today.AddHours(23).AddMinutes(59));

			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario,
				today.ToDateOnly(),
				new DayOffTemplate());

			var startTimeSpan = new TimeSpan(6, 0, 0);
			var endTimeSpan = new TimeSpan(22, 0, 0);

			var scheduleDay = new fakeScheduleDay(personAssignment, true);

			setGlobalFullDayAbsencePeriod(new TimeSpanSetting(startTimeSpan), new TimeSpanSetting(endTimeSpan));

			var expectedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(scheduledPeriod,
				person, scheduleDay, scheduleDay,
				globalSettingDataRepository);
			Assert.IsTrue(scheduleDay.HasDayOff());
			Assert.IsTrue(expectedPeriod.StartDateTime.TimeOfDay == startTimeSpan);
			Assert.IsTrue(expectedPeriod.EndDateTime.TimeOfDay == endTimeSpan);
		}

		[Test]
		[Category("AdjustFullDayAbsencePeriodIfRequired")]
		public void ShouldApplyShiftTimeToFullDayAbsence()
		{
		    var settingStart = new TimeSpan (6, 0, 0);
		    var settingEnd = new TimeSpan (22, 0, 0);

			var person = PersonFactory.CreatePerson(TimeZoneInfo.Utc);

			var today = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var requestPeriod = new DateTimePeriod(today, today.AddHours(23).AddMinutes(59));
			var schedulePeriod = new DateTimePeriod(today, today.AddHours(23));

			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario,schedulePeriod);
			var scheduleDay = new fakeScheduleDay(personAssignment, true);
			setGlobalFullDayAbsencePeriod(new TimeSpanSetting(settingStart),new TimeSpanSetting(settingEnd));

			var adjustedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(requestPeriod, person,
				scheduleDay, scheduleDay,
				globalSettingDataRepository);

			Assert.IsTrue(adjustedPeriod.StartDateTime == personAssignment.Period.StartDateTime);
			Assert.IsTrue(adjustedPeriod.EndDateTime == personAssignment.Period.EndDateTime);
		}

        [Test]
		[Category("AdjustFullDayAbsencePeriodIfRequired")]
		public void ShouldApplyEarlyShiftTimeToFullDayAbsence()
        {
			var settingStart = new TimeSpan(0, 0, 0);
			var settingEnd = new TimeSpan(23, 0, 0);

			var person = PersonFactory.CreatePerson(TimeZoneInfo.Utc);

			var today = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var requestPeriod = new DateTimePeriod(today, today.AddHours(23).AddMinutes(59));
			var schedulePeriod = new DateTimePeriod(today.AddHours(10), today.AddHours(23));

			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, schedulePeriod);
			var scheduleDay = new fakeScheduleDay(personAssignment, true);
			setGlobalFullDayAbsencePeriod(new TimeSpanSetting(settingStart), new TimeSpanSetting(settingEnd));

			var adjustedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(requestPeriod, person,
				scheduleDay, scheduleDay,
				globalSettingDataRepository);

			Assert.IsTrue(adjustedPeriod.StartDateTime == personAssignment.Period.StartDateTime);
			Assert.IsTrue(adjustedPeriod.EndDateTime == personAssignment.Period.EndDateTime);
		}

        [Test]
		[Category("AdjustFullDayAbsencePeriodIfRequired")]
		public void ShouldApplyLateShiftTimeToFullDayAbsence()
        {
			var person = PersonFactory.CreatePerson(TimeZoneInfo.Utc);

			var today = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var requestPeriod = new DateTimePeriod(today, today.AddHours(23).AddMinutes(59));
			var schedulePeriod = new DateTimePeriod(today, today.AddHours(22));

			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, schedulePeriod);
			var scheduleDay = new fakeScheduleDay(personAssignment, true);
			setGlobalFullDayAbsencePeriod();

			var adjustedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(requestPeriod, person,
				scheduleDay, scheduleDay,
				globalSettingDataRepository);

			Assert.IsTrue(adjustedPeriod.StartDateTime == personAssignment.Period.StartDateTime);
			Assert.IsTrue(adjustedPeriod.EndDateTime == personAssignment.Period.EndDateTime);
		}

        [Test]
		[Category("AdjustFullDayAbsencePeriodIfRequired")]
		public void ShouldApplyShiftTimeInsideSettingPeriodToFullDayAbsence()
        {
            var settingStart = new TimeSpan(10, 0, 0);
            var settingEnd = new TimeSpan(22, 00, 0);

			var person = PersonFactory.CreatePerson(TimeZoneInfo.Utc);

			var today = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var requestPeriod = new DateTimePeriod(today, today.AddHours(23).AddMinutes(59));
			var schedulePeriod = new DateTimePeriod(today.AddHours(12), today.AddHours(20));

			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, schedulePeriod);
			var scheduleDay = new fakeScheduleDay(personAssignment, true);
			setGlobalFullDayAbsencePeriod(new TimeSpanSetting(settingStart), new TimeSpanSetting(settingEnd));

			var adjustedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(requestPeriod, person,
				scheduleDay, scheduleDay,
				globalSettingDataRepository);

			Assert.IsTrue(adjustedPeriod.StartDateTime == personAssignment.Period.StartDateTime);
			Assert.IsTrue(adjustedPeriod.EndDateTime == personAssignment.Period.EndDateTime);
		}

        [Test]
		[Category("AdjustFullDayAbsencePeriodIfRequired")]
		public void ShouldApplyShiftTimeToFullDayAbsenceWhereShiftIsOvernight()
        {
            var settingStart = new TimeSpan(00, 0, 0);
            var settingEnd = new TimeSpan(23, 00, 0);

			var person = PersonFactory.CreatePerson(TimeZoneInfo.Utc);

			var today = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var requestPeriod = new DateTimePeriod(today, today.AddHours(23).AddMinutes(59));
			var schedulePeriod = new DateTimePeriod(today.AddHours(22), today.AddDays(1).AddHours(6));

			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, schedulePeriod);
			var scheduleDay = new fakeScheduleDay(personAssignment, true);
			setGlobalFullDayAbsencePeriod(new TimeSpanSetting(settingStart), new TimeSpanSetting(settingEnd));

			var adjustedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(requestPeriod, person,
				scheduleDay, scheduleDay,
				globalSettingDataRepository);

			Assert.IsTrue(adjustedPeriod.StartDateTime == personAssignment.Period.StartDateTime);
			Assert.IsTrue(adjustedPeriod.EndDateTime == personAssignment.Period.EndDateTime);
		}

		[Test]
		[Category("AdjustFullDayAbsencePeriodIfRequired")]
		public void EmptyPersonAssignmentShouldWorkTheSameAsEmptyDay()
		{
			var person = PersonFactory.CreatePerson(TimeZoneInfo.Utc);
			var today = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(today, today.Add(new TimeSpan(23, 59, 0)));

			var personAssignment = new PersonAssignment(person, scenario, new DateOnly(2011, 3, 4));
			var scheduleDay = new fakeScheduleDay(personAssignment, true);

			setGlobalFullDayAbsencePeriod();

			var adjustedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(period, person,
				scheduleDay, scheduleDay,
				globalSettingDataRepository);

			adjustedPeriod.StartDateTime.Should().Be.EqualTo(today);
			adjustedPeriod.EndDateTime.Should().Be.EqualTo(today.Add(new TimeSpan(23, 59, 0)));
		}

		[Test]
		[Category("AdjustFullDayAbsencePeriodForOvernightShift")]
		public void ShouldNotAdjustPeriodIfRequestIsNotFullDay()
		{
			var person = PersonFactory.CreatePerson(TimeZoneInfo.Utc);

			var today = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var requestPeriod = new DateTimePeriod(today, today.Add(new TimeSpan(23, 59, 0)));

			var scheduleDay = new fakeScheduleDay(null);
			var fullDayAbsenceRequest = new fakeAbsenceRequest(person, requestPeriod);

			setGlobalFullDayAbsencePeriod();

			var adjustedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodForOvernightShift(
				fullDayAbsenceRequest, new List<IScheduleDay> { scheduleDay },
				globalSettingDataRepository);

			Assert.IsTrue(adjustedPeriod == requestPeriod);
		}

		[Test]
		[Category("AdjustFullDayAbsencePeriodForOvernightShift")]
		public void ShouldNotAdjustPeriodForScheduleDaysLessThanTwo()
		{
			var person = PersonFactory.CreatePerson(TimeZoneInfo.Utc);

			var today = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var requestPeriod = new DateTimePeriod(today, today.Add(new TimeSpan(23,59,0)));

			var scheduleDay = new fakeScheduleDay(null);
			var fullDayAbsenceRequest = new fakeAbsenceRequest(person, requestPeriod, true);

			setGlobalFullDayAbsencePeriod();

			var adjustedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodForOvernightShift(
				fullDayAbsenceRequest, new List<IScheduleDay> {scheduleDay},
				globalSettingDataRepository);

			Assert.IsTrue(adjustedPeriod == requestPeriod);
		}

		[Test]
		[Category("AdjustFullDayAbsencePeriodForOvernightShift")]
		public void ShouldUseGlobalFullDayAbsenceSettingsIfNoScheduledShift()
		{
			var settingStart = new TimeSpan(07, 0, 0);
			var settingEnd = new TimeSpan(23, 00, 0);

			var today = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var requestPeriod = new DateTimePeriod(today, today.Add(new TimeSpan(23, 59, 0)));

			var person = PersonFactory.CreatePerson(TimeZoneInfo.Utc);
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var daysToCoverOverNightShift = requestPeriod.ChangeStartTime(TimeSpan.FromDays(-1))
				.ToDateOnlyPeriod(timeZone)
				.DayCollection();

			var scheduleDays =
				getFakeScheduleDays(daysToCoverOverNightShift, new List<IPersonAssignment>());
			var fullDayAbsenceRequest = new fakeAbsenceRequest(person, requestPeriod,true);

			setGlobalFullDayAbsencePeriod(new TimeSpanSetting(settingStart), new TimeSpanSetting(settingEnd));

			var adjustedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodForOvernightShift(
				fullDayAbsenceRequest, scheduleDays,
				globalSettingDataRepository);

			Assert.IsTrue(adjustedPeriod != requestPeriod);
			Assert.IsTrue(adjustedPeriod.StartDateTime.TimeOfDay == settingStart);
			Assert.IsTrue(adjustedPeriod.EndDateTime.TimeOfDay == settingEnd);
		}

		[Test]
		[Category("AdjustFullDayAbsencePeriodForOvernightShift")]
		public void ShouldUseGlobalFullDayAbsenceSettingsIfDayOff()
		{
			var settingStart = new TimeSpan(07, 0, 0);
			var settingEnd = new TimeSpan(23, 00, 0);

			var today = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var requestPeriod = new DateTimePeriod(today, today.Add(new TimeSpan(23, 59, 0)));

			var person = PersonFactory.CreatePerson(TimeZoneInfo.Utc);
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var daysToCoverOverNightShift = requestPeriod.ChangeStartTime(TimeSpan.FromDays(-1))
				.ToDateOnlyPeriod(timeZone)
				.DayCollection();
			var personAssignment =
				PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, today.ToDateOnly(),
					new DayOffTemplate());
			var scheduleDays =
				getFakeScheduleDays(daysToCoverOverNightShift, new List<IPersonAssignment>{ personAssignment });
			var fullDayAbsenceRequest = new fakeAbsenceRequest(person, requestPeriod, true);

			setGlobalFullDayAbsencePeriod(new TimeSpanSetting(settingStart), new TimeSpanSetting(settingEnd));

			var adjustedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodForOvernightShift(
				fullDayAbsenceRequest, scheduleDays,
				globalSettingDataRepository);

			Assert.IsTrue(adjustedPeriod != requestPeriod);
			Assert.IsTrue(adjustedPeriod.StartDateTime.TimeOfDay == settingStart);
			Assert.IsTrue(adjustedPeriod.EndDateTime.TimeOfDay == settingEnd);
		}

		[Test]
		[Category("AdjustFullDayAbsencePeriodForOvernightShift")]
		public void ShouldCoverOvernightShiftBelongsToSingleDayRequestPeriod()
		{
			var day1 = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var day2 = new DateTime(2011, 3, 5, 0, 0, 0, DateTimeKind.Utc);

			var requestPeriod = new DateTimePeriod(day1, day1.Add(new TimeSpan(23, 59, 0)));
			var scheduledPeriod = new DateTimePeriod(day1.AddHours(22), day2.AddHours(4));

			var person = PersonFactory.CreatePerson(TimeZoneInfo.Utc);
			var personAssignment =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, scheduledPeriod);

			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var daysToCoverOverNightShift = requestPeriod.ChangeStartTime(TimeSpan.FromDays(-1))
				.ToDateOnlyPeriod(timeZone)
				.DayCollection();

			var scheduleDays =
				getFakeScheduleDays(daysToCoverOverNightShift, new List<IPersonAssignment> {personAssignment});
			var fullDayAbsenceRequest = new fakeAbsenceRequest(person, requestPeriod, true);
			setGlobalFullDayAbsencePeriod();

			var adjustedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodForOvernightShift(
				fullDayAbsenceRequest, scheduleDays, globalSettingDataRepository);

			var expectedPeriod = new DateTimePeriod(requestPeriod.StartDateTime, scheduledPeriod.EndDateTime);

			Assert.IsTrue(adjustedPeriod == expectedPeriod);
		}

		[Test]
		[Category("AdjustFullDayAbsencePeriodForOvernightShift")]
		public void ShouldFilterOvernightShiftDoesNotBelongsToSingleDayRequestPeriod()
		{
			var day1 = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var day2 = new DateTime(2011, 3, 5, 0, 0, 0, DateTimeKind.Utc);

			var requestPeriod = new DateTimePeriod(day2, day2.AddHours(23).AddMinutes(59));
			var scheduledPeriod = new DateTimePeriod(day1.AddHours(22), day2.AddHours(4));

			var person = PersonFactory.CreatePerson(TimeZoneInfo.Utc);
			var personAssignment =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, scheduledPeriod);

			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var daysToCoverOverNightShift = requestPeriod.ChangeStartTime(TimeSpan.FromDays(-1))
				.ToDateOnlyPeriod(timeZone)
				.DayCollection();

			var scheduleDays =
				getFakeScheduleDays(daysToCoverOverNightShift, new List<IPersonAssignment> {personAssignment});
			var fullDayAbsenceRequest = new fakeAbsenceRequest(person, requestPeriod, true);

			setGlobalFullDayAbsencePeriod();

			var adjustedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodForOvernightShift(
				fullDayAbsenceRequest, scheduleDays, globalSettingDataRepository);
			var expectedPeriod = new DateTimePeriod(scheduledPeriod.EndDateTime, requestPeriod.EndDateTime);

			Assert.IsTrue(adjustedPeriod == expectedPeriod);
		}

		[Test]
		[Category("AdjustFullDayAbsencePeriodForOvernightShift")]
		public void ShouldCoverOvernightShiftBelongsToMultiDaysRequestPeriod()
		{
			var day1 = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var day2 = new DateTime(2011, 3, 5, 0, 0, 0, DateTimeKind.Utc);
			var day3 = new DateTime(2011, 3, 6, 0, 0, 0, DateTimeKind.Utc);

			var requestPeriod = new DateTimePeriod(day1, day2.AddHours(23).AddMinutes(59));
			var scheduledPeriod = new DateTimePeriod(day2.AddHours(22), day3.AddHours(4));

			var person = PersonFactory.CreatePerson(TimeZoneInfo.Utc);
			var personAssignment =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, scheduledPeriod);

			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var daysToCoverOverNightShift = requestPeriod.ChangeStartTime(TimeSpan.FromDays(-1))
				.ToDateOnlyPeriod(timeZone)
				.DayCollection();

			var scheduleDays =
				getFakeScheduleDays(daysToCoverOverNightShift, new List<IPersonAssignment> {personAssignment});
			var fullDayAbsenceRequest = new fakeAbsenceRequest(person, requestPeriod, true);

			setGlobalFullDayAbsencePeriod();

			var adjustedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodForOvernightShift(
				fullDayAbsenceRequest, scheduleDays, globalSettingDataRepository);

			var expectedPeriod = new DateTimePeriod(requestPeriod.StartDateTime, scheduledPeriod.EndDateTime);

			Assert.IsTrue(adjustedPeriod == expectedPeriod);
		}

		[Test]
		[Category("AdjustFullDayAbsencePeriodForOvernightShift")]
		public void ShouldFilterOvernightShiftDoesNotBelongsToMultiDaysRequestPeriod()
		{
			var day1 = new DateTime(2011, 3, 4, 0, 0, 0, DateTimeKind.Utc);
			var day2 = new DateTime(2011, 3, 5, 0, 0, 0, DateTimeKind.Utc);
			var day3 = new DateTime(2011, 3, 6, 0, 0, 0, DateTimeKind.Utc);

			var requestPeriod = new DateTimePeriod(day2, day3.AddHours(23).AddMinutes(59));
			var scheduledPeriod = new DateTimePeriod(day1.AddHours(22), day2.AddHours(4));

			var person = PersonFactory.CreatePerson(TimeZoneInfo.Utc);
			var personAssignment =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, scheduledPeriod);

			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var daysToCoverOverNightShift = requestPeriod.ChangeStartTime(TimeSpan.FromDays(-1))
				.ToDateOnlyPeriod(timeZone)
				.DayCollection();

			var scheduleDays =
				getFakeScheduleDays(daysToCoverOverNightShift, new List<IPersonAssignment> {personAssignment});
			var fullDayAbsenceRequest = new fakeAbsenceRequest(person, requestPeriod, true);

			setGlobalFullDayAbsencePeriod();

			var adjustedPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodForOvernightShift(
				fullDayAbsenceRequest, scheduleDays, globalSettingDataRepository);

			var expectedPeriod = new DateTimePeriod(scheduledPeriod.EndDateTime, requestPeriod.EndDateTime);

			Assert.IsTrue(adjustedPeriod == expectedPeriod);
		}

		private void setGlobalFullDayAbsencePeriod(TimeSpanSetting startTimeSetting = null, TimeSpanSetting endTimeSetting = null)
		{
			var defaultStartTimeSetting = new TimeSpanSetting(new TimeSpan(0, 0, 0));
			var defaultEndTimeSetting = new TimeSpanSetting(new TimeSpan(23, 59, 0));
			startTimeSetting = startTimeSetting ?? defaultStartTimeSetting;
			endTimeSetting = endTimeSetting ?? defaultEndTimeSetting;
			globalSettingDataRepository.PersistSettingValue("FullDayAbsenceRequestStartTime", startTimeSetting);
			globalSettingDataRepository.PersistSettingValue("FullDayAbsenceRequestEndTime", endTimeSetting);
		}

		private List<IScheduleDay> getFakeScheduleDays(IList<DateOnly> dayCollection,
			IList<IPersonAssignment> personAssignments)
		{
			var scheduleDays = new List<IScheduleDay>();
			foreach (var dateOnly in dayCollection)
			{
				var personAssignment = personAssignments.SingleOrDefault(x => x.Date == dateOnly);
				if (personAssignment != null)
				{
					var scheduleDay = new fakeScheduleDay(personAssignment, true);
					scheduleDays.Add(scheduleDay);
				}
				else
				{
					scheduleDays.Add(new fakeScheduleDay(null));
				}
			}

			return scheduleDays;
		}

		private class fakeScheduleDay:IScheduleDay
		{
			private IPersonAssignment _assignment;
			private bool _isSchedule;

			public fakeScheduleDay(IPersonAssignment assignment, bool isSchedule = false)
			{
				_assignment = assignment;
				_isSchedule = isSchedule;
			}
			public bool IsScheduled()
			{
				return _isSchedule;
			}

			public bool HasDayOff()
			{
				return _assignment?.DayOff()!=null;
			}

			public IPersonAssignment PersonAssignment(bool createIfNotExists = false)
			{
				return _assignment;
			}

			#region Not Implememted		
			public IProjectionService ProjectionService()
			{
				throw new NotImplementedException();
			}

			public void Restore(IScheduleDay previousState)
			{
				throw new NotImplementedException();
			}

			public IMemento CreateMemento()
			{
				throw new NotImplementedException();
			}

			public DateTimePeriod Period { get; }
			public IPerson Person { get; }
			public IScenario Scenario { get; }
			public object Clone()
			{
				throw new NotImplementedException();
			}

			public IScheduleDictionary Owner { get; }
			public IList<IBusinessRuleResponse> BusinessRuleResponseInternalCollection { get; }
			public IDateOnlyAsDateTimePeriod DateOnlyAsPeriod { get; }
			public SchedulePartView SignificantPart()
			{
				throw new NotImplementedException();
			}

			public IPersonMeeting[] PersonMeetingCollection(bool includeOutsideActualDay)
			{
				throw new NotImplementedException();
			}

			public IPersonAbsence[] PersonAbsenceCollection(bool includeOutsideActualDay)
			{
				throw new NotImplementedException();
			}

			public IPersonAbsence[] PersonAbsenceCollection()
			{
				throw new NotImplementedException();
			}

			public SchedulePartView SignificantPartForDisplay()
			{
				throw new NotImplementedException();
			}

			public IEditableShift GetEditorShift()
			{
				throw new NotImplementedException();
			}

			public void DeleteFullDayAbsence(IScheduleDay source)
			{
				throw new NotImplementedException();
			}

			public void MergeAbsences(IScheduleDay source, bool all)
			{
				throw new NotImplementedException();
			}

			public void DeleteAbsence(bool all)
			{
				throw new NotImplementedException();
			}

			public void Merge(IScheduleDay source, bool isDelete, ITimeZoneGuard timeZoneGuard)
			{
				throw new NotImplementedException();
			}

			public void Merge(IScheduleDay source, bool isDelete, bool ignoreTimeZoneChanges, ITimeZoneGuard timeZoneGuard,
				bool ignoreAssignmentPermission = false)
			{
				throw new NotImplementedException();
			}

			public bool FullAccess { get; }
			public IPersonMeeting[] PersonMeetingCollection()
			{
				throw new NotImplementedException();
			}

			public IOvertimeAvailability[] OvertimeAvailablityCollection()
			{
				throw new NotImplementedException();
			}

			public IScheduleData[] PersonRestrictionCollection()
			{
				throw new NotImplementedException();
			}

			public IEnumerable<IRestrictionBase> RestrictionCollection()
			{
				throw new NotImplementedException();
			}

			public INote[] NoteCollection()
			{
				throw new NotImplementedException();
			}

			public IPublicNote[] PublicNoteCollection()
			{
				throw new NotImplementedException();
			}

			public IList<IBusinessRuleResponse> BusinessRuleResponseCollection { get; }
			public bool IsFullyPublished { get; }
			public IEnumerable<IPersistableScheduleData> PersistableScheduleDataCollection()
			{
				throw new NotImplementedException();
			}

			public void Clear<T>() where T : IScheduleData
			{
				throw new NotImplementedException();
			}

			public void Add(IScheduleData scheduleData)
			{
				throw new NotImplementedException();
			}

			public void Remove(IScheduleData scheduleData)
			{
				throw new NotImplementedException();
			}

			public TimeZoneInfo TimeZone { get; }
			public IScheduleTag ScheduleTag()
			{
				throw new NotImplementedException();
			}

			public void CreateAndAddDayOff(IDayOffTemplate dayOff)
			{
				throw new NotImplementedException();
			}

			public IPersonAbsence CreateAndAddAbsence(IAbsenceLayer layer)
			{
				throw new NotImplementedException();
			}

			public void CreateAndAddActivity(IActivity activity, DateTimePeriod period, IShiftCategory shiftCategory)
			{
				throw new NotImplementedException();
			}

			public IScheduleDay CreateAndAddActivity(IActivity activity, DateTimePeriod period)
			{
				throw new NotImplementedException();
			}

			public void CreateAndAddPersonalActivity(IActivity activity, DateTimePeriod period, bool muteEvent = true,
				TrackedCommandInfo trackedCommandInfo = null)
			{
				throw new NotImplementedException();
			}

			public void CreateAndAddNote(string text)
			{
				throw new NotImplementedException();
			}

			public void CreateAndAddPublicNote(string text)
			{
				throw new NotImplementedException();
			}

			public void CreateAndAddOvertime(IActivity activity, DateTimePeriod period, IMultiplicatorDefinitionSet definitionSet,
				bool muteEvent = false, TrackedCommandInfo trackedCommandInfo = null)
			{
				throw new NotImplementedException();
			}

			public void AddMainShift(IEditableShift mainShift)
			{
				throw new NotImplementedException();
			}

			public void DeleteMainShift()
			{
				throw new NotImplementedException();
			}

			public void DeleteMainShiftSpecial()
			{
				throw new NotImplementedException();
			}

			public void DeletePersonalStuff()
			{
				throw new NotImplementedException();
			}

			public void DeleteDayOff(TrackedCommandInfo trackedCommandInfo = null)
			{
				throw new NotImplementedException();
			}

			public void DeleteOvertime()
			{
				throw new NotImplementedException();
			}

			public void DeletePreferenceRestriction()
			{
				throw new NotImplementedException();
			}

			public void DeleteStudentAvailabilityRestriction()
			{
				throw new NotImplementedException();
			}

			public void DeleteOvertimeAvailability()
			{
				throw new NotImplementedException();
			}

			public void DeleteNote()
			{
				throw new NotImplementedException();
			}

			public void DeletePublicNote()
			{
				throw new NotImplementedException();
			}

			public IScheduleDay ReFetch()
			{
				throw new NotImplementedException();
			}

			public void AddMainShift(IPersonAssignment mainShiftSource)
			{
				throw new NotImplementedException();
			}

			public bool HasProjection()
			{
				throw new NotImplementedException();
			}

			public IPreferenceDay PreferenceDay()
			{
				throw new NotImplementedException();
			}

			public DateTimePeriod AbsenceSplitPeriod(IScheduleDay scheduleDay)
			{
				throw new NotImplementedException();
			}

			public void AdjustFullDayAbsenceNextDay(IScheduleDay destination)
			{
				throw new NotImplementedException();
			}
			#endregion
		}

		private class fakeAbsenceRequest : IAbsenceRequest
		{
			private IPerson _person;
			private DateTimePeriod _requestPeriod;

			public fakeAbsenceRequest(IPerson person, DateTimePeriod requestPeriod, bool fullDay = false)
			{
				_person = person;
				_requestPeriod = requestPeriod;
				FullDay = fullDay;
			}

			public DateTimePeriod Period
			{
				get { return _requestPeriod; }
			}

			public IPerson Person
			{
				get { return _person; }
			}

			public bool FullDay { get; set; }

			#region Not Implemented
			public object Clone()
			{
				throw new NotImplementedException();
			}

			public IRequest NoneEntityClone()
			{
				throw new NotImplementedException();
			}

			public IRequest EntityClone()
			{
				throw new NotImplementedException();
			}

			public bool Equals(IEntity other)
			{
				throw new NotImplementedException();
			}

			public Guid? Id { get; }

			public void SetId(Guid? newId)
			{
				throw new NotImplementedException();
			}

			public void ClearId()
			{
				throw new NotImplementedException();
			}

			public IEntity Parent { get; }

			public IAggregateRoot Root()
			{
				throw new NotImplementedException();
			}

			public void SetParent(IEntity parent)
			{
				throw new NotImplementedException();
			}


			public string RequestTypeDescription { get; set; }
			public RequestType RequestType { get; }
			public Description RequestPayloadDescription { get; }

			public void Deny(IPerson denyPerson)
			{
				throw new NotImplementedException();
			}

			public string GetDetails(CultureInfo cultureInfo)
			{
				throw new NotImplementedException();
			}

			public string TextForNotification { get; set; }
			public bool ShouldNotifyWithMessage { get; }
			public IList<IPerson> ReceiversForNotification { get; }
			public IPerson PersonTo { get; }
			public IPerson PersonFrom { get; }
			public IAbsence Absence { get; }

			public bool IsRequestForOneLocalDay(TimeZoneInfo timeZone)
			{
				throw new NotImplementedException();
			}

			#endregion
		}
	}
}
