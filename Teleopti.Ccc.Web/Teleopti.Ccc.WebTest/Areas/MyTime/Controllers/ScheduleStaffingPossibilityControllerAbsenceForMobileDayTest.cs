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
	public class ScheduleStaffingPossibilityControllerAbsenceForMobileDayTest : IIsolateSystem, ITestInterceptor
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


		private readonly TimePeriod _defaultSiteOpenHour = new TimePeriod(0, 0, 24, 0);
		private readonly TimePeriod _defaultSkillOpenHour = new TimePeriod(0, 00, 24, 00);
		private DateTimePeriod _defaultAssignmentPeriod;
		private int _defaultSkillStaffingIntervalNumber;
		private TimeZoneInfo _defaultTimezone;
		private IPerson _loggedUser;
		private DateOnly _today;

		readonly ISkillType phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
			.WithId();

		public void OnBefore()
		{
			_today = Now.UtcDateTime().ToDateOnly();
			_loggedUser = User.CurrentUser();
			_defaultTimezone = User.CurrentUser().PermissionInformation.DefaultTimeZone();

			_defaultSkillStaffingIntervalNumber =
				(int)_defaultSkillOpenHour.EndTime.Subtract(_defaultSkillOpenHour.StartTime).TotalMinutes / 15;

			_defaultAssignmentPeriod =
				new DateTimePeriod(TimeZoneHelper.ConvertToUtc(_today.Date.AddHours(8), _defaultTimezone),
					TimeZoneHelper.ConvertToUtc(_today.Date.AddHours(17), _defaultTimezone));
		}

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
		public void ShouldReturnPossibilitiesForCurrentDay()
		{
			setupSiteOpenHour(_defaultSiteOpenHour);
			setupWorkFlowControlSet();

			var activity = createActivity("activity for test");
			createAssignment(_loggedUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test", new TimePeriod(0, 24));
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(_today, personSkill);

			setupIntradayStaffingSkillFor24Hours(skill, _today, 2.353d, 2.00d);

			var result = Target.GetPossibilityViewModelsForMobileDay(_today, StaffingPossibilityType.Absence).ToList();

			result.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber);
			result[0].Possibility.Should().Be(0);
			result[_defaultSkillStaffingIntervalNumber - 1].Possibility.Should().Be(0);
		}

		[Test]
		public void ShouldNotReturnPossibilitiesForPastDays()
		{
			setupSiteOpenHour(_defaultSiteOpenHour);
			setupWorkFlowControlSet();

			var activity = createActivity("activity for test");
			createAssignment(_loggedUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill for test", new TimePeriod(0, 24));
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(_today, personSkill);

			setupIntradayStaffingSkillFor24Hours(skill, _today.AddDays(-1), 2.353d, 2.00d);

			var result = Target.GetPossibilityViewModelsForMobileDay(_today.AddDays(-1), StaffingPossibilityType.Absence).ToList();

			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetPossibilitiesThereIsAnOvernightSchedule()
		{
			setupSiteOpenHour(_defaultSiteOpenHour);
			setupWorkFlowControlSet();

			var activity = createActivity("activity for test");
			var datetimePeriod = new DateTimePeriod(
				TimeZoneHelper.ConvertToUtc(_today.Date.AddHours(18), _defaultTimezone),
				TimeZoneHelper.ConvertToUtc(_today.Date.AddHours(26), _defaultTimezone));
			createAssignment(_loggedUser, datetimePeriod, activity);

			var skill = createSkill("skill for test", new TimePeriod(0, 24));
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(_today, personSkill);

			setupIntradayStaffingSkillFor24Hours(skill, _today, 10d, 20d);
			setupIntradayStaffingSkillFor24Hours(skill, _today.AddDays(1), 10d, 20d);

			var possibilities = Target.GetPossibilityViewModelsForMobileDay(_today,
				StaffingPossibilityType.Absence).ToList();

			possibilities.Count.Should().Be(_defaultSkillStaffingIntervalNumber);
			possibilities.ElementAt(0).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(0).StartTime.Should().Be.EqualTo(_today.Date);
			possibilities.ElementAt(1).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(2).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(3).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(4).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(5).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(6).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(7).Possibility.Should().Be.EqualTo(1);
			possibilities.ElementAt(7).StartTime.Should().Be.EqualTo(_today.Date.AddHours(1).AddMinutes(45));
		}

		[Test]
		public void ShouldGetPossibilitiesAccordingToAgentTimeZone()
		{
			Now.Is(_today.Date); //Remove the default time of now 1:31:00PM and use 12:00:00AM

			setupSiteOpenHour(_defaultSiteOpenHour);
			setupWorkFlowControlSet();

			var timeZoneInfo = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(timeZoneInfo);

			var viewDateInUserTimeZone = new DateOnly(TimeZoneHelper.ConvertFromUtc(_today.Date,
				User.CurrentUser().PermissionInformation.DefaultTimeZone()));

			var activity = createActivity("activity for test");
			var datetimePeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(viewDateInUserTimeZone.Date.AddHours(8), timeZoneInfo),
				TimeZoneHelper.ConvertToUtc(viewDateInUserTimeZone.Date.AddHours(17), timeZoneInfo));
			createAssignment(_loggedUser, datetimePeriod, activity);

			var skill = createSkill("skill for test", new TimePeriod(0, 24));
			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(_today.AddDays(-3), personSkill);

			setupIntradayStaffingSkillFor24Hours(skill, viewDateInUserTimeZone, 2.353d, 2.00d);

			var result = Target
				.GetPossibilityViewModelsForMobileDay(viewDateInUserTimeZone, StaffingPossibilityType.Absence).ToList();

			result.Count.Should().Be.EqualTo(_defaultSkillStaffingIntervalNumber);
			result.FirstOrDefault()?.Date.Should().Be(viewDateInUserTimeZone.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldSubtractCurrentUserShiftWhenScheduledStaffingIsLargerThanOne()
		{
			setupSiteOpenHour(_defaultSiteOpenHour);
			setupWorkFlowControlSet();

			var activity = createActivity("activity for test");
			createAssignment(_loggedUser, _defaultAssignmentPeriod, activity);

			var skill = createSkill("skill understaffed", new TimePeriod(0, 24));
			skill.StaffingThresholds = new StaffingThresholds(new Percent(0), new Percent(0), new Percent(0.1));

			var personSkill = createPersonSkill(activity, skill);
			addPersonSkillsToPersonPeriod(_today, personSkill);

			setupIntradayStaffingSkillFor24Hours(skill, _today, 10d, 10.05d);

			var possibilities =
				Target.GetPossibilityViewModelsForMobileDay(_today, StaffingPossibilityType.Absence).ToList();

			possibilities.Count.Should().Be(_defaultSkillStaffingIntervalNumber);
			possibilities.ElementAt(_defaultAssignmentPeriod.StartDateTime.Hour * 4).Possibility.Should().Be(0);
		}

		private void setupWorkFlowControlSet()
		{
			var absenceRequestOpenDatePeriod = new AbsenceRequestOpenDatePeriod
			{
				Absence = AbsenceFactory.CreateAbsence("absence for test"),
				Period = new DateOnlyPeriod(_today.AddDays(-20),
					_today.AddDays(20)),
				OpenForRequestsPeriod = new DateOnlyPeriod(_today.AddDays(-20),
					_today.AddDays(20)),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			var overtimeRequestOpenDatePeriod = new OvertimeRequestOpenRollingPeriod(new[] { phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 13)
			};

			var workFlowControlSet = new WorkflowControlSet();

			workFlowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenDatePeriod);
			workFlowControlSet.AddOpenOvertimeRequestPeriod(overtimeRequestOpenDatePeriod);

			_loggedUser.WorkflowControlSet = workFlowControlSet;
		}

		private void setupSiteOpenHour(TimePeriod timePeriod)
		{
			var personPeriod = getOrAddPersonPeriod(_today);
			var team = personPeriod.Team;
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
		}

		private PersonPeriod getOrAddPersonPeriod(DateOnly startDate)
		{
			var personPeriod =
				(PersonPeriod)_loggedUser.PersonPeriods(startDate.ToDateOnlyPeriod())
					.FirstOrDefault();
			if (personPeriod != null) return personPeriod;
			var team = TeamFactory.CreateTeam("team1", "site1");
			personPeriod =
				(PersonPeriod)
				PersonPeriodFactory.CreatePersonPeriod(startDate,
					PersonContractFactory.CreatePersonContract(), team);
			_loggedUser.AddPersonPeriod(personPeriod);
			return personPeriod;
		}

		private void addPersonSkillsToPersonPeriod(DateOnly startDate, params IPersonSkill[] personSkills)
		{
			var personPeriod = getOrAddPersonPeriod(startDate);
			foreach (var personSkill in personSkills)
			{
				personPeriod.AddPersonSkill(personSkill);
			}
		}

		private IPersonSkill createPersonSkill(IActivity activity, ISkill skill)
		{
			skill.Activity = activity;
			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			return personSkill;
		}

		private ISkill createSkill(string name, TimePeriod openHour)
		{
			var skill = SkillFactory.CreateSkill(name).WithId();
			skill.SkillType = phoneSkillType;
			skill.StaffingThresholds = createStaffingThresholds();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHour);
			SkillRepository.Has(skill);
			return skill;
		}

		private static StaffingThresholds createStaffingThresholds()
		{
			return new StaffingThresholds(new Percent(-0.3), new Percent(-0.1), new Percent(0.1));
		}

		private static IActivity createActivity(string name = "activity1")
		{
			var activity = ActivityFactory.CreateActivity(name);
			activity.RequiresSkill = true;
			return activity;
		}

		private IPersonAssignment createAssignment(IPerson person, DateTimePeriod dateTimePeriod,
			params IActivity[] activities)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				Scenario.Current(),
				dateTimePeriod,
				ShiftCategoryFactory.CreateShiftCategory(), activities);
			PersonAssignmentRepository.Has(assignment);

			return assignment;
		}

		private void setupIntradayStaffingSkillFor24Hours(ISkill skill, DateOnly date, double forecastedStaffing,
			double scheduledStaffing)
		{
			var forecastedStaffingList = new List<double>();
			var scheduledStaffingList = new List<double>();
			var timePeriodsList = new List<TimePeriod>();

			var start = TimeSpan.Zero;
			while (date.Date.Add(start) < date.Date.AddDays(1).Subtract(TimeSpan.FromSeconds(1)))
			{
				forecastedStaffingList.Add(forecastedStaffing);
				scheduledStaffingList.Add(scheduledStaffing);
				timePeriodsList.Add(new TimePeriod(start, start.Add(TimeSpan.FromMinutes(15))));

				start += TimeSpan.FromMinutes(15);
			}

			var skillStaffingDataList = new List<SkillStaffingData>
			{
				new SkillStaffingData
				{
					Date = date,
					ForecastedStaffing = forecastedStaffingList,
					ScheduledStaffing = scheduledStaffingList,
					TimePeriods = timePeriodsList
				}
			};

			setupStaffingForSkill(skill, skillStaffingDataList);
		}

		private void setupStaffingForSkill(ISkill skill, List<SkillStaffingData> staffingDataList)
		{
			var staffingPeriodDataList = new List<StaffingPeriodData>();
			staffingDataList.ForEach(staffingData =>
			{
				for (var i = 0; i < staffingData.ForecastedStaffing.Count; i++)
				{
					var staffingPeriodData = new StaffingPeriodData
					{
						ForecastedStaffing = staffingData.ForecastedStaffing[i],
						ScheduledStaffing = staffingData.ScheduledStaffing[i],
						Period = new DateTimePeriod(staffingData.Date.Utc().Add(staffingData.TimePeriods[i].StartTime),
							staffingData.Date.Utc().Date.Add(staffingData.TimePeriods[i].EndTime))
					};
					staffingPeriodDataList.Add(staffingPeriodData);
				}

				SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(skill, staffingData.Date,
					staffingPeriodDataList,
					_loggedUser.PermissionInformation.DefaultTimeZone());

				staffingPeriodDataList.Clear();
			});
		}

		class SkillStaffingData
		{
			public DateOnly Date;
			public List<double> ForecastedStaffing { get; set; }
			public List<double> ScheduledStaffing { get; set; }
			public List<TimePeriod> TimePeriods { get; set; }
		}
	}
}