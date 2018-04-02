using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
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
		public IScheduleStorage ScheduleData;
		public MutableNow Now;
		public FakeUserCulture Culture;
		public FakeUserTimeZone TimeZone;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeSkillCombinationResourceRepository CombinationRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillTypeRepository SkillTypeRepository;
		public FakeToggleManager ToggleManager;

		private readonly TimeSpan[] intervals = { TimeSpan.FromMinutes(495), TimeSpan.FromMinutes(510) };

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
			system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			system.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			system.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();
			system.UseTestDouble<FakeSkillTypeRepository>().For<ISkillTypeRepository>();
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

			var result = getPossibilityViewModels(today.AddDays(7), StaffingPossiblityType.Absence).ToList();
			result.Count.Should().Be.EqualTo(4);
		}

		[Test, SetCulture("en-US")]
		public void ShouldReturnPossibiliesForCurrentWeek()
		{
			setupSiteOpenHour();
			setupDefaultTestData();
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
			setupDefaultTestData();
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
		public void ShouldReturnPossibiliesForDaysLongerThan14()
		{
			ToggleManager.Enable(Toggles.Wfm_Staffing_StaffingReadModel28DaysStep1_45109);

			setupSiteOpenHour();
			setupDefaultTestData();
			setupWorkFlowControlSet();

			var workflowControlSet = User.CurrentUser().WorkflowControlSet;
			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
				.WithId();
			SkillTypeRepository.Add(phoneSkillType);

			var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(ToggleManager);
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod()
			{
				BetweenDays = new MinMax<int>(0, staffingInfoAvailableDays),
				SkillType = phoneSkillType
			});

			var result = getPossibilityViewModels(Now.ServerDate_DontUse().AddWeeks(3), StaffingPossiblityType.Overtime).ToList();
			result.Count.Should().Be.EqualTo(14);
			DateHelper.GetWeekPeriod(Now.ServerDate_DontUse().AddWeeks(3), CultureInfo.CurrentCulture)
				.DayCollection()
				.ToList()
				.ForEach(day =>
				{
					Assert.AreEqual(2, result.Count(d => d.Date == day.ToFixedClientDateOnlyFormat()), day.ToShortDateString());
				});
		}

		[Test, SetCulture("en-US")]
		public void ShouldReturnPossibiliesForDaysLongerThan28()
		{
			ToggleManager.Enable(Toggles.Wfm_Staffing_StaffingReadModel49DaysStep2_45109);

			setupSiteOpenHour();
			setupDefaultTestData();
			setupWorkFlowControlSet();

			var workflowControlSet = User.CurrentUser().WorkflowControlSet;
			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
				.WithId();
			SkillTypeRepository.Add(phoneSkillType);

			var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(ToggleManager);
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod()
			{
				BetweenDays = new MinMax<int>(0, staffingInfoAvailableDays),
				SkillType = phoneSkillType
			});

			var result = getPossibilityViewModels(Now.ServerDate_DontUse().AddWeeks(4), StaffingPossiblityType.Overtime).ToList();
			result.Count.Should().Be.EqualTo(14);
			DateHelper.GetWeekPeriod(Now.ServerDate_DontUse().AddWeeks(4), CultureInfo.CurrentCulture)
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
			var result = getPossibilityViewModels(Now.ServerDate_DontUse().AddDays(-1), StaffingPossiblityType.Absence).ToList();
			result.Count.Should().Be.EqualTo(6);
		}

		[Test, SetCulture("en-US")]
		public void ShouldNotReturnPossibiliesForFarFutureDays()
		{
			setupSiteOpenHour();
			setupDefaultTestData();
			setupWorkFlowControlSet();
			var result = getPossibilityViewModels(Now.ServerDate_DontUse().AddWeeks(3), StaffingPossiblityType.Absence);
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
				setupIntradayStaffingForSkill(skill, new DateOnly(utcDate), new[] { staffingPeriodData1, staffingPeriodData2 });
			});

			setupWorkFlowControlSet();
			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Absence)
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
			PersonAssignmentRepository.Clear();
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
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
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime).ToList();
			Assert.AreEqual(0, possibilities.Count);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForAbsenceWhenUnderstaffing()
		{
			setupSiteOpenHour();
			setupDefaultTestData();
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
			setupDefaultTestData(new double?[] { 10d, 10d }, new double?[] { 11d, 11d });
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
			setupDefaultTestData(new double?[] { 10d, 10d }, new double?[] { 8d, 11d });
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
		public void ShouldGetFairPossibilitiesForOvertimeWhenOverstaffing()
		{
			setupWorkFlowControlSet();
			setupSiteOpenHour();
			setupDefaultTestData(new double?[] { 10d, 10d }, new double?[] { 22d, 22d });
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		[Toggle(Toggles.OvertimeRequestPeriodSkillTypeSetting_47290)]
		public void ShouldNotGetPossibilitiesForOvertimeWhenOverstaffingAndNoSkillTypeIsMatched()
		{
			setupSiteOpenHour();

			var person = User.CurrentUser();
			var activity1 = createActivity();
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, new double?[] { 10d, 10d }, new double?[] { 22d, 22d });
			addPersonSkillsToPersonPeriod(personSkill1);

			createAssignment(person, activity1);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13))),
				SkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId()
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(0, possibilities.Count);
		}

		[Test]
		[Toggle(Toggles.OvertimeRequestPeriodSkillTypeSetting_47290)]
		public void ShouldGetFairPossibilitiesForOvertimeWhenOverstaffingAndSkillTypeIsMatched()
		{
			setupSiteOpenHour();
			var person = User.CurrentUser();
			var activity1 = createActivity();
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, new double?[] { 10d, 10d }, new double?[] { 22d, 22d });
			addPersonSkillsToPersonPeriod(personSkill1);

			createAssignment(person, activity1);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13))),
				SkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId()
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		[Toggle(Toggles.OvertimeRequestPeriodSkillTypeSetting_47290)]
		public void ShouldGetFairPossibilitiesForOvertimeWhenOverstaffingAndSkillTypeIsNotSet()
		{
			setupSiteOpenHour();
			var person = User.CurrentUser();
			var activity1 = createActivity();
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, new double?[] { 10d, 10d }, new double?[] { 22d, 22d });
			addPersonSkillsToPersonPeriod(personSkill1);

			SkillTypeRepository.Add(new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony));
			createAssignment(person, activity1);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		[Toggle(Toggles.OvertimeRequestPeriodSkillTypeSetting_47290)]
		public void ShouldGetPossibilitiesForOvertimeBasedOnSkillTypeSetInOpenPeriod()
		{
			var emailSkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();
			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();

			var person = User.CurrentUser();
			var activity1 = createActivity();
			var skill1 = createSkill("skill1");
			skill1.SkillType = emailSkillType;
			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, new double?[] { 10d, 10d }, new double?[] { 15d, 15d });

			var activity2 = createActivity();
			var skill2 = createSkill("skill2");
			skill2.SkillType = phoneSkillType;
			var personSkill2 = createPersonSkill(activity2, skill2);
			setupIntradayStaffingForSkill(skill2, new double?[] { 10d, 10d }, new double?[] { 5d, 5d });

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			createAssignment(person, activity1, activity2);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13))),
				SkillType = emailSkillType
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		[Toggle(Toggles.OvertimeRequestPeriodSkillTypeSetting_47290)]
		public void ShouldGetPossibilitiesForOvertimeBasedOnSkillTypeOfEachDaySetInOpenPeriod()
		{
			Now.Is(new DateTime(2018, 2, 1, 8, 0, 0, DateTimeKind.Utc));

			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();
			var chatSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Chat), ForecastSource.Chat).WithId();

			var person = User.CurrentUser();
			var activity1 = createActivity();
			var phoneSkill = createSkill("phoneSkill", new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17)));
			phoneSkill.SkillType = phoneSkillType;
			var personPhoneSkill = createPersonSkill(activity1, phoneSkill);
			setupIntradayStaffingForSkill(phoneSkill, new double?[] { 10d, 10d }, new double?[] { 15d, 15d });

			var activity2 = createActivity();
			var chatSkill = createSkill("chatSkill", new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(15)));
			chatSkill.SkillType = chatSkillType;
			var personChatSkill = createPersonSkill(activity2, chatSkill);
			setupIntradayStaffingForSkill(chatSkill, new double?[] { 10d, 10d }, new double?[] { 5d, 5d });

			addPersonSkillsToPersonPeriod(personPhoneSkill, personChatSkill);

			createAssignment(person, activity1, activity2);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 1),
				SkillType = chatSkillType,
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 2),
				SkillType = phoneSkillType,
				OrderIndex = 2
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var possibilities = Target.GetPossibilityViewModels(new DateOnly(2018, 2, 1), StaffingPossiblityType.Overtime);

			var possibilitiesOn2nd = possibilities.Where(d => d.Date == new DateOnly(2018, 2, 2).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(possibilitiesOn2nd.ElementAt(0).StartTime, new DateTime(2018, 2, 2, 7, 0, 0));
			Assert.AreEqual(0, possibilitiesOn2nd.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilitiesOn2nd.ElementAt(1).Possibility);

			var possibilitiesOn3rd = possibilities.Where(d => d.Date == new DateOnly(2018, 2, 3).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(possibilitiesOn3rd.ElementAt(0).StartTime, new DateTime(2018, 2, 3, 8, 0, 0));
			Assert.AreEqual(0, possibilitiesOn3rd.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilitiesOn3rd.ElementAt(1).Possibility);
		}

		[Test]
		[Toggle(Toggles.OvertimeRequestPeriodSkillTypeSetting_47290)]
		public void ShouldGetPossibilitiesForOvertimeBasedOnSkillTypeOfOneDaySetInOpenPeriod()
		{
			Now.Is(new DateTime(2018, 2, 1, 8, 0, 0, DateTimeKind.Utc));

			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();
			var chatSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Chat), ForecastSource.Chat).WithId();

			var person = User.CurrentUser();
			var activity1 = createActivity();
			var phoneSkill = createSkill("phoneSkill", new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17)));
			phoneSkill.SkillType = phoneSkillType;
			var personPhoneSkill = createPersonSkill(activity1, phoneSkill);
			setupIntradayStaffingForSkill(phoneSkill, new double?[] { 10d, 10d }, new double?[] { 15d, 15d });

			var activity2 = createActivity();
			var chatSkill = createSkill("chatSkill", new TimePeriod(TimeSpan.FromHours(7), TimeSpan.FromHours(15)));
			chatSkill.SkillType = chatSkillType;
			var personChatSkill = createPersonSkill(activity2, chatSkill);
			setupIntradayStaffingForSkill(chatSkill, new double?[] { 10d, 10d }, new double?[] { 5d, 5d });

			addPersonSkillsToPersonPeriod(personPhoneSkill, personChatSkill);

			createAssignment(person, activity1, activity2);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 1),
				SkillType = chatSkillType,
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 2),
				SkillType = phoneSkillType,
				OrderIndex = 2
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var possibilities = Target.GetPossibilityViewModels(new DateOnly(2018, 2, 3), StaffingPossiblityType.Overtime, false);

			var possibilitiesOn3rd = possibilities.Where(d => d.Date == new DateOnly(2018, 2, 3).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(possibilitiesOn3rd.ElementAt(0).StartTime, new DateTime(2018, 2, 3, 8, 0, 0));
			Assert.AreEqual(possibilitiesOn3rd.Last().StartTime, new DateTime(2018, 2, 3, 16, 45, 0));
			Assert.AreEqual(0, possibilitiesOn3rd.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilitiesOn3rd.ElementAt(1).Possibility);
		}

		[Test]
		[Toggle(Toggles.OvertimeRequestPeriodSkillTypeSetting_47290)]
		public void ShouldNotGetPossibilitiesForOvertimeWhenOneOfThePeriodIsDeny()
		{
			Now.Is(new DateTime(2018, 1, 31, 8, 0, 0, DateTimeKind.Utc));

			var person = User.CurrentUser();

			var chatSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Chat), ForecastSource.Chat).WithId();
			var activity = createActivity();
			var chatSkill = createSkill("chatSkill", new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1)));
			chatSkill.SkillType = chatSkillType;
			var personChatSkill = createPersonSkill(activity, chatSkill);
			setupIntradayStaffingForSkill(chatSkill, new double?[] { 10d, 10d }, new double?[] { 5d, 5d });

			addPersonSkillsToPersonPeriod(personChatSkill);

			createAssignment(person, activity);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				BetweenDays = new MinMax<int>(0, 7),
				SkillType = chatSkillType,
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				BetweenDays = new MinMax<int>(1, 2),
				SkillType = chatSkillType,
				OrderIndex = 1
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var possibilities = Target.GetPossibilityViewModels(new DateOnly(2018, 2, 1), StaffingPossiblityType.Overtime);

			var possibilitiesOn1st = possibilities.Where(d => d.Date == new DateOnly(2018, 1, 31).ToFixedClientDateOnlyFormat()).ToList();
			var possibilitiesOn2nd = possibilities.Where(d => d.Date == new DateOnly(2018, 2, 1).ToFixedClientDateOnlyFormat()).ToList();
			var possibilitiesOn3rd = possibilities.Where(d => d.Date == new DateOnly(2018, 2, 2).ToFixedClientDateOnlyFormat()).ToList();

			Assert.AreEqual(possibilitiesOn1st.Count, 96);
			Assert.AreEqual(possibilitiesOn2nd.Count, 0);
			Assert.AreEqual(possibilitiesOn3rd.Count, 0);
		}

		[Test]
		public void ShouldGetGoodPossibilitiesForOvertimeWhenNotOverstaffing()
		{
			setupWorkFlowControlSet();
			setupSiteOpenHour();
			setupDefaultTestData(new double?[] { 10d, 10d }, new double?[] { 6d, 6d });
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairAndGoodPossibilitiesForOvertime()
		{
			setupSiteOpenHour();
			setupDefaultTestData(new double?[] { 10d, 10d }, new double?[] { 16d, 6d });
			setupWorkFlowControlSet();
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForOvertimeWhenOneOfSkillIsNotCriticalUnderStaffing()
		{
			setupWorkFlowControlSet();
			var person = User.CurrentUser();
			var activity1 = createActivity();
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, new double?[] { 10d, 10d }, new double?[] { 15d, 15d });

			var activity2 = createActivity();
			var skill2 = createSkill("skill2");
			var personSkill2 = createPersonSkill(activity2, skill2);
			setupIntradayStaffingForSkill(skill2, new double?[] { 10d, 10d }, new double?[] { 5d, 5d });

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			createAssignment(person, activity1, activity2);

			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		[Toggle(Toggles.OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944)]
		public void ShouldGetGoodPossibilitiesForOvertimeWhenOneOfSkillIsNotCriticalUnderStaffing()
		{
			setupWorkFlowControlSet();
			var person = User.CurrentUser();
			var activity1 = createActivity();
			var skill1 = createSkill("skill1");
			var personSkill1 = createPersonSkill(activity1, skill1);
			setupIntradayStaffingForSkill(skill1, new double?[] { 10d, 10d }, new double?[] { 15d, 15d });

			var activity2 = createActivity();
			var criticalUnderStaffingSkill = createSkill("criticalUnderStaffingSkill");
			var personSkill2 = createPersonSkill(activity2, criticalUnderStaffingSkill);
			setupIntradayStaffingForSkill(criticalUnderStaffingSkill, new double?[] { 10d, 10d }, new double?[] { 15d, 5d });

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			createAssignment(person, activity1, activity2);

			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		[Toggle(Toggles.OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944)]
		[Toggle(Toggles.OvertimeRequestPeriodSkillTypeSetting_47290)]
		public void ShouldMergePeriodsWithSameSkillType()
		{
			var person = User.CurrentUser();

			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();
			SkillTypeRepository.Add(phoneSkillType);

			var chatSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Chat), ForecastSource.Chat).WithId();
			SkillTypeRepository.Add(chatSkillType);

			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				BetweenDays = new MinMax<int>(0, 7),
				SkillType = phoneSkillType,
				AutoGrantType = OvertimeRequestAutoGrantType.No
			});
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				BetweenDays = new MinMax<int>(1, 4),
				SkillType = phoneSkillType,
				AutoGrantType = OvertimeRequestAutoGrantType.Deny
			});
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				BetweenDays = new MinMax<int>(0, 48),
				SkillType = chatSkillType,
				AutoGrantType = OvertimeRequestAutoGrantType.No
			});
			person.WorkflowControlSet = workFlowControlSet;
			
			var activity1 = createActivity();
			var channelSupportSkill = createSkill("channelSupportCriticalUnderStaffedSkill");
			channelSupportSkill.SkillType = phoneSkillType;
			var personSkill1 = createPersonSkill(activity1, channelSupportSkill);
			setupIntradayStaffingForSkill(channelSupportSkill, new double?[] { 10d, 10d }, new double?[] { 5d, 5d });

			var activity2 = createActivity();
			var webChatSkill = createSkill("webChatSkill not cricical understaffed at first");
			webChatSkill.SkillType = chatSkillType;
			var personSkill2 = createPersonSkill(activity2, webChatSkill);
			setupIntradayStaffingForSkill(webChatSkill, new double?[] { 10d, 10d }, new double?[] { 15d, 5d});

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			createAssignment(person, activity1, activity2);

			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().AddDays(1).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}


		[Test]
		public void ShouldGetPossibilitiesForOvertimeWithDayOff()
		{
			setupWorkFlowControlSet();
			setupSiteOpenHour();
			setupDefaultTestData();
			PersonAssignmentRepository.Clear();
			var person = User.CurrentUser();
			var assignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, Scenario.Current(),
				Now.ServerDate_DontUse(), new DayOffTemplate());
			PersonAssignmentRepository.Has(assignment);
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
		}

		[Test]
		public void ShouldOnlyGetOneDayDataIfReturnOneWeekDataIsFalse()
		{
			setupSiteOpenHour();
			setupDefaultTestData();
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
			setupDefaultTestData();
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
			setupDefaultTestData(new double?[] { 1, 1 }, new double?[] { 1, 1 });
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
		[Toggle(Toggles.MyTimeWeb_CalculateOvertimeProbabilityByPrimarySkill_44686)]
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
		[Toggle(Toggles.MyTimeWeb_CalculateOvertimeProbabilityByPrimarySkill_44686)]
		public void ShouldGetFairOvertimePossibilitiesWhenAllSkillsArePrimarySkillWithOneSkillCriticalUnderStaffing()
		{
			setupWorkFlowControlSet();
			var primarySkill = createSkill("primarySkill1");
			primarySkill.SetCascadingIndex(1);
			setupIntradayStaffingForSkill(primarySkill, new double?[] { 5, 5 }, new double?[] { 0, 0 });

			var primarySkill2 = createSkill("PrimarySkill2");
			primarySkill2.SetCascadingIndex(1);
			setupIntradayStaffingForSkill(primarySkill2, new double?[] { 5, 5 }, new double?[] { 7, 7 });

			var activity = createActivity();
			createAssignment(User.CurrentUser(), activity);
			var primaryPersonSkill = createPersonSkill(activity, primarySkill);
			var nonPrimaryPersonSkill = createPersonSkill(activity, primarySkill2);

			addPersonSkillsToPersonPeriod(primaryPersonSkill, nonPrimaryPersonSkill);

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldNotUsePrimarySkillsWhenCalculatingOvertimeProbabilityToggle44686Off()
		{
			setupWorkFlowControlSet();
			var primarySkill = createSkill("primarySkill");
			primarySkill.SetCascadingIndex(1);
			setupIntradayStaffingForSkill(primarySkill, new double?[] { 5, 5 }, new double?[] { 0, 0 });

			var nonPrimarySkill = createSkill("nonPrimarySkill");
			setupIntradayStaffingForSkill(nonPrimarySkill, new double?[] { 5, 5 }, new double?[] { 8, 8 });

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
				getPossibilityViewModels(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		[Toggle(Toggles.MyTimeWeb_CalculateOvertimeProbabilityByPrimarySkill_44686)]
		public void ShouldGetOvertimePossibilitiesWhenAgentHasNoCascadingSkillInPrimarySkills()
		{
			setupWorkFlowControlSet();
			var primarySkill = createSkill("primarySkill");
			primarySkill.SetCascadingIndex(1);
			setupIntradayStaffingForSkill(primarySkill, new double?[] { 5, 5 }, new double?[] { 1, 1 });

			var nonPrimarySkill = createSkill("nonPrimarySkill");
			nonPrimarySkill.SetCascadingIndex(2);
			setupIntradayStaffingForSkill(nonPrimarySkill, new double?[] { 5, 5 }, new double?[] { 8, 8 });

			var activity = createActivity();
			createAssignment(User.CurrentUser(), activity);
			var nonPrimaryPersonSkill = createPersonSkill(activity, nonPrimarySkill);

			addPersonSkillsToPersonPeriod(nonPrimaryPersonSkill);

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldNotGetOvertimePossibilitiesWhenSiteOpenHourIsClosed()
		{
			var personPeriod = getOrAddPersonPeriod();
			var team = personPeriod.Team;
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(),
				IsClosed = true,
				WeekDay = DayOfWeek.Thursday
			});

			setupDefaultTestData();
			setupWorkFlowControlSet();
			var result = getPossibilityViewModels(null, StaffingPossiblityType.Overtime, false).ToList();
			result.Count.Should().Be.EqualTo(0);
		}

		[Test, SetCulture("en-US")]
		public void ShouldNotGetOvertimePossibilitiesForDaysWhichSiteOpenHourIsClosed()
		{
			var personPeriod = getOrAddPersonPeriod();
			var team = personPeriod.Team;
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(),
				IsClosed = true,
				WeekDay = DayOfWeek.Thursday
			});
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = new TimePeriod(8, 17),
				IsClosed = false,
				WeekDay = DayOfWeek.Friday
			});

			setupDefaultTestData();
			setupWorkFlowControlSet();
			var result = getPossibilityViewModels(null, StaffingPossiblityType.Overtime).ToList();
			result.Count.Should().Be.EqualTo(4);

			var possibilities = result.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(0, possibilities.Count);
		}

		[Test, SetCulture("en-US")]
		public void ShouldNotReturnPossibiliesForDaysOutOfOvertimeOpenPeriod()
		{
			ToggleManager.Enable(Toggles.Wfm_Staffing_StaffingReadModel28DaysStep1_45109);

			setupSiteOpenHour();
			setupDefaultTestData();

			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
				.WithId();
			SkillTypeRepository.Add(phoneSkillType);

			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				BetweenDays = new MinMax<int>(0, 13),
				SkillType = phoneSkillType
			});
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = getPossibilityViewModels(Now.ServerDate_DontUse().AddWeeks(3), StaffingPossiblityType.Overtime).ToList();
			result.Count.Should().Be.EqualTo(0);
		}

		[Test, SetCulture("en-US")]
		public void ShouldReturnPossibiliesWithoutOvertimeOpenPeriod()
		{
			setupSiteOpenHour();
			setupDefaultTestData();

			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
				.WithId();
			SkillTypeRepository.Add(phoneSkillType);

			var workFlowControlSet = new WorkflowControlSet();
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = getPossibilityViewModels(Now.ServerDate_DontUse(), StaffingPossiblityType.Overtime).ToList();
			result.Count.Should().Be.EqualTo(6);
		}

		[Test, SetCulture("en-US")]
		public void ShouldReturnCorrectEndtimeOnDSTDay()
		{
			TimeZone.Is(TimeZoneInfoFactory.CentralStandardTime());
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			Now.Is("2018-03-11 6:00");

			var personPeriod = getOrAddPersonPeriod();
			var timePeriod = new TimePeriod(1, 0, 4, 0);
			var team = personPeriod.Team;
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});

			var activity = createActivity();
			var skill = createSkill("test1", timePeriod);
			skill.TimeZone = TimeZone.TimeZone();
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(personSkill);
			createAssignment(User.CurrentUser(), activity);

			var date = new DateOnly(2018, 3, 11);
			var staffingPeriodData = new List<StaffingPeriodData>();
			staffingPeriodData.Add(new StaffingPeriodData(){Period = date.ToDateTimePeriod(timePeriod, TimeZone.TimeZone()),ForecastedStaffing = 1,ScheduledStaffing = 2});
			setupIntradayStaffingForSkill(skill, date, staffingPeriodData);

			var workFlowControlSet = new WorkflowControlSet();
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.GetPossibilityViewModels(date, StaffingPossiblityType.Overtime, false).ToList();
			result.Count.Should().Be.EqualTo(8);
			result[3].EndTime.TimeOfDay.TotalMinutes.Should().Be.EqualTo(180);
		}

		private void setupWorkFlowControlSet()
		{
			var absenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(Now.ServerDate_DontUse().AddDays(-20), Now.ServerDate_DontUse().AddDays(20)),
				OpenForRequestsPeriod = new DateOnlyPeriod(Now.ServerDate_DontUse().AddDays(-20), Now.ServerDate_DontUse().AddDays(20)),
				StaffingThresholdValidator = new StaffingThresholdValidator()
			};
			var overtimeRequestOpenDatePeriod = new OvertimeRequestOpenRollingPeriod
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

		private void setupDefaultTestData(double?[] forecastedStaffing = null, double?[] scheduledStaffing = null)
		{
			var activity = createActivity();
			var skill = createSkill("test1");
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
			skill.SkillType.Description = new Description("SkillTypeInboundTelephony");
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
				setupIntradayStaffingForSkill(skill, new DateOnly(utcDate), new[] { staffingPeriodData1, staffingPeriodData2 });
			});
		}

		private void setupIntradayStaffingForSkill(ISkill skill, DateOnly date, IEnumerable<StaffingPeriodData> staffingPeriodDatas)
		{
			var skillCombinationResources = new List<SkillCombinationResource>();
			var skillForecastedStaffings = new List<Tuple<TimePeriod, double>>();

			foreach (var staffingPeriodData in staffingPeriodDatas)
			{
				skillCombinationResources.AddRange(createSkillCombinationResources(skill, staffingPeriodData.Period, staffingPeriodData.ScheduledStaffing));
				skillForecastedStaffings.AddRange(createSkillForecastedStaffings(skill, staffingPeriodData.Period, staffingPeriodData.ForecastedStaffing));
			}

			setupIntradayStaffingForSkill(skill, date, skillCombinationResources, skillForecastedStaffings);
		}

		private void setupIntradayStaffingForSkill(ISkill skill, DateOnly date, IEnumerable<SkillCombinationResource> skillCombinationResources, IEnumerable<Tuple<TimePeriod, double>> skillForecastedStaffings)
		{
			foreach (var skillCombinationResource in skillCombinationResources)
			{
				CombinationRepository.AddSkillCombinationResource(new DateTime(),
					new[]
					{
						skillCombinationResource
					});
			}

			var skillDay = skill.CreateSkillDayWithDemandOnInterval(Scenario.Current(),
				date, 0,
				skillForecastedStaffings.ToArray());
			skillDay.SkillDataPeriodCollection.ForEach(s => { s.Shrinkage = new Percent(0.5); });
			SkillDayRepository.Has(skillDay);
		}

		private List<SkillCombinationResource> createSkillCombinationResources(ISkill skill, DateTimePeriod dateTimePeriod, double scheduledStaffing)
		{
			var skillCombinationResources = new List<SkillCombinationResource>();
			var intervals = dateTimePeriod.Intervals(TimeSpan.FromMinutes(skill.DefaultResolution));
			for (var i = 0; i < intervals.Count; i++)
			{
				skillCombinationResources.Add(
					new SkillCombinationResource
					{
						StartDateTime = intervals[i].StartDateTime,
						EndDateTime = intervals[i].EndDateTime,
						Resource = scheduledStaffing,
						SkillCombination = new[] { skill.Id.Value }
					}
				);
			}
			return skillCombinationResources;
		}

		private List<Tuple<TimePeriod, double>> createSkillForecastedStaffings(ISkill skill, DateTimePeriod dateTimePeriod, double forecastedStaffing)
		{
			var skillForecastedStaffings = new List<Tuple<TimePeriod, double>>();

			var timezone = User.CurrentUser().PermissionInformation.DefaultTimeZone();
			for (var time = dateTimePeriod.StartDateTimeLocal(timezone);
				time < dateTimePeriod.EndDateTimeLocal(timezone);
				time = time.AddMinutes(skill.DefaultResolution))
			{
				skillForecastedStaffings.Add(new Tuple<TimePeriod, double>(
					new TimePeriod(time.TimeOfDay, time.AddMinutes(skill.DefaultResolution).TimeOfDay),
					forecastedStaffing));
			}
			return skillForecastedStaffings;
		}

		private class StaffingPeriodData
		{
			public DateTimePeriod Period;

			public double ForecastedStaffing;

			public double ScheduledStaffing;
		}
	}
}
