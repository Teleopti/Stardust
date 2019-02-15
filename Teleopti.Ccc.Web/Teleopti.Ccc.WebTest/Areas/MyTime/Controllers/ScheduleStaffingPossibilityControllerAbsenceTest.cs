using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
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

		[Test]
		public void ShouldNotReturnPossibilitiesForDaysNotInAbsenceOpenPeriod()
		{
			setupSiteOpenHour();
			setupTestData();
			setupWorkFlowControlSet();

			var today = Now.UtcDateTime().ToDateOnly();
			var absenceRequestOpenDatePeriod =
				(AbsenceRequestOpenDatePeriod)User.CurrentUser().WorkflowControlSet.AbsenceRequestOpenPeriods[0];

			absenceRequestOpenDatePeriod.Period = new DateOnlyPeriod(today.AddDays(6), today.AddDays(7));
			absenceRequestOpenDatePeriod.OpenForRequestsPeriod = new DateOnlyPeriod(today.AddDays(1), today.AddDays(7));

			var result = getPossibilityViewModels(today, StaffingPossiblityType.Absence).ToList();
			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotReturnAbsencePossibilitiesIfSkillIsNotScheduled()
		{
			var underStaffedSkill = createSkill("skill understaffed");
			underStaffedSkill.StaffingThresholds = new StaffingThresholds(new Percent(-0.95), new Percent(-0.95), new Percent(0.1));
			setupTestDataWithoutSchedule(underStaffedSkill, new double?[] { 100, 100 }, new double?[] { 4, 4 });

			setupWorkFlowControlSet();

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.UtcDateTime().ToDateOnly().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(0, possibilities.Count);
		}

		[Test]
		public void ShouldReturnPossibilitiesForCurrentWeek()
		{
			setupSiteOpenHour();
			setupTestData();
			setupWorkFlowControlSet();

			var result = getPossibilityViewModels(null, StaffingPossiblityType.Absence).ToList();
			result.Count.Should().Be.EqualTo(2);

			var dayCollection = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly(), Now.UtcDateTime().ToDateOnly().AddDays(2)).DayCollection().ToList();

			Assert.AreEqual(2, result.Count(d => d.Date == dayCollection[0].ToFixedClientDateOnlyFormat()));
			Assert.AreEqual(0, result.Count(d => d.Date == dayCollection[1].ToFixedClientDateOnlyFormat()));
			Assert.AreEqual(0, result.Count(d => d.Date == dayCollection[2].ToFixedClientDateOnlyFormat()));
		}

		[Test]
		public void ShouldReturnPossibilitiesForNextWeek()
		{
			setupSiteOpenHour();
			setupWorkFlowControlSet();

			var skill = createSkill("test1");
			var activity = createActivity();
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personSkill);

			var timezone = User.CurrentUser().PermissionInformation.DefaultTimeZone();
			var dayInNextWeek = Now.UtcDateTime().ToDateOnly().AddWeeks(1);
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(dayInNextWeek.Date.AddHours(8), timezone), TimeZoneHelper.ConvertToUtc(dayInNextWeek.Date.AddHours(17), timezone));

			createAssignment(User.CurrentUser(), period, activity);

			setupIntradayStaffingForSkill(skill, new double?[] { 10d, 9.5d }, new double?[] { 10d, 9.5d });

			var result = getPossibilityViewModels(dayInNextWeek, StaffingPossiblityType.Absence, false).ToList();

			result.Count.Should().Be.EqualTo(2);
			result[0].Date.Should().Be.EqualTo(dayInNextWeek.ToFixedClientDateOnlyFormat());
			result[1].Date.Should().Be.EqualTo(dayInNextWeek.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldNotReturnPossibilitiesForPastDays()
		{
			setupSiteOpenHour();
			setupTestData();
			setupWorkFlowControlSet();
			var result = getPossibilityViewModels(Now.UtcDateTime().ToDateOnly().AddDays(-1), StaffingPossiblityType.Absence, false).ToList();
			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotReturnPossibilitiesForFarFutureDays()
		{
			setupSiteOpenHour();
			setupTestData();
			setupWorkFlowControlSet();
			var result = getPossibilityViewModels(Now.UtcDateTime().ToDateOnly().AddWeeks(8), StaffingPossiblityType.Absence);
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
				Period = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly().AddDays(-20), Now.UtcDateTime().ToDateOnly().AddDays(20)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly().AddDays(-20), Now.UtcDateTime().ToDateOnly().AddDays(20)),
				StaffingThresholdValidator = new StaffingThresholdWithShrinkageValidator()
			});
			person.WorkflowControlSet = workflowControlSet;

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.UtcDateTime().ToDateOnly().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
		}

		[Test]
		public void ShouldGetProperPossibilitiesWhenUsingBothIntradayAndIntradayWithShrinkageValidators()
		{
			setupSiteOpenHour();
			var skill = createSkill("test1");
			var activity = createActivity();
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personSkill);

			var timezone = User.CurrentUser().PermissionInformation.DefaultTimeZone();
			var dayInRollingPeriod = Now.UtcDateTime().ToDateOnly().AddDays(19);
			var dayInShrinkagePeriod = Now.UtcDateTime().ToDateOnly().AddDays(22);
			var period1 = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(dayInRollingPeriod.Date.AddHours(8), timezone), TimeZoneHelper.ConvertToUtc(dayInRollingPeriod.Date.AddHours(17), timezone));
			var period2 = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(dayInShrinkagePeriod.Date.AddHours(8), timezone), TimeZoneHelper.ConvertToUtc(dayInShrinkagePeriod.Date.AddHours(17), timezone));

			createAssignment(User.CurrentUser(), period1, activity);
			createAssignment(User.CurrentUser(), period2, activity);

			setupIntradayStaffingForSkill(skill, new double?[] { 10d, 9.5d }, new double?[] { 10d, 9.5d });

			var rollingPeriod = new AbsenceRequestOpenRollingPeriod
			{
				OrderIndex = 0,
				BetweenDays = new MinMax<int>(0, 20),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly().AddDays(-20), Now.UtcDateTime().ToDateOnly().AddDays(40)),
				StaffingThresholdValidator = new StaffingThresholdValidator()
			};

			var intradayWithShkinagePeriod = new AbsenceRequestOpenDatePeriod
			{
				OrderIndex = 1,
				Period = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly().AddDays(21), Now.UtcDateTime().ToDateOnly().AddDays(40)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly().AddDays(-20), Now.UtcDateTime().ToDateOnly().AddDays(40)),
				StaffingThresholdValidator = new StaffingThresholdWithShrinkageValidator()
			};

			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenAbsenceRequestPeriod(rollingPeriod);
			workFlowControlSet.AddOpenAbsenceRequestPeriod(intradayWithShkinagePeriod);
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var possibilitiesIntraday = getPossibilityViewModels(dayInRollingPeriod, StaffingPossiblityType.Absence)
				.Where(x => x.Date == dayInRollingPeriod.ToFixedClientDateOnlyFormat()).ToList();

			var possibilitiesIntradayWithShrinkage = getPossibilityViewModels(dayInShrinkagePeriod, StaffingPossiblityType.Absence)
				.Where(x => x.Date == dayInShrinkagePeriod.ToFixedClientDateOnlyFormat()).ToList();

			Assert.AreEqual(2, possibilitiesIntraday.Count);
			Assert.AreEqual(1, possibilitiesIntraday[0].Possibility);
			Assert.AreEqual(0, possibilitiesIntraday[1].Possibility);

			Assert.AreEqual(2, possibilitiesIntradayWithShrinkage.Count);
			Assert.AreEqual(0, possibilitiesIntradayWithShrinkage[0].Possibility);
			Assert.AreEqual(0, possibilitiesIntradayWithShrinkage[1].Possibility);
		}

		[Test]
		public void ShouldGetPossibilitiesWhenSomeStaffingDataIsNotAvailable()
		{
			setupSiteOpenHour();

			var activity = createActivity();
			var skill = createSkill("test1");
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personSkill);
			createAssignment(User.CurrentUser(), null, activity);
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
					new[] { staffingPeriodData1, staffingPeriodData2 }, User.CurrentUser().PermissionInformation.DefaultTimeZone());
			});

			setupWorkFlowControlSet();
			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.UtcDateTime().ToDateOnly().ToFixedClientDateOnlyFormat())
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
				.Where(d => d.Date == Now.UtcDateTime().ToDateOnly().ToFixedClientDateOnlyFormat()).ToList();
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
				.Where(d => d.Date == Now.UtcDateTime().ToDateOnly().ToFixedClientDateOnlyFormat()).ToList();
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
				.Where(d => d.Date == Now.UtcDateTime().ToDateOnly().ToFixedClientDateOnlyFormat()).ToList();
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

			createAssignment(person, null, activity1, activity2);
			setupWorkFlowControlSet();
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Absence)
				.Where(d => d.Date == Now.UtcDateTime().ToDateOnly().ToFixedClientDateOnlyFormat()).ToList();
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
		public void ShouldGetPossibilitiesAccordingToAgentTimeZone()
		{
			var timeZoneInfo = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(timeZoneInfo);

			setupSiteOpenHour();
			setupTestData(timezone: timeZoneInfo);
			setupWorkFlowControlSet();

			var today = new DateOnly(TimeZoneHelper.ConvertFromUtc(Now.UtcDateTime(),
				User.CurrentUser().PermissionInformation.DefaultTimeZone()));

			var result = getPossibilityViewModels(today, StaffingPossiblityType.Absence).ToList();
			result.Count.Should().Be.EqualTo(2);
			result.FirstOrDefault()?.Date.Should().Be(today.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldSubtractCurrentUsersMainShiftWhenCalculatingAbsenceProbability()
		{
			setupTestData(new double?[] { 1, 1 }, new double?[] { 1, 1 });
			setupWorkFlowControlSet();

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.UtcDateTime().ToDateOnly().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldSubtractCurrentUsersOvertimeShiftWhenCalculatingAbsenceProbability()
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
					.Where(d => d.Date == Now.UtcDateTime().ToDateOnly().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldSubtractCurrentUserShiftWhenSceduledStaffingLagerThanOne()
		{
			var underStaffedSkill = createSkill("skill understaffed");
			underStaffedSkill.StaffingThresholds =
				new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0.1));

			setupTestData(new double?[] { 5, 5 }, new double?[] { 5, 5 });

			setupWorkFlowControlSet();

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence, false).ToList();
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
			createAssignment(User.CurrentUser(), null, activity);
			var primaryPersonSkill = createPersonSkill(activity, primarySkill);
			var nonPrimaryPersonSkill = createPersonSkill(activity, nonPrimarySkill);

			addPersonSkillsToPersonPeriod(primaryPersonSkill, nonPrimaryPersonSkill);

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
					.Where(d => d.Date == Now.UtcDateTime().ToDateOnly().ToFixedClientDateOnlyFormat())
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
			createAssignment(User.CurrentUser(), null, activity);
			var primaryPersonSkill = createPersonSkill(activity, primarySkill);
			var nonPrimaryPersonSkill = createPersonSkill(activity, nonPrimarySkill);

			addPersonSkillsToPersonPeriod(primaryPersonSkill, nonPrimaryPersonSkill);

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.UtcDateTime().ToDateOnly().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetAllFairPossibilitiesWhenStaffingLevelIsLowerThanUnderStaffingThresholds()
		{
			var skill = createSkill("test1");
			skill.StaffingThresholds = new StaffingThresholds(new Percent(-0.95), new Percent(-0.95), new Percent(0.1));
			setupTestData(skill, new double?[] { 1, 1 }, new double?[] { 0.1, 0.2 });
			setupWorkFlowControlSet();

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.UtcDateTime().ToDateOnly().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetAllGoodPossibilitiesWhenStaffingLevelIsHigherThanUnderStaffingThresholds()
		{
			var skill = createSkill("test1");
			skill.StaffingThresholds = new StaffingThresholds(new Percent(-0.95), new Percent(-0.95), new Percent(0.1));
			setupTestData(skill, new double?[] { 100, 100 }, new double?[] { 20, 20 });
			setupWorkFlowControlSet();

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.UtcDateTime().ToDateOnly().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetAllGoodPossibilitiesWhenStaffingLevelIsEqualToUnderStaffingThresholds()
		{
			var skill = createSkill("test1");
			skill.StaffingThresholds = new StaffingThresholds(new Percent(-0.96), new Percent(-0.96), new Percent(0.1));
			setupTestData(skill, new double?[] { 100, 100 }, new double?[] { 5, 5 });
			setupWorkFlowControlSet();

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.UtcDateTime().ToDateOnly().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldNotRoundStaffingDataForAbsenceProbability()
		{
			var underStaffedSkill = createSkill("skill understaffed");
			underStaffedSkill.StaffingThresholds =
				new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0.1));

			setupTestData(new double?[] { 1.03, 1.03 }, new double?[] { 1, 1 });

			setupWorkFlowControlSet();

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence, false).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		private void setupWorkFlowControlSet()
		{
			var absenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly().AddDays(-20), Now.UtcDateTime().ToDateOnly().AddDays(20)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly().AddDays(-20), Now.UtcDateTime().ToDateOnly().AddDays(20)),
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
				(PersonPeriod)User.CurrentUser().PersonPeriods(Now.UtcDateTime().ToDateOnly().ToDateOnlyPeriod()).FirstOrDefault();
			if (personPeriod != null) return personPeriod;
			var team = TeamFactory.CreateTeam("team1", "site1");
			personPeriod =
				(PersonPeriod)
				PersonPeriodFactory.CreatePersonPeriod(Now.UtcDateTime().ToDateOnly(), PersonContractFactory.CreatePersonContract(), team);
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

		private void setupTestData(double?[] forecastedStaffing = null, double?[] scheduledStaffing = null, TimeZoneInfo timezone = null)
		{
			var skill = createSkill("test1");
			setupTestData(skill, forecastedStaffing, scheduledStaffing, timezone);
		}

		private void setupTestData(ISkill skill, double?[] forecastedStaffing = null, double?[] scheduledStaffing = null, TimeZoneInfo timezone = null)
		{
			var activity = createActivity();
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personSkill);
			createAssignment(User.CurrentUser(), timezone, activity);
			forecastedStaffing = forecastedStaffing ?? new Double?[] { 10d, 10d };
			scheduledStaffing = scheduledStaffing ?? new Double?[] { 8d, 8d };
			setupIntradayStaffingForSkill(skill, forecastedStaffing, scheduledStaffing);
		}

		private void setupTestDataWithoutSchedule(ISkill skill, double?[] forecastedStaffing = null, double?[] scheduledStaffing = null)
		{
			var activity = createActivity();
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personSkill);
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

		private static IActivity createActivity()
		{
			var activity = ActivityFactory.CreateActivity("activity1");
			activity.RequiresSkill = true;
			return activity;
		}

		private void createAssignment(IPerson person, TimeZoneInfo timezone = null, params IActivity[] activities)
		{
			timezone = timezone ?? User.CurrentUser().PermissionInformation.DefaultTimeZone();
			var startDate = TimeZoneHelper.ConvertToUtc(Now.UtcDateTime().Date.AddHours(8), timezone);
			var endDate = TimeZoneHelper.ConvertToUtc(Now.UtcDateTime().Date.AddHours(17), timezone);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				Scenario.Current(), new DateTimePeriod(startDate, endDate),
				ShiftCategoryFactory.CreateShiftCategory(), activities);
			PersonAssignmentRepository.Has(assignment);
		}

		private void createAssignment(IPerson person, DateTimePeriod period, params IActivity[] activities)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				Scenario.Current(), period,
				ShiftCategoryFactory.CreateShiftCategory(), activities);
			PersonAssignmentRepository.Has(assignment);
		}

		private DateOnlyPeriod getAvailablePeriod()
		{
			var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(ToggleManager);
			var today = Now.UtcDateTime();
			var period = new DateOnlyPeriod(today, today.AddDays(staffingInfoAvailableDays)).Inflate(1);
			return period;
		}

		private IEnumerable<PeriodStaffingPossibilityViewModel> getPossibilityViewModels(DateOnly? date,
			StaffingPossiblityType staffingPossiblityType = StaffingPossiblityType.None,
			bool returnOneWeekData = true)
		{
			var result = Target.GetPossibilityViewModels(date, staffingPossiblityType, returnOneWeekData);
			return result.Where(view => intervals.Contains(view.StartTime.TimeOfDay));
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
					new[] { staffingPeriodData1, staffingPeriodData2 }, User.CurrentUser().PermissionInformation.DefaultTimeZone());
			});
		}
	}
}
