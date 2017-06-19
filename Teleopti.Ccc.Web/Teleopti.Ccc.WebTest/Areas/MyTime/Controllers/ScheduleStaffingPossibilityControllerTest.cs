﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
	[MyTimeWebTest]
	public class ScheduleStaffingPossibilityControllerTest : ISetup
	{
		public ScheduleStaffingPossibilityController Target;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public FakeScheduleDataReadScheduleStorage ScheduleData;
		public MutableNow Now;
		public FakeUserCulture Culture;
		public FakeUserTimeZone TimeZone;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeSkillCombinationResourceRepository CombinationRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;

		private readonly TimeSpan[] intervals = { TimeSpan.FromMinutes(495), TimeSpan.FromMinutes(510) };

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
			system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			system.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			system.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();
		}

		[Test, SetCulture("en-US")]
		public void ShouldNotReturnAbsencePossibiliesForDaysNotInAbsenceOpenPeriod()
		{
			setupSiteOpenHour();
			setupDefaultTestData();
			setupWorkFlowControlSet();

			var today = Now.ServerDate_DontUse();
			var absenceRequestOpenDatePeriod =
				(AbsenceRequestOpenDatePeriod)User.CurrentUser().WorkflowControlSet.AbsenceRequestOpenPeriods[0];

			absenceRequestOpenDatePeriod.Period = new DateOnlyPeriod(today.AddDays(6), today.AddDays(7));
			absenceRequestOpenDatePeriod.OpenForRequestsPeriod = new DateOnlyPeriod(today, today.AddDays(7));

			var result = getIntradayAbsencePossibility(today.AddDays(7), StaffingPossiblityType.Absence).ToList();
			result.Count.Should().Be.EqualTo(4);
		}

		[Test, SetCulture("en-US")]
		public void ShouldReturnPossibiliesForCurrentWeek()
		{
			setupSiteOpenHour();
			setupDefaultTestData();
			setupWorkFlowControlSet();
			var result = getIntradayAbsencePossibility(null, StaffingPossiblityType.Absence).ToList();
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
			setupDefaultTestData();
			setupWorkFlowControlSet();
			var result = getIntradayAbsencePossibility(Now.ServerDate_DontUse().AddWeeks(1), StaffingPossiblityType.Absence).ToList();
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
			setupDefaultTestData();
			setupWorkFlowControlSet();
			var result = getIntradayAbsencePossibility(Now.ServerDate_DontUse().AddDays(-1), StaffingPossiblityType.Absence).ToList();
			result.Count.Should().Be.EqualTo(6);
		}

		[Test, SetCulture("en-US")]
		public void ShouldNotReturnPossibiliesForFarFutureDays()
		{
			setupSiteOpenHour();
			setupDefaultTestData();
			setupWorkFlowControlSet();
			var result = getIntradayAbsencePossibility(Now.ServerDate_DontUse().AddWeeks(3), StaffingPossiblityType.Absence);
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnEmptyPossibilities()
		{
			var possibilities = getIntradayAbsencePossibility(null).ToList();
			Assert.AreEqual(0, possibilities.Count);
		}

		[Test]
		public void ShouldGetPossibilitiesOnlyForSkillsInSchedule()
		{
			var person = User.CurrentUser();
			var activity = createActivity();
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity, skill1);
			setupIntradayStaffingForSkill(skill1, new double?[] { 10d, 10d }, new double?[] { 12d, 11d });

			var activity2 = createActivity();
			var skill2 = createSkill("skill2");
			var personSkill2 = createPersonSkill(activity2, skill2);
			setupIntradayStaffingForSkill(skill2, new double?[] { 10d, 10d }, new double?[] { 6d, 12d });

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			createAssignment(person, activity2);

			var possibilities =
				getIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetPossibilitiesWhenUsingShrinkageValidator()
		{
			setupSiteOpenHour();
			setupDefaultTestData();
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
				getIntradayAbsencePossibility(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
		}

		[Test]
		public void ShouldGetPossibilitiesWhenSomeStaffingDataIsNotAvailable()
		{
			setupSiteOpenHour();
			setupDefaultTestData(new double?[] { 10d }, new double?[] { 11d, 11d });
			setupWorkFlowControlSet();
			var possibilities =
				getIntradayAbsencePossibility(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
		}

		[Test]
		public void ShouldGetPossibilitiesWithoutIntradaySchedule()
		{
			setupSiteOpenHour();
			setupDefaultTestData();
			setupWorkFlowControlSet();
			ScheduleData.Clear();
			var possibilities = getIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
		}


		[Test]
		public void ShouldNotGetPossibilitiesForNotSupportedSkill()
		{
			setupSiteOpenHour();
			setupDefaultTestData();
			setupWorkFlowControlSet();
			SkillRepository.LoadAllSkills().First().SkillType.Description = new Description("NotSupportedSkillType");
			var possibilities = getIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime).ToList();
			Assert.AreEqual(0, possibilities.Count);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForAbsenceWhenUnderstaffing()
		{
			setupSiteOpenHour();
			setupDefaultTestData();
			setupWorkFlowControlSet();
			var possibilities = getIntradayAbsencePossibility(null, StaffingPossiblityType.Absence)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetGoodPossibilitiesForAbsenceWhenNotUnderstaffing()
		{
			setupSiteOpenHour();
			setupDefaultTestData(new double?[] { 10d, 10d }, new double?[] { 11d, 11d });
			setupWorkFlowControlSet();
			var possibilities = getIntradayAbsencePossibility(null, StaffingPossiblityType.Absence)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairAndGoodPossibilitiesForAbsence()
		{
			setupSiteOpenHour();
			setupDefaultTestData(new double?[] { 10d, 10d }, new double?[] { 8d, 11d });
			setupWorkFlowControlSet();
			var possibilities = getIntradayAbsencePossibility(null, StaffingPossiblityType.Absence)
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
			var possibilities = getIntradayAbsencePossibility(null, StaffingPossiblityType.Absence)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForOvertimeWhenOverstaffing()
		{
			setupSiteOpenHour();
			setupDefaultTestData(new double?[] { 10d, 10d }, new double?[] { 12d, 12d });
			var possibilities = getIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetGoodPossibilitiesForOvertimeWhenNotOverstaffing()
		{
			setupSiteOpenHour();
			setupDefaultTestData(new double?[] { 10d, 10d }, new double?[] { 6d, 6d });
			var possibilities = getIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairAndGoodPossibilitiesForOvertime()
		{
			setupSiteOpenHour();
			setupDefaultTestData(new double?[] { 10d, 10d }, new double?[] { 7d, 6d });
			setupWorkFlowControlSet();
			var possibilities = getIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForOvertimeWhenOneOfSkillIsNotCriticalUnderStaffing()
		{
			var person = User.CurrentUser();
			var activity1 = createActivity();
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, new double?[] { 10d, 10d }, new double?[] { 10d, 10d });

			var activity2 = createActivity();
			var skill2 = createSkill("skill2");
			var personSkill2 = createPersonSkill(activity2, skill2);
			setupIntradayStaffingForSkill(skill2, new double?[] { 10d, 10d }, new double?[] { 5d, 5d });

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			createAssignment(person, activity1, activity2);

			var possibilities = getIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetPossibilitiesForOvertimeWithDayOff()
		{
			setupSiteOpenHour();
			setupDefaultTestData();
			ScheduleData.Clear();
			var person = User.CurrentUser();
			var assignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, Scenario.Current(),
				Now.ServerDate_DontUse(), new DayOffTemplate());
			ScheduleData.Set(new List<IScheduleData> { assignment });
			var possibilities = getIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
		}

		[Test]
		public void ShouldOnlyGetOneDayDataIfReturnOneWeekDataIsFalse()
		{
			setupSiteOpenHour();
			setupDefaultTestData();
			setupWorkFlowControlSet();
			var result = getIntradayAbsencePossibility(null, StaffingPossiblityType.Absence, false).ToList();
			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldGetPossibilitiesAccordingToAgentTimeZone()
		{
			Now.Is(Now.UtcDateTime().Date.AddHours(2));

			setupSiteOpenHour();
			setupDefaultTestData();
			setupWorkFlowControlSet();

			var timeZoneInfo = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(timeZoneInfo);

			var result = getIntradayAbsencePossibility(null, StaffingPossiblityType.Absence).ToList();
			var today = new DateOnly(TimeZoneHelper.ConvertFromUtc(Now.UtcDateTime(),
				User.CurrentUser().PermissionInformation.DefaultTimeZone()));
			result.FirstOrDefault()?.Date.Should().Be(today.ToFixedClientDateOnlyFormat());
			result.Count.Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldSubstractCurrentUsersScheduleWhenCalculatingProbability()
		{
			setupDefaultTestData(new double?[] { 1, 1 }, new double?[] { 1, 1 });
			setupWorkFlowControlSet();

			var possibilities =
				getIntradayAbsencePossibility(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_CalculateOvertimeProbabilityByPrimarySkill_44686)]
		public void ShouldUsePrimarySkillsWhenCalculatingOvertimeProbability()
		{
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
				getIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_CalculateOvertimeProbabilityByPrimarySkill_44686)]
		public void ShouldGetFairOvertimePossibilitiesWhenAllSkillsArePrimarySkillWithOneSkillCriticalUnderStaffing()
		{
			var primarySkill = createSkill("primarySkill1");
			primarySkill.SetCascadingIndex(1);
			setupIntradayStaffingForSkill(primarySkill, new double?[] { 5, 5 }, new double?[] { 0, 0 });

			var primarySkill2 = createSkill("PrimarySkill2");
			primarySkill2.SetCascadingIndex(1);
			setupIntradayStaffingForSkill(primarySkill2, new double?[] { 5, 5 }, new double?[] { 4, 4 });

			var activity = createActivity();
			createAssignment(User.CurrentUser(), activity);
			var primaryPersonSkill = createPersonSkill(activity, primarySkill);
			var nonPrimaryPersonSkill = createPersonSkill(activity, primarySkill2);

			addPersonSkillsToPersonPeriod(primaryPersonSkill, nonPrimaryPersonSkill);

			var possibilities =
				getIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}


		[Test]
		public void ShouldNotUsePrimarySkillsWhenCalculatingOvertimeProbabilityToggle44686Off()
		{
			var primarySkill = createSkill("primarySkill");
			primarySkill.SetCascadingIndex(1);
			setupIntradayStaffingForSkill(primarySkill, new double?[] { 5, 5 }, new double?[] { 0, 0 });

			var nonPrimarySkill = createSkill("nonPrimarySkill");
			setupIntradayStaffingForSkill(nonPrimarySkill, new double?[] { 5, 5 }, new double?[] { 4, 4 });

			var activity = createActivity();
			createAssignment(User.CurrentUser(), activity);
			var primaryPersonSkill = createPersonSkill(activity, primarySkill);
			var nonPrimaryPersonSkill = createPersonSkill(activity, nonPrimarySkill);

			addPersonSkillsToPersonPeriod(primaryPersonSkill, nonPrimaryPersonSkill);

			var possibilities =
				getIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_CalculateOvertimeProbabilityByPrimarySkill_44686)]
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
				getIntradayAbsencePossibility(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
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
			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenDatePeriod);
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

		private void setupDefaultTestData(double?[] forecastedStaffing = null, double?[] scheduledStaffing = null)
		{
			var activity = createActivity();
			var skill = createSkill("test1");
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personSkill);
			createAssignment(User.CurrentUser(), activity);
			setupIntradayStaffingForSkill(skill, forecastedStaffing ?? new double?[] { 10d, 10d },
				scheduledStaffing ?? new double?[] { 8d, 8d });
		}

		private void addPersonSkillsToPersonPeriod(params IPersonSkill[] personSkills)
		{
			var personPeriod = getOrAddPersonPeriod();
			foreach (var personSkill in personSkills)
			{
				personPeriod.AddPersonSkill(personSkill);
			}
		}

		private void setupIntradayStaffingForSkill(ISkill skill, double?[] forecastedStaffings,
			double?[] scheduledStaffings)
		{
			var period = getAvailablePeriod();
			period.DayCollection().ToList().ForEach(day =>
			{
				var utcDate = TimeZoneHelper.ConvertToUtc(day.Date,
					User.CurrentUser().PermissionInformation.DefaultTimeZone());
				
				for (var i = 0; i < scheduledStaffings.Length; i++)
				{
					CombinationRepository.AddSkillCombinationResource(new DateTime(),
						new[]
						{
							new SkillCombinationResource
							{
								StartDateTime = utcDate.Add(intervals[i]),
								EndDateTime = utcDate.Add(intervals[i]).AddMinutes(15),
								Resource = scheduledStaffings[i].Value,
								SkillCombination = new[] {skill.Id.Value}
							}
						});
				}
				
				var timePeriodTuples = new List<Tuple<TimePeriod, double>>();
				for (var i = 0; i < forecastedStaffings.Length; i++)
				{
					timePeriodTuples.Add(new Tuple<TimePeriod, double>(
						new TimePeriod(intervals[i], intervals[i].Add(TimeSpan.FromMinutes(15))),
						forecastedStaffings[i].Value));
				}
				SkillDayRepository.Has(skill.CreateSkillDayWithDemandOnInterval(Scenario.Current(), day, 0, timePeriodTuples.ToArray()));
			});
		}


		private IPersonSkill createPersonSkill(IActivity activity, ISkill skill)
		{
			skill.Activity = activity;
			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			return personSkill;
		}

		private ISkill createSkill(string name)
		{
			var skill = SkillFactory.CreateSkill(name).WithId();
			skill.SkillType.Description = new Description("SkillTypeInboundTelephony");
			skill.StaffingThresholds = createStaffingThresholds();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(8, 00, 9, 30));
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

		private void createAssignment(IPerson person, params IActivity[] activities)
		{
			var startDate = Now.UtcDateTime().Date.AddHours(8);
			var endDate = Now.UtcDateTime().Date.AddHours(17);
			var scheduleDatas = new List<IScheduleData>();
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				Scenario.Current(), new DateTimePeriod(startDate, endDate),
				ShiftCategoryFactory.CreateShiftCategory(), activities);
			scheduleDatas.Add(assignment);
			ScheduleData.Set(scheduleDatas);
		}

		private DateOnlyPeriod getAvailablePeriod()
		{
			var today = Now.ServerDate_DontUse();
			var period = new DateOnlyPeriod(today, today.AddDays(13)).Inflate(1);
			return period;
		}

		private IEnumerable<PeriodStaffingPossibilityViewModel> getIntradayAbsencePossibility(DateOnly? date,
			StaffingPossiblityType staffingPossiblityType = StaffingPossiblityType.None,
			bool returnOneWeekData = true)
		{
			return Target.GetIntradayAbsencePossibility(date, staffingPossiblityType, returnOneWeekData).Where(view => intervals.Contains(view.StartTime.TimeOfDay));
		}
	}
}
