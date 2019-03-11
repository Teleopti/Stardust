using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
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
using Teleopti.Ccc.WebTest.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[MyTimeWebTest]
	public class ScheduleStaffingPossibilityControllerAbsenceForMobileDayTest : IIsolateSystem
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

		private readonly TimeSpan[] intervals =
			{TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15)), TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30))};

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
		[Ignore("day")]
		[Test]
		public void ShouldReturnPossibilitiesBaseOnTopLayerSkill()
		{
			var today = Now.UtcDateTime().ToDateOnly();

			setupSiteOpenHour();
			setupWorkFlowControlSet();

			var person = User.CurrentUser();
			var baseActivity = createActivity("base activity");
			var underlyingActivitySkill = createSkill("underlyingActivitySkill", new TimePeriod(0, 24));
			var personSkill1 = createPersonSkill(baseActivity, underlyingActivitySkill);
			setupIntradayStaffingSkillFor24Hours(underlyingActivitySkill, 2.353d, 3.00d);
			var period1 = new DateTimePeriod(today.Date.AddHours(8).Utc(), today.Date.AddHours(17).Utc());
			var assignment = createAssignment(person, period1, baseActivity);

			var onTopActivity = createActivity("on top activity");
			var onTopActivitySkill = createSkill("onTopActivitySkill", new TimePeriod(0, 24));
			var personSkill2 = createPersonSkill(onTopActivity, onTopActivitySkill);
			setupIntradayStaffingSkillFor24Hours(onTopActivitySkill, 2.353d, 5.00d);
			var period2 = new DateTimePeriod(today.Date.AddHours(10).Utc(), today.Date.AddHours(11).Utc());
			assignment.AddActivity(onTopActivity, period2.StartDateTime, period2.EndDateTime);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var possibilities = Target.GetPossibilityViewModelsForMobileDay(today, StaffingPossibilityType.Absence).ToList();

			possibilities.Count.Should().Be(96);
			possibilities.Where(x => x.StartTime.Hour == 10 && x.Possibility == 1)
				.ToList().Count.Should().Be(4);
		}

		[Ignore("day")]
		[Test]
		public void ShouldReturnPossibilitiesForCurrentWeek()
		{
			setupSiteOpenHour();
			setupTestData();
			setupWorkFlowControlSet();

			var result = Target.GetPossibilityViewModelsForMobileDay(null, StaffingPossibilityType.Absence).ToList();
			result.Count.Should().Be.EqualTo(2);

			var dayCollection = new DateOnlyPeriod(Now.UtcDateTime().ToDateOnly(), Now.UtcDateTime().ToDateOnly().AddDays(2)).DayCollection().ToList();

			result.Count(d => d.Date == dayCollection[0].ToFixedClientDateOnlyFormat()).Should().Be(2);
			result.Count(d => d.Date == dayCollection[1].ToFixedClientDateOnlyFormat()).Should().Be(0);
			result.Count(d => d.Date == dayCollection[2].ToFixedClientDateOnlyFormat()).Should().Be(0);
		}

		[Ignore("day")]
		[Test]
		public void ShouldReturnPossibilitiesInNextWeek()
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

			var result = Target.GetPossibilityViewModelsForMobileDay(dayInNextWeek, StaffingPossibilityType.Absence).ToList();

			result.Count.Should().Be.EqualTo(2);
			result[0].Date.Should().Be.EqualTo(dayInNextWeek.ToFixedClientDateOnlyFormat());
			result[1].Date.Should().Be.EqualTo(dayInNextWeek.ToFixedClientDateOnlyFormat());
		}

		[Ignore("day")]
		[Test]
		public void ShouldNotReturnPossibilitiesForPastDays()
		{
			setupSiteOpenHour();
			setupTestData();
			setupWorkFlowControlSet();

			var result = Target.GetPossibilityViewModelsForMobileDay(Now.UtcDateTime().ToDateOnly().AddDays(-1), StaffingPossibilityType.Absence).ToList();

			result.Count.Should().Be.EqualTo(0);
		}
		[Ignore("day")]
		[Test]
		public void ShouldGetPossibilitiesThereIsAnOvernightSchedule()
		{
			var person = User.CurrentUser();
			var activity = createActivity();
			var skill = createSkill("skill", new TimePeriod(00, 00, 24, 00));
			var personSkill = createPersonSkill(activity, skill);

			setupIntradayStaffingSkillFor24Hours(skill, 10d, 20d);
			addPersonSkillsToPersonPeriod(personSkill);

			var datetimePeriod = new DateTimePeriod(Now.UtcDateTime().Date.AddHours(18), Now.UtcDateTime().Date.AddHours(26));
			var datetimePeriod2 = new DateTimePeriod(Now.UtcDateTime().Date.AddDays(1).AddHours(18), Now.UtcDateTime().Date.AddDays(1).AddHours(26));

			createAssignment(person, datetimePeriod, activity);
			createAssignment(person, datetimePeriod2, activity);
			setupWorkFlowControlSet();

			var possibilities = Target.GetPossibilityViewModelsForMobileDay(Now.UtcDateTime().ToDateOnly().AddDays(1),
				StaffingPossibilityType.Absence).ToList();

			possibilities.Count.Should().Be(96);
			possibilities.ElementAt(0).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(0).StartTime.Should().Be.EqualTo(Now.UtcDateTime().Date.AddDays(1));
			possibilities.ElementAt(1).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(2).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(3).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(4).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(5).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(6).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(7).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(7).StartTime.Should().Be.EqualTo(Now.UtcDateTime().Date.AddDays(1).AddHours(1).AddMinutes(45));
		}

		[Ignore("day")]
		[Test]
		public void ShouldOnlyGetOneDayDataIfReturnOneWeekDataIsFalse()
		{
			setupSiteOpenHour();
			setupTestData();
			setupWorkFlowControlSet();

			var result = Target.GetPossibilityViewModelsForMobileDay(null, StaffingPossibilityType.Absence).ToList();

			result.Count.Should().Be.EqualTo(2);
		}

		[Ignore("day")]
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

			var result = Target.GetPossibilityViewModelsForMobileDay(today, StaffingPossibilityType.Absence).ToList();

			result.Count.Should().Be.EqualTo(2);
			result.FirstOrDefault()?.Date.Should().Be(today.ToFixedClientDateOnlyFormat());
		}

		[Ignore("day")]
		[Test]
		public void ShouldSubtractCurrentUserShiftWhenSceduledStaffingLagerThanOne()
		{
			var underStaffedSkill = createSkill("skill understaffed");
			underStaffedSkill.StaffingThresholds =
				new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0.1));

			setupTestData(new double?[] { 5, 5 }, new double?[] { 5, 5 });

			setupWorkFlowControlSet();

			var possibilities =
				Target.GetPossibilityViewModelsForMobileDay(null, StaffingPossibilityType.Absence).ToList();

			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		private void setupIntradayStaffingSkillFor24Hours(ISkill skill, double forecastedStaffing,
			double scheduledStaffing)
		{
			var period = getAvailablePeriod();
			period.DayCollection().ToList().ForEach(day =>
			{
				var staffingDataList = new List<StaffingPeriodData>();
				var utcDate = TimeZoneHelper.ConvertToUtc(day.Date, User.CurrentUser().PermissionInformation.DefaultTimeZone());
				var start = TimeSpan.Zero;
				while (day.Date.Add(start) < day.Date.AddDays(1).Subtract(TimeSpan.FromSeconds(1)))
				{
					var staffingPeriodData = new StaffingPeriodData
					{
						ForecastedStaffing = forecastedStaffing,
						ScheduledStaffing = scheduledStaffing,
						Period = new DateTimePeriod(utcDate.Date.Add(start), utcDate.Date.Add(start.Add(TimeSpan.FromMinutes(15))))
					};
					start += TimeSpan.FromMinutes(15);
					staffingDataList.Add(staffingPeriodData);
				}

				SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(skill, new DateOnly(utcDate),
					staffingDataList, User.CurrentUser().PermissionInformation.DefaultTimeZone());
			});
		}

		private static IActivity createActivity(string name = "activity")
		{
			var activity = ActivityFactory.CreateActivity(name);
			activity.RequiresSkill = true;
			return activity;
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

		private IPersonAssignment createAssignment(IPerson person, DateTimePeriod period, params IActivity[] activities)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				Scenario.Current(), period,
				ShiftCategoryFactory.CreateShiftCategory(), activities);
			PersonAssignmentRepository.Has(assignment);
			return assignment;
		}

		private DateOnlyPeriod getAvailablePeriod()
		{
			var staffingInfoAvailableDays = StaffingInfoAvailableDaysProvider.GetDays(ToggleManager);
			var today = Now.UtcDateTime();
			var period = new DateOnlyPeriod(today, today.AddDays(staffingInfoAvailableDays)).Inflate(1);
			return period;
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