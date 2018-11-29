using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[MyTimeWebTest]
	public class ScheduleStaffingPossibilityControllerAbsenceTest : IIsolateSystem
	{
		public ScheduleStaffingPossibilityController Target;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public MutableNow Now;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillTypeRepository SkillTypeRepository;
		public FakeToggleManager ToggleManager;
		public SkillIntradayStaffingFactory SkillIntradayStaffingFactory;

		private readonly TimeSpan[] intervals = { TimeSpan.FromMinutes(495), TimeSpan.FromMinutes(510) };
		readonly ISkillType phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
			.WithId();

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
			isolate.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			isolate.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
			isolate.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			isolate.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();
			isolate.UseTestDouble<FakeSkillTypeRepository>().For<ISkillTypeRepository>();
		}

		[Test, SetCulture("en-US")]
		public void ShouldNotReturnAbsencePossibiliesForDaysNotInAbsenceOpenPeriod()
		{
			setupSiteOpenHour();
			setupTestData();
			setupWorkFlowControlSet();

			var today = Now.ServerDate_DontUse();
			var absenceRequestOpenDatePeriod =
				(AbsenceRequestOpenDatePeriod)User.CurrentUser().WorkflowControlSet.AbsenceRequestOpenPeriods[0];

			absenceRequestOpenDatePeriod.Period = new DateOnlyPeriod(today.AddDays(6), today.AddDays(7));
			absenceRequestOpenDatePeriod.OpenForRequestsPeriod = new DateOnlyPeriod(today, today.AddDays(7));

			var result = getPossibilityViewModels(today.AddDays(7), StaffingPossiblityType.Absence).ToList();
			result.Count.Should().Be.EqualTo(4);
		}

		[Test, SetCulture("en-US")]
		public void ShouldReturnPossibiliesForCurrentWeek()
		{
			setupSiteOpenHour();
			setupTestData();
			setupWorkFlowControlSet();
			var result = getPossibilityViewModels(null, StaffingPossiblityType.Absence).ToList();
			result.Count.Should().Be.EqualTo(6);
			new DateOnlyPeriod(Now.ServerDate_DontUse(), Now.ServerDate_DontUse().AddDays(2)).DayCollection().ToList().ForEach(day =>
			{
				Assert.AreEqual(2, result.Count(d => d.Date == day.ToFixedClientDateOnlyFormat()));
			});
		}

		[Test, SetCulture("en-US")]
		public void ShouldReturnPossibiliesForNextWeek()
		{
			setupSiteOpenHour();
			setupTestData();
			setupWorkFlowControlSet();
			var result = getPossibilityViewModels(Now.ServerDate_DontUse().AddWeeks(1), StaffingPossiblityType.Absence).ToList();
			result.Count.Should().Be.EqualTo(14);
			DateHelper.GetWeekPeriod(Now.ServerDate_DontUse().AddWeeks(1), CultureInfo.CurrentCulture)
				.DayCollection()
				.ToList()
				.ForEach(day =>
				{
					Assert.AreEqual(2, result.Count(d => d.Date == day.ToFixedClientDateOnlyFormat()), day.ToShortDateString());
				});
		}

		[Test, SetCulture("en-US")]
		public void ShouldNotReturnPossibiliesForPastDays()
		{
			setupSiteOpenHour();
			setupTestData();
			setupWorkFlowControlSet();
			var result = getPossibilityViewModels(Now.ServerDate_DontUse().AddDays(-1), StaffingPossiblityType.Absence).ToList();
			result.Count.Should().Be.EqualTo(6);
		}

		[Test, SetCulture("en-US")]
		public void ShouldNotReturnPossibiliesForFarFutureDays()
		{
			setupSiteOpenHour();
			setupTestData();
			setupWorkFlowControlSet();
			var result = getPossibilityViewModels(Now.ServerDate_DontUse().AddWeeks(8), StaffingPossiblityType.Absence);
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnEmptyPossibilities()
		{
			var possibilities = getPossibilityViewModels(null).ToList();
			Assert.AreEqual(0, possibilities.Count);
		}

		[Test]
		public void ShouldGetPossibilitiesWhenUsingShrinkageValidator()
		{
			setupSiteOpenHour();
			setupTestData();
			var person = User.CurrentUser();
			var absence = AbsenceFactory.CreateAbsenceWithId();

			var workflowControlSet = WorkflowControlSetFactory.CreateWorkFlowControlSet(absence, new PendingAbsenceRequest(),
				false);

			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.ServerDate_DontUse().AddDays(-20), Now.ServerDate_DontUse().AddDays(20)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.ServerDate_DontUse().AddDays(-20), Now.ServerDate_DontUse().AddDays(20)),
				StaffingThresholdValidator = new StaffingThresholdWithShrinkageValidator()
			});
			person.WorkflowControlSet = workflowControlSet;

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
		}

		[Test]
		public void ShouldGetPossibilitiesWhenSomeStaffingDataIsNotAvailable()
		{
			setupSiteOpenHour();

			var activity = createActivity();
			var skill = createSkill("test1");
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personSkill);
			createAssignment(User.CurrentUser(), activity);
			var period = getAvailablePeriod();
			period.DayCollection().ToList().ForEach(day =>
			{
				var utcDate = TimeZoneHelper.ConvertToUtc(day.Date, User.CurrentUser().PermissionInformation.DefaultTimeZone());
				var staffingPeriodData1 = new StaffingPeriodData
				{
					ForecastedStaffing = 10d,
					ScheduledStaffing = 11d,
					Period = new DateTimePeriod(utcDate.Date.Add(intervals[0]), utcDate.Date.Add(intervals[0]).AddMinutes(15))
				};
				var staffingPeriodData2 = new StaffingPeriodData
				{
					ScheduledStaffing = 11d,
					Period = new DateTimePeriod(utcDate.Date.Add(intervals[1]), utcDate.Date.Add(intervals[1]).AddMinutes(15))
				};
				SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(skill, new DateOnly(utcDate),
					new[] {staffingPeriodData1, staffingPeriodData2}, User.CurrentUser().PermissionInformation.DefaultTimeZone());
			});

			setupWorkFlowControlSet();
			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForAbsenceWhenUnderstaffing()
		{
			setupSiteOpenHour();
			setupTestData();
			setupWorkFlowControlSet();
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Absence)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetGoodPossibilitiesForAbsenceWhenNotUnderstaffing()
		{
			setupSiteOpenHour();
			setupTestData(new double?[] { 10d, 10d }, new double?[] { 11d, 11d });
			setupWorkFlowControlSet();
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Absence)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairAndGoodPossibilitiesForAbsence()
		{
			setupSiteOpenHour();
			setupTestData(new double?[] { 10d, 10d }, new double?[] { 8d, 11d });
			setupWorkFlowControlSet();
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Absence)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForAbsenceWhenOneOfSkillIsUnderstaffing()
		{
			var person = User.CurrentUser();
			var activity1 = createActivity();
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, new double?[] { 10d, 10d }, new double?[] { 8d, 11d });

			var activity2 = createActivity();
			var skill2 = createSkill("skill2");
			var personSkill2 = createPersonSkill(activity2, skill2);
			setupIntradayStaffingForSkill(skill2, new double?[] { 10d, 10d }, new double?[] { 11d, 8d });

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			createAssignment(person, activity1, activity2);
			setupWorkFlowControlSet();
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Absence)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldOnlyGetOneDayDataIfReturnOneWeekDataIsFalse()
		{
			setupSiteOpenHour();
			setupTestData();
			setupWorkFlowControlSet();
			var result = getPossibilityViewModels(null, StaffingPossiblityType.Absence, false).ToList();
			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldGetPossibilitiesAccordingToAgentTimeZone()
		{
			Now.Is(Now.UtcDateTime().Date.AddHours(2));

			setupSiteOpenHour();
			setupTestData();
			setupWorkFlowControlSet();

			var timeZoneInfo = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(timeZoneInfo);

			var result = getPossibilityViewModels(null, StaffingPossiblityType.Absence).ToList();
			var today = new DateOnly(TimeZoneHelper.ConvertFromUtc(Now.UtcDateTime(),
				User.CurrentUser().PermissionInformation.DefaultTimeZone()));
			result.FirstOrDefault()?.Date.Should().Be(today.ToFixedClientDateOnlyFormat());
			result.Count.Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldSubstractCurrentUsersMainShiftWhenCalculatingAbsenceProbability()
		{
			setupTestData(new double?[] { 1, 1 }, new double?[] { 1, 1 });
			setupWorkFlowControlSet();

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldSubstractCurrentUsersOvertimeShiftWhenCalculatingAbsenceProbability()
		{
			var activity = createActivity();
			var skill = createSkill("test1");
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personSkill);
			setupIntradayStaffingForSkill(skill, new double?[] { 0.1d, 0.1d }, new double?[] { 1d, 1d });
			setupWorkFlowControlSet();

			var startDate = Now.UtcDateTime().Date.AddHours(8);
			var endDate = Now.UtcDateTime().Date.AddHours(17);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithOvertimeShift(User.CurrentUser(),
				Scenario.Current(), activity, new DateTimePeriod(startDate, endDate));
			PersonAssignmentRepository.Has(assignment);

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldUsePrimarySkillsWhenCalculatingOvertimeProbability()
		{
			setupWorkFlowControlSet();
			var primarySkill = createSkill("primarySkill");
			primarySkill.SetCascadingIndex(1);
			setupIntradayStaffingForSkill(primarySkill, new double?[] { 5, 5 }, new double?[] { 1, 1 });

			var nonPrimarySkill = createSkill("nonPrimarySkill");
			setupIntradayStaffingForSkill(nonPrimarySkill, new double?[] { 5, 5 }, new double?[] { 4, 4 });

			var activity = createActivity();
			createAssignment(User.CurrentUser(), activity);
			var primaryPersonSkill = createPersonSkill(activity, primarySkill);
			var nonPrimaryPersonSkill = createPersonSkill(activity, nonPrimarySkill);

			addPersonSkillsToPersonPeriod(primaryPersonSkill, nonPrimaryPersonSkill);

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldNotUsePrimarySkillsWhenCalculatingAbsenceProbability()
		{
			setupWorkFlowControlSet();

			var primarySkill = createSkill("primarySkill");
			primarySkill.SetCascadingIndex(1);
			setupIntradayStaffingForSkill(primarySkill, new double?[] { 5, 5 }, new double?[] { 5.8, 5.8 });

			var nonPrimarySkill = createSkill("nonPrimarySkill");
			setupIntradayStaffingForSkill(nonPrimarySkill, new double?[] { 5, 5 }, new double?[] { 1, 1 });

			var activity = createActivity();
			createAssignment(User.CurrentUser(), activity);
			var primaryPersonSkill = createPersonSkill(activity, primarySkill);
			var nonPrimaryPersonSkill = createPersonSkill(activity, nonPrimarySkill);

			addPersonSkillsToPersonPeriod(primaryPersonSkill, nonPrimaryPersonSkill);

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetAllGoodPossibilitiesWhenUnderStaffingThresholdsIsMinusOne()
		{
			var skill = createSkill("test1");
			skill.StaffingThresholds = new StaffingThresholds(new Percent(-1), new Percent(-1), new Percent(0.1));
			setupTestData(skill, new double?[] { 1, 1 }, new double?[] { 0.1, 0.2 });
			setupWorkFlowControlSet();

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForAbsenceWhenUnderstaffingIsZeroAndForcastedAndScheduledAreEqual()
		{
			var person = User.CurrentUser();
			var activity1 = createActivity();
			var skill1 = createSkill("skill1");
			skill1.StaffingThresholds = createStaffingThresholds(0, 0, 0.1);

			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, new double?[] { 10d, 10d }, new double?[] { 10d, 10d });

			addPersonSkillsToPersonPeriod(personSkill1);

			createAssignment(person, activity1);
			setupWorkFlowControlSet();

			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Absence)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();

			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		private void setupWorkFlowControlSet()
		{
			var absenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.ServerDate_DontUse().AddDays(-20), Now.ServerDate_DontUse().AddDays(20)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.ServerDate_DontUse().AddDays(-20), Now.ServerDate_DontUse().AddDays(20)),
				StaffingThresholdValidator = new StaffingThresholdValidator()
			};
			var overtimeRequestOpenDatePeriod = new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, 13)
			};
			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenDatePeriod);
			workFlowControlSet.AddOpenOvertimeRequestPeriod(overtimeRequestOpenDatePeriod);
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;
		}

		private void setupSiteOpenHour()
		{
			var personPeriod = getOrAddPersonPeriod();
			var timePeriod = new TimePeriod(8, 0, 17, 0);
			var team = personPeriod.Team;
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
		}

		private PersonPeriod getOrAddPersonPeriod()
		{
			var personPeriod =
				(PersonPeriod)User.CurrentUser().PersonPeriods(Now.ServerDate_DontUse().ToDateOnlyPeriod()).FirstOrDefault();
			if (personPeriod != null) return personPeriod;
			var team = TeamFactory.CreateTeam("team1", "site1");
			personPeriod =
				(PersonPeriod)
				PersonPeriodFactory.CreatePersonPeriod(Now.ServerDate_DontUse(), PersonContractFactory.CreatePersonContract(), team);
			User.CurrentUser().AddPersonPeriod(personPeriod);
			return personPeriod;
		}

		private void addPersonSkillsToPersonPeriod(params IPersonSkill[] personSkills)
		{
			var personPeriod = getOrAddPersonPeriod();
			foreach (var personSkill in personSkills)
			{
				personPeriod.AddPersonSkill(personSkill);
			}
		}

		private void setupTestData(double?[] forecastedStaffing = null, double?[] scheduledStaffing = null)
		{
			var skill = createSkill("test1");
			setupTestData(skill, forecastedStaffing, scheduledStaffing);
		}

		private void setupTestData(ISkill skill, double?[] forecastedStaffing = null, double?[] scheduledStaffing = null)
		{
			var activity = createActivity();
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personSkill);
			createAssignment(User.CurrentUser(), activity);
			forecastedStaffing = forecastedStaffing ?? new Double?[] { 10d, 10d };
			scheduledStaffing = scheduledStaffing ?? new Double?[] { 8d, 8d };
			setupIntradayStaffingForSkill(skill, forecastedStaffing, scheduledStaffing);
		}

		private IPersonSkill createPersonSkill(IActivity activity, ISkill skill)
		{
			skill.Activity = activity;
			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			return personSkill;
		}

		private ISkill createSkill(string name, TimePeriod? openHour = null)
		{
			var skill = SkillFactory.CreateSkill(name).WithId();
			skill.SkillType = phoneSkillType;
			skill.StaffingThresholds = createStaffingThresholds();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHour ?? new TimePeriod(8, 00, 9, 30));
			SkillRepository.Has(skill);
			return skill;
		}

		private static StaffingThresholds createStaffingThresholds()
		{
			return new StaffingThresholds(new Percent(-0.3), new Percent(-0.1), new Percent(0.1));
		}

		private static StaffingThresholds createStaffingThresholds(double seriousUnderstaffing, double understaffing, double overstaffing)
		{
			return new StaffingThresholds(new Percent(seriousUnderstaffing), new Percent(understaffing), new Percent(overstaffing));
		}

		private static IActivity createActivity()
		{
			var activity = ActivityFactory.CreateActivity("activity1");
			activity.RequiresSkill = true;
			return activity;
		}

		private void createAssignment(IPerson person, params IActivity[] activities)
		{
			var startDate = Now.UtcDateTime().Date.AddHours(8);
			var endDate = Now.UtcDateTime().Date.AddHours(17);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				Scenario.Current(), new DateTimePeriod(startDate, endDate),
				ShiftCategoryFactory.CreateShiftCategory(), activities);
			PersonAssignmentRepository.Has(assignment);
		}

		private DateOnlyPeriod getAvailablePeriod()
		{
			var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(ToggleManager);
			var today = Now.ServerDate_DontUse();
			var period = new DateOnlyPeriod(today, today.AddDays(staffingInfoAvailableDays)).Inflate(1);
			return period;
		}

		private IEnumerable<PeriodStaffingPossibilityViewModel> getPossibilityViewModels(DateOnly? date,
			StaffingPossiblityType staffingPossiblityType = StaffingPossiblityType.None,
			bool returnOneWeekData = true)
		{
			return Target.GetPossibilityViewModels(date, staffingPossiblityType, returnOneWeekData).Where(view => intervals.Contains(view.StartTime.TimeOfDay));
		}

		private void setupIntradayStaffingForSkill(ISkill skill, double?[] forecastedStaffings,
			double?[] scheduledStaffings)
		{
			var period = getAvailablePeriod();
			period.DayCollection().ToList().ForEach(day =>
			{
				var utcDate = TimeZoneHelper.ConvertToUtc(day.Date, User.CurrentUser().PermissionInformation.DefaultTimeZone());
				var staffingPeriodData1 = new StaffingPeriodData
				{
					ForecastedStaffing = forecastedStaffings[0].Value,
					ScheduledStaffing = scheduledStaffings[0].Value,
					Period = new DateTimePeriod(utcDate.Date.Add(intervals[0]), utcDate.Date.Add(intervals[0]).AddMinutes(15))
				};
				var staffingPeriodData2 = new StaffingPeriodData
				{
					ForecastedStaffing = forecastedStaffings[1].Value,
					ScheduledStaffing = scheduledStaffings[1].Value,
					Period = new DateTimePeriod(utcDate.Date.Add(intervals[1]), utcDate.Date.Add(intervals[1]).AddMinutes(15))
				};
				SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(skill, new DateOnly(utcDate),
					new[] {staffingPeriodData1, staffingPeriodData2}, User.CurrentUser().PermissionInformation.DefaultTimeZone());
			});
		}
	}
}
