using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[MyTimeWebTest, Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
	[Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_xx)]
	public class ScheduleApiControllerPossibiliesTest : ISetup
	{
		public ScheduleApiController Target;
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
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
			system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			system.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
			system.UseTestDouble<FakeScheduleForecastSkillReadModelRepository>().For<IScheduleForecastSkillReadModelRepository>();
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			system.UseTestDouble<FakeIntervalLengthFetcher>().For<IIntervalLengthFetcher>();
			system.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldReturnPossibiliesForCurrentWeek()
		{
			setup();
			var result = Target.FetchData(null, StaffingPossiblityType.Absence).Possibilities;
			result.Count(r => r.Date >= Now.LocalDateOnly()).Should().Be.EqualTo(6);
		}

		private void setup()
		{
			setupIntervalLength();
			setupSiteOpenHour();
			setupTestDataForOneSkill();
		}

		private void setupIntervalLength()
		{
			IntervalLengthFetcher.Has(15);
		}

		private void setupSiteOpenHour()
		{
			var timePeriod = new TimePeriod(8, 0, 17, 0);
			var team = TeamFactory.CreateTeam("team1", "site1");
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
			User.CurrentUser()
				.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(Now.LocalDateOnly(),
					PersonContractFactory.CreatePersonContract(), team));
		}

		private void setupTestDataForOneSkill(double?[] forecastedStaffing = null, double?[] scheduledStaffing = null, bool useShrinkage = false)
		{
			var person = User.CurrentUser();
			var activity = createActivity();
			createAssignment(person, activity);
			setPersonSkill(person, activity, forecastedStaffing ?? new double?[] { 10d, 10d },
				scheduledStaffing ?? new double?[] { 8d, 8d });
		}

		private void setPersonSkill(IPerson person, IActivity activity, double?[] forecastedStaffing,
			double?[] scheduledStaffing)
		{
			var personSkill = createPersonSkill(activity);
			addPersonSkillsToPersonPeriod(person, personSkill);
			personSkill.Skill.StaffingThresholds = createStaffingThresholds();
			setupIntradayStaffingForSkill(getAvailablePeriod(), personSkill.Skill, forecastedStaffing,
				scheduledStaffing);
		}

		private DateOnlyPeriod getAvailablePeriod()
		{
			var today = Now.LocalDateOnly();
			var period = new DateOnlyPeriod(today, today.AddDays(13));
			return period;
		}

		private static StaffingThresholds createStaffingThresholds()
		{
			return new StaffingThresholds(new Percent(-0.3), new Percent(-0.1), new Percent(0.1));
		}

		private IPersonSkill createPersonSkill(IActivity activity)
		{
			var skill = SkillFactory.CreateSkill("test1").WithId();
			skill.SkillType.Description = new Description("SkillTypeInboundTelephony");
			skill.Activity = activity;
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(8, 00, 9, 30));
			SkillRepository.Has(skill);

			var personSkill = PersonSkillFactory.CreatePersonSkill(skill, 1);
			return personSkill;
		}

		private void addPersonSkillsToPersonPeriod(IPerson person, params IPersonSkill[] personSkills)
		{
			var personPeriod = person.PersonPeriods(Now.LocalDateOnly().ToDateOnlyPeriod()).First();
			foreach (var personSkill in personSkills)
			{
				((PersonPeriod)personPeriod).AddPersonSkill(personSkill);
			}
		}

		private static IActivity createActivity()
		{
			var activity = ActivityFactory.CreateActivity("activity1");
			activity.RequiresSkill = true;
			return activity;
		}

		private void setupIntradayStaffingForSkill(DateOnlyPeriod period, ISkill skill, double?[] forecastedStaffings,
			double?[] scheduledStaffings)
		{
			period.DayCollection().ToList().ForEach(day =>
			{
				var utcDate = TimeZoneHelper.ConvertToUtc(day.Date,
					User.CurrentUser().PermissionInformation.DefaultTimeZone());
				var intervals = new[] { utcDate.AddHours(8).AddMinutes(15), utcDate.AddHours(8).AddMinutes(30) };
				for (var i = 0; i < scheduledStaffings.Length; i++)
				{
					CombinationRepository.AddSkillCombinationResource(new DateTime(),
						new[]
						{
							new SkillCombinationResource
							{
								StartDateTime = intervals[i],
								EndDateTime = intervals[i].AddMinutes(15),
								Resource = scheduledStaffings[i].Value,
								SkillCombination = new[] {skill.Id.Value}
							}
						});
				}

				var skillStaffingIntervals = new List<SkillStaffingInterval>();
				var timePeriodTuples = new List<Tuple<TimePeriod, double>>();
				for (var i = 0; i < forecastedStaffings.Length; i++)
				{
					skillStaffingIntervals.Add(new SkillStaffingInterval
					{
						StartDateTime = intervals[i],
						EndDateTime = intervals[i].AddMinutes(15),
						FStaff = forecastedStaffings[i].Value,
						SkillId = skill.Id.GetValueOrDefault()
					});
					timePeriodTuples.Add(new Tuple<TimePeriod, double>(
						new TimePeriod(intervals[i].TimeOfDay, intervals[i].TimeOfDay.Add(TimeSpan.FromMinutes(15))),
						forecastedStaffings[i].Value));
				}

				ScheduleForecastSkillReadModelRepository.Persist(skillStaffingIntervals, new DateTime());
				SkillDayRepository.Has(skill.CreateSkillDayWithDemandOnInterval(Scenario.Current(), day, 0,
					timePeriodTuples.ToArray()));
			});

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
	}
}
