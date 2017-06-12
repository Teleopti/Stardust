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
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
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
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[Toggle(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
	[Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_xx)]
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
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
			system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			system.UseTestDouble<FakeSkillCombinationResourceRepository>().For<ISkillCombinationResourceRepository>();
			system.UseTestDouble<FakeScheduleForecastSkillReadModelRepository>().For<IScheduleForecastSkillReadModelRepository>();
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			system.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();
		}

		[Test, SetCulture("en-US")]
		public void ShouldNotReturnAbsencePossibiliesForDaysNotInAbsenceOpenPeriod()
		{
			setupSiteOpenHour();
			setupTestDataForOneSkill();
			setupWorkFlowControlSet();
			var absenceRequestOpenDatePeriod =
				(AbsenceRequestOpenDatePeriod)User.CurrentUser().WorkflowControlSet.AbsenceRequestOpenPeriods[0];
			absenceRequestOpenDatePeriod.Period =
				absenceRequestOpenDatePeriod.OpenForRequestsPeriod =
					new DateOnlyPeriod(Now.ServerDate_DontUse().AddDays(6), Now.ServerDate_DontUse().AddDays(7));

			var result =
				Target.GetIntradayAbsencePossibility(Now.ServerDate_DontUse().AddDays(7), StaffingPossiblityType.Absence)
					.ToList();
			result.Count.Should().Be.EqualTo(4);
		}

		[Test, SetCulture("en-US")]
		public void ShouldReturnPossibiliesForCurrentWeek()
		{
			setupSiteOpenHour();
			setupTestDataForOneSkill();
			setupWorkFlowControlSet();
			var result = Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Absence).ToList();
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
			setupTestDataForOneSkill();
			setupWorkFlowControlSet();
			var result = Target.GetIntradayAbsencePossibility(Now.ServerDate_DontUse().AddWeeks(1), StaffingPossiblityType.Absence).ToList();
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
			setupTestDataForOneSkill();
			setupWorkFlowControlSet();
			var result = Target.GetIntradayAbsencePossibility(Now.ServerDate_DontUse().AddDays(-1), StaffingPossiblityType.Absence).ToList();
			result.Count.Should().Be.EqualTo(6);
		}

		[Test, SetCulture("en-US")]
		public void ShouldNotReturnPossibiliesForFarFutureDays()
		{
			setupSiteOpenHour();
			setupTestDataForOneSkill();
			setupWorkFlowControlSet();
			var result = Target.GetIntradayAbsencePossibility(Now.ServerDate_DontUse().AddWeeks(3), StaffingPossiblityType.Absence);
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnEmptyPossibilities()
		{
			var possibilities = Target.GetIntradayAbsencePossibility(null).ToList();
			Assert.AreEqual(0, possibilities.Count);
		}

		[Test]
		public void ShouldGetPossibilitiesOnlyForSkillsInSchedule()
		{
			setupSiteOpenHour();
			var person = User.CurrentUser();
			var activity1 = createActivity();
			var personSkill1 = createPersonSkill(activity1);
			setPersonSkill(personSkill1, new double?[] { 10d, 10d }, new double?[] { 12d, 11d });

			var activity2 = createActivity();
			var personSkill2 = createPersonSkill(activity2);
			setPersonSkill(personSkill2, new double?[] { 10d, 10d }, new double?[] { 6d, 12d });

			var personPeriod = createPersonPeriod(personSkill1, personSkill2);
			person.AddPersonPeriod(personPeriod);

			createAssignment(person, activity2);

			setupWorkFlowControlSet();
			var possibilities =
				Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat()).ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetPossibilitiesWhenUsingShrinkageValidator()
		{
			setupSiteOpenHour();
			setupTestDataForOneSkill(useShrinkage: true);
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
				Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
		}

		[Test]
		public void ShouldGetPossibilitiesWhenSomeStaffingDataIsNotAvailable()
		{
			setupSiteOpenHour();
			setupTestDataForOneSkill(new double?[] { 10d }, new double?[] { 11d, 11d });
			setupWorkFlowControlSet();
			var possibilities =
				Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Absence)
					.Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(1, possibilities.Count);
		}

		[Test]
		public void ShouldGetPossibilitiesWithoutIntradaySchedule()
		{
			setupSiteOpenHour();
			setupTestDataForOneSkill();
			setupWorkFlowControlSet();
			ScheduleData.Clear();
			var possibilities = Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime).Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
		}


		[Test]
		public void ShouldNotGetPossibilitiesForNotSupportedSkill()
		{
			setupSiteOpenHour();
			setupTestDataForOneSkill();
			setupWorkFlowControlSet();
			SkillRepository.LoadAllSkills().First().SkillType.Description = new Description("NotSupportedSkillType");
			var possibilities = Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime).ToList();
			Assert.AreEqual(0, possibilities.Count);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForAbsenceWhenUnderstaffing()
		{
			setupSiteOpenHour();
			setupTestDataForOneSkill();
			setupWorkFlowControlSet();
			var possibilities = Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Absence).Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetGoodPossibilitiesForAbsenceWhenNotUnderstaffing()
		{
			setupSiteOpenHour();
			setupTestDataForOneSkill(new double?[] { 10d, 10d }, new double?[] { 11d, 11d });
			setupWorkFlowControlSet();
			var possibilities = Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Absence).Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
					.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairAndGoodPossibilitiesForAbsence()
		{
			setupSiteOpenHour();
			setupTestDataForOneSkill(new double?[] { 10d, 10d }, new double?[] { 8d, 11d });
			setupWorkFlowControlSet();
			var possibilities = Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Absence).Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
				.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForAbsenceWhenOneOfSkillIsUnderstaffing()
		{
			setupSiteOpenHour();
			var person = User.CurrentUser();
			var activity1 = createActivity();
			var personSkill1 = createPersonSkill(activity1);
			setPersonSkill(personSkill1, new double?[] { 10d, 10d }, new double?[] { 8d, 11d });

			var activity2 = createActivity();
			var personSkill2 = createPersonSkill(activity2);
			setPersonSkill(personSkill2, new double?[] { 10d, 10d }, new double?[] { 11d, 8d });

			var personPeriod = createPersonPeriod(personSkill1, personSkill2);
			person.AddPersonPeriod(personPeriod);

			createAssignment(person, activity1, activity2);
			setupWorkFlowControlSet();
			var possibilities = Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Absence).Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
				.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForOvertimeWhenOverstaffing()
		{
			setupSiteOpenHour();
			setupTestDataForOneSkill(new double?[] { 10d, 10d }, new double?[] { 12d, 12d });
			var possibilities = Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime).Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
				.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetGoodPossibilitiesForOvertimeWhenNotOverstaffing()
		{
			setupSiteOpenHour();
			setupTestDataForOneSkill(new double?[] { 10d, 10d }, new double?[] { 6d, 6d });
			var possibilities = Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime).Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
			.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(1, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairAndGoodPossibilitiesForOvertime()
		{
			setupSiteOpenHour();
			setupTestDataForOneSkill(new double?[] { 10d, 10d }, new double?[] { 7d, 6d });
			setupWorkFlowControlSet();
			var possibilities = Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime).Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
			.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(1, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetFairPossibilitiesForOvertimeWhenOneOfSkillIsOverstaffing()
		{
			setupSiteOpenHour();
			var person = User.CurrentUser();
			var activity1 = createActivity();
			var personSkill1 = createPersonSkill(activity1);
			setPersonSkill(personSkill1, new double?[] { 10d, 10d }, new double?[] { 12d, 11d });

			var activity2 = createActivity();
			var personSkill2 = createPersonSkill(activity2);
			setPersonSkill(personSkill2, new double?[] { 10d, 10d }, new double?[] { 11d, 12d });

			var personPeriod = createPersonPeriod(personSkill1, personSkill2);
			person.AddPersonPeriod(personPeriod);

			createAssignment(person, activity1, activity2);

			var possibilities = Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime).Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
			.ToList();
			Assert.AreEqual(2, possibilities.Count);
			Assert.AreEqual(0, possibilities.ElementAt(0).Possibility);
			Assert.AreEqual(0, possibilities.ElementAt(1).Possibility);
		}

		[Test]
		public void ShouldGetPossibilitiesForOvertimeWithDayOff()
		{
			setupSiteOpenHour();
			setupTestDataForOneSkill();
			ScheduleData.Clear();
			var person = User.CurrentUser();
			var assignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, Scenario.Current(),
				Now.ServerDate_DontUse(), new DayOffTemplate());
			ScheduleData.Set(new List<IScheduleData> { assignment });
			var possibilities = Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Overtime).Where(d => d.Date == Now.ServerDate_DontUse().ToFixedClientDateOnlyFormat())
			.ToList();
			Assert.AreEqual(2, possibilities.Count);
		}

		[Test]
		public void ShouldOnlyGetOneDayDataIfReturnOneWeekDataIsFalse()
		{
			setupSiteOpenHour();
			setupTestDataForOneSkill();
			setupWorkFlowControlSet();
			var result = Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Absence, false).ToList();
			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldGetPossibilitiesAccordingToAgentTimeZone()
		{
			Now.Is(Now.UtcDateTime().Date.AddHours(2));

			setupSiteOpenHour();
			setupTestDataForOneSkill();
			setupWorkFlowControlSet();

			var timeZoneInfo = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(timeZoneInfo);

			var result = Target.GetIntradayAbsencePossibility(null, StaffingPossiblityType.Absence).ToList();
			var today = new DateOnly(TimeZoneHelper.ConvertFromUtc(Now.UtcDateTime(), User.CurrentUser().PermissionInformation.DefaultTimeZone()));
			result.FirstOrDefault()?.Date.Should().Be(today.ToFixedClientDateOnlyFormat());
			result.Count.Should().Be.EqualTo(8);
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
			var timePeriod = new TimePeriod(8, 0, 17, 0);
			var team = TeamFactory.CreateTeam("team1", "site1");
			team.Site.AddOpenHour(new SiteOpenHour
			{
				TimePeriod = timePeriod,
				IsClosed = false,
				WeekDay = DayOfWeek.Thursday
			});
			User.CurrentUser()
				.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(Now.ServerDate_DontUse(),
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

		private void setPersonSkill(IPersonSkill personSkill, double?[] forecastedStaffing, double?[] scheduledStaffing)
		{
			addPersonSkillsToPersonPeriod(User.CurrentUser(), personSkill);
			personSkill.Skill.StaffingThresholds = createStaffingThresholds();
			setupIntradayStaffingForSkill(getAvailablePeriod(), personSkill.Skill, forecastedStaffing,
				scheduledStaffing);
		}

		private PersonPeriod createPersonPeriod(params IPersonSkill[] personSkills)
		{
			var personContract = PersonContractFactory.CreatePersonContract();
			var team = TeamFactory.CreateSimpleTeam();
			var personPeriod = new PersonPeriod(Now.ServerDate_DontUse(), personContract, team);
			foreach (var personSkill in personSkills)
			{
				personPeriod.AddPersonSkill(personSkill);
			}
			return personPeriod;
		}

		private DateOnlyPeriod getAvailablePeriod()
		{
			var today = Now.ServerDate_DontUse();
			var period = new DateOnlyPeriod(today, today.AddDays(13)).Inflate(1);
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
			var personPeriod = person.PersonPeriods(Now.ServerDate_DontUse().ToDateOnlyPeriod()).First();
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
				SkillDayRepository.Has(skill.CreateSkillDayWithDemandOnInterval(Scenario.Current(), day, 0, timePeriodTuples.ToArray()));
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
