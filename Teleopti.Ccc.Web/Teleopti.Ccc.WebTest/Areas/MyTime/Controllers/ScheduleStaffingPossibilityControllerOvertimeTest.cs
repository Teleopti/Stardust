using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
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
	public class ScheduleStaffingPossibilityControllerOvertimeTest : IIsolateSystem
	{
		public ScheduleStaffingPossibilityController Target;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillTypeRepository SkillTypeRepository;
		public FakeToggleManager ToggleManager;
		public SkillIntradayStaffingFactory SkillIntradayStaffingFactory;

		private readonly TimeSpan[] intervals = { TimeSpan.FromMinutes(495), TimeSpan.FromMinutes(510) };
		readonly ISkillType phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();

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
		public void ShouldReturnPossibiliesForDaysLongerThan14()
		{
			setupSiteOpenHour();
			setupDefaultTestData();
			setupWorkFlowControlSet();

			var workflowControlSet = User.CurrentUser().WorkflowControlSet;

			var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(ToggleManager);
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, staffingInfoAvailableDays)
			});

			var result = getPossibilityViewModels(new DateOnly(Now.UtcDateTime()).AddWeeks(3), StaffingPossiblityType.Overtime).ToList();
			result.Count.Should().Be.EqualTo(14);
			DateHelper.GetWeekPeriod(new DateOnly(Now.UtcDateTime()).AddWeeks(3), CultureInfo.CurrentCulture)
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
			setupSiteOpenHour();
			setupDefaultTestData();
			setupWorkFlowControlSet();

			var workflowControlSet = User.CurrentUser().WorkflowControlSet;
			var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(ToggleManager);
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, staffingInfoAvailableDays)
			});

			var result = getPossibilityViewModels(new DateOnly(Now.UtcDateTime()).AddWeeks(4), StaffingPossiblityType.Overtime).ToList();
			result.Count.Should().Be.EqualTo(14);
			DateHelper.GetWeekPeriod(new DateOnly(Now.UtcDateTime()).AddWeeks(4), CultureInfo.CurrentCulture)
				.DayCollection()
				.ToList()
				.ForEach(day =>
				{
					Assert.AreEqual(2, result.Count(d => d.Date == day.ToFixedClientDateOnlyFormat()), day.ToShortDateString());
				});
		}

		[Test]
		public void ShouldReturnEmptyPossibilities()
		{
			var possibilities = getPossibilityViewModels(null).ToList();
			Assert.AreEqual(0, possibilities.Count);
		}

		[Test]
		public void ShouldGetPossibilitiesWithoutIntradaySchedule()
		{
			setupSiteOpenHour();
			setupDefaultTestData();
			setupWorkFlowControlSet();
			PersonAssignmentRepository.Clear();
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
		}

		[Test]
		public void ShouldNotGetPossibilitiesForNotSupportedSkill()
		{
			setupSiteOpenHour();
			setupDefaultTestData();
			setupWorkFlowControlSet();

			var overtimeRequestOpenPeriod = User.CurrentUser().WorkflowControlSet.OvertimeRequestOpenPeriods.FirstOrDefault();
			overtimeRequestOpenPeriod.ClearPeriodSkillType();
			overtimeRequestOpenPeriod.AddPeriodSkillType(
				new OvertimeRequestOpenPeriodSkillType(new SkillTypePhone(new Description("NotSupportedSkillType"),
					ForecastSource.Backoffice)));
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime).ToList();
			Assert.AreEqual(0, possibilities.Count);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForOvertimeWhenOverstaffing()
		{
			setupWorkFlowControlSet();
			setupSiteOpenHour();
			setupDefaultTestData(new double?[] { 10d, 10d }, new double?[] { 22d, 22d });
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairPossibilitiesAfterRoundStaffingDataWithTwoFractional()
		{
			setupWorkFlowControlSet();
			setupSiteOpenHour();
			setupDefaultTestData(new double?[] { 0.001, 0.001d }, new double?[] { 0d, 0d });
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
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
			var emailSkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(0, possibilities.Count);
		}

		[Test]
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
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForOvertimeWhenOverstaffingAndSkillTypeIsNotSet()
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
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
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
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			User.CurrentUser().WorkflowControlSet = workflowControlSet;

			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
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
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 1),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 2),
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
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 1),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 2),
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
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				BetweenDays = new MinMax<int>(0, 7),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				BetweenDays = new MinMax<int>(1, 2),
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
				.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForOvertimeWhenNotOverstaffingWithoutShrinkage()
		{
			setupWorkFlowControlSet();
			User.CurrentUser().WorkflowControlSet.OvertimeRequestStaffingCheckMethod =
				OvertimeRequestStaffingCheckMethod.Intraday;
			setupSiteOpenHour();
			setupDefaultTestData(new double?[] { 10d, 10d }, new double?[] { 8d, 8d });
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetGoodPossibilitiesForOvertimeWhenNotOverstaffingWithShrinkage()
		{
			setupWorkFlowControlSet();
			User.CurrentUser().WorkflowControlSet.OvertimeRequestStaffingCheckMethod =
				OvertimeRequestStaffingCheckMethod.IntradayWithShrinkage;
			setupSiteOpenHour();
			setupDefaultTestData(new double?[] { 10d, 10d }, new double?[] { 8d, 8d });
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat()).ToList();
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
				.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
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
				.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldMergePeriodsWithSameSkillType()
		{
			var person = User.CurrentUser();

			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();
			SkillTypeRepository.Add(phoneSkillType);

			var chatSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Chat), ForecastSource.Chat).WithId();
			SkillTypeRepository.Add(chatSkillType);

			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, 7),
				AutoGrantType = OvertimeRequestAutoGrantType.No
			});
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(1, 4),
				AutoGrantType = OvertimeRequestAutoGrantType.Deny
			});
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { chatSkillType })
			{
				BetweenDays = new MinMax<int>(0, 48),
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
			setupIntradayStaffingForSkill(webChatSkill, new double?[] { 10d, 10d }, new double?[] { 15d, 5d });

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			createAssignment(person, activity1, activity2);

			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).AddDays(1).ToFixedClientDateOnlyFormat()).ToList();
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
				new DateOnly(Now.UtcDateTime()), new DayOffTemplate());
			PersonAssignmentRepository.Has(assignment);
			var possibilities = getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
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
					.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldUseAllSkillsForOvertimeProbabilityWhenUsePrimarySkillValidationIsDisabled()
		{
			setupWorkFlowControlSet();

			User.CurrentUser().WorkflowControlSet.OvertimeRequestUsePrimarySkill = false;

			var primarySkill = createSkill("primarySkill");
			primarySkill.SetCascadingIndex(1);
			setupIntradayStaffingForSkill(primarySkill, new double?[] { 5, 5 }, new double?[] { 10, 10 });

			var nonPrimarySkill = createSkill("nonPrimarySkill");
			setupIntradayStaffingForSkill(nonPrimarySkill, new double?[] { 5, 5 }, new double?[] { 1, 1 });

			var activity = createActivity();
			createAssignment(User.CurrentUser(), activity);
			var primaryPersonSkill = createPersonSkill(activity, primarySkill);
			var nonPrimaryPersonSkill = createPersonSkill(activity, nonPrimarySkill);

			addPersonSkillsToPersonPeriod(primaryPersonSkill, nonPrimaryPersonSkill);

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
					.Where(d => d.Date == Now.UtcDateTime().Date.ToString("yyyy-MM-dd"))
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldUsePrimarySkillsForOvertimeProbabilityWhenUsePrimarySkillValidationIsEnabled()
		{
			setupWorkFlowControlSet();

			User.CurrentUser().WorkflowControlSet.OvertimeRequestUsePrimarySkill = true;

			var primarySkill = createSkill("primarySkill");
			primarySkill.SetCascadingIndex(1);
			setupIntradayStaffingForSkill(primarySkill, new double?[] { 5, 5 }, new double?[] { 10, 10 });

			var nonPrimarySkill = createSkill("nonPrimarySkill");
			setupIntradayStaffingForSkill(nonPrimarySkill, new double?[] { 5, 5 }, new double?[] { 1, 1 });

			var activity = createActivity();
			createAssignment(User.CurrentUser(), activity);
			var primaryPersonSkill = createPersonSkill(activity, primarySkill);
			var nonPrimaryPersonSkill = createPersonSkill(activity, nonPrimarySkill);

			addPersonSkillsToPersonPeriod(primaryPersonSkill, nonPrimaryPersonSkill);

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
					.Where(d => d.Date == Now.UtcDateTime().Date.ToString("yyyy-MM-dd"))
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldUsePrimarySkillsForOvertimeProbabilityWhenPrimarySkillIsNotLevel1InCascading()
		{
			setupWorkFlowControlSet();

			User.CurrentUser().WorkflowControlSet.OvertimeRequestUsePrimarySkill = true;

			var primarySkill1 = createSkill("primarySkill");
			primarySkill1.SetCascadingIndex(1);
			setupIntradayStaffingForSkill(primarySkill1, new double?[] { 5, 5 }, new double?[] { 10, 10 });

			var primarySkill2 = createSkill("primarySkill2");
			primarySkill2.SetCascadingIndex(2);
			setupIntradayStaffingForSkill(primarySkill2, new double?[] { 5, 5 }, new double?[] { 10, 10 });

			var primarySkill3 = createSkill("primarySkill3");
			setupIntradayStaffingForSkill(primarySkill3, new double?[] { 5, 5 }, new double?[] { 1, 1 });

			var activity = createActivity();
			createAssignment(User.CurrentUser(), activity);
			var primaryPersonSkill2 = createPersonSkill(activity, primarySkill2);
			var primaryPersonSkill3 = createPersonSkill(activity, primarySkill3);

			addPersonSkillsToPersonPeriod(primaryPersonSkill2, primaryPersonSkill3);

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
					.Where(d => d.Date == Now.UtcDateTime().Date.ToString("yyyy-MM-dd"))
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetGoodOvertimePossibilitiesWhenAllSkillsArePrimarySkillWithOneSkillCriticalUnderStaffing()
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
			var primaryPersonSkill2 = createPersonSkill(activity, primarySkill2);

			addPersonSkillsToPersonPeriod(primaryPersonSkill, primaryPersonSkill2);

			var possibilities =
				getPossibilityViewModels(null, StaffingPossiblityType.Overtime)
					.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat())
					.ToList();

			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
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
					.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat())
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

			var possibilities = result.Where(d => d.Date == new DateOnly(Now.UtcDateTime()).ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(0, possibilities.Count);
		}

		[Test, SetCulture("en-US")]
		public void ShouldNotReturnPossibiliesForDaysOutOfOvertimeOpenPeriod()
		{
			setupSiteOpenHour();
			setupDefaultTestData();

			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
				.WithId();
			SkillTypeRepository.Add(phoneSkillType);

			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, 13)
			});
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = getPossibilityViewModels(new DateOnly(Now.UtcDateTime()).AddWeeks(3), StaffingPossiblityType.Overtime).ToList();
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

			var workFlowControlSet = new WorkflowControlSet()
			{
				OvertimeProbabilityEnabled = true
			};
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = getPossibilityViewModels(new DateOnly(Now.UtcDateTime()), StaffingPossiblityType.Overtime).ToList();
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
			staffingPeriodData.Add(new StaffingPeriodData() { Period = date.ToDateTimePeriod(timePeriod, TimeZone.TimeZone()), ForecastedStaffing = 1, ScheduledStaffing = 2 });
			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(skill, date, staffingPeriodData, TimeZone.TimeZone());

			var workFlowControlSet = new WorkflowControlSet();
			workFlowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				BetweenDays = new MinMax<int>(0, 13)
			});
			User.CurrentUser().WorkflowControlSet = workFlowControlSet;

			var result = Target.GetPossibilityViewModels(date, StaffingPossiblityType.Overtime, false).ToList();
			result.Count.Should().Be.EqualTo(8);
			result[3].EndTime.TimeOfDay.TotalMinutes.Should().Be.EqualTo(180);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForOvertimeWhenSeriousUnderstaffingIsZeroAndForcastedAndScheduledAreEqual()
		{
			TimeZone.Is(TimeZoneInfoFactory.DenverTimeZoneInfo());
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());

			Now.Is("2018-09-19 6:00");
			var today = new DateOnly(Now.UtcDateTime());

			setupWorkFlowControlSet();
			User.CurrentUser().WorkflowControlSet.OvertimeRequestStaffingCheckMethod = OvertimeRequestStaffingCheckMethod.Intraday;

			setupSiteOpenHour();

			var person = User.CurrentUser();
			var activity1 = createActivity();
			var skill1 = createSkill("skill1");
			skill1.TimeZone = TimeZone.TimeZone();
			skill1.StaffingThresholds = createStaffingThresholds(0, 0, 0.1);
			var personSkill1 = createPersonSkill(activity1, skill1);

			setUpIntradayStaffing(today, skill1, new double?[] { 6d, 6d }, new double?[] { 6d, 6d });

			addPersonSkillsToPersonPeriod(personSkill1);

			createAssignment(person, activity1);

			var possibilitiesWeek = getPossibilityViewModels(today, StaffingPossiblityType.Overtime)
				.Where(d => d.Date == today.ToFixedClientDateOnlyFormat()).ToList();

			Assert.AreEqual(2, possibilitiesWeek.Count);
			Assert.AreEqual(0, possibilitiesWeek.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilitiesWeek.ElementAt(1).Possibility);
		}

		private void setupWorkFlowControlSet()
		{
			var absenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()).AddDays(-20), new DateOnly(Now.UtcDateTime()).AddDays(20)),
				OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()).AddDays(-20), new DateOnly(Now.UtcDateTime()).AddDays(20)),
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
				(PersonPeriod)User.CurrentUser().PersonPeriods(new DateOnly(Now.UtcDateTime()).ToDateOnlyPeriod()).FirstOrDefault();
			if (personPeriod != null) return personPeriod;
			var team = TeamFactory.CreateTeam("team1", "site1");
			personPeriod =
				(PersonPeriod)
				PersonPeriodFactory.CreatePersonPeriod(new DateOnly(Now.UtcDateTime()), PersonContractFactory.CreatePersonContract(), team);
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
			var today = new DateOnly(Now.UtcDateTime());
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
				setUpIntradayStaffing(day, skill, forecastedStaffings, scheduledStaffings);
			});
		}

		private void setUpIntradayStaffing(DateOnly day, ISkill skill, double?[] forecastedStaffings, double?[] scheduledStaffings)
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
		}
	}
}
