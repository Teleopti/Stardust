using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class SchedulingRestrictionsDesktopTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldNotScheduleShiftsForRestrictionsOnlyWhenNoRestrictionExists()
		{
			var date = new DateOnly(2017, 1, 10);
			var activity = new Activity("_").WithId();
			var otherActivity = new Activity("_").WithId();
			otherActivity.RequiresSkill = true;
			var scenario = new Scenario("_");
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 100);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId())) { OnlyForRestrictions = true };
			var rulsetToGetMinMaxWorkTimeFrom = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(otherActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId())); //so we get min max worktime from the ruleset bag when not having any availability restriction
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneDay(date);
			agent.Period(date).RuleSetBag.AddRuleSet(rulsetToGetMinMaxWorkTimeFrom);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(date, date), new[] { agent }, Enumerable.Empty<IPersonAssignment>(), new[] { skillDay });
			var schedulingOptions = new SchedulingOptions { UseAvailability = true };

			Target.Execute(new NoSchedulingCallback(),
				schedulingOptions,
				new NoSchedulingProgress(),
				schedulerStateHolder.SchedulingResultState.LoadedAgents.FixedStaffPeople(date.ToDateOnlyPeriod()), date.ToDateOnlyPeriod()
				);

			schedulerStateHolder.Schedules[agent].ScheduledDay(date).IsScheduled().Should().Be.False();
		}
		
		[Test]
		public void ShouldScheduleAbsencePreferences()
		{
			var date = new DateOnly(2017, 1, 10);
			var activity = new Activity().WithId();
			var scenario = new Scenario();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 100);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneDay(date);
			var absenceRestriction = new PreferenceRestriction {Absence = new Absence().WithId()};
			var preferenceDay = new PreferenceDay(agent, date, absenceRestriction).WithId();
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, date, agent, preferenceDay,skillDay);
			var schedulingOptions = new SchedulingOptions { UsePreferences = true };

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] {agent}, date.ToDateOnlyPeriod());

			schedulerStateHolder.Schedules[agent].ScheduledDay(date).PersonAbsenceCollection().Length
				.Should().Be.EqualTo(1);
		}
		
		[Test]
		public void ShouldScheduleFixedStaffWhenUsingHourlyAvailability()
		{
			var firstDay = new DateOnly(2017, 5, 15);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(firstDay);
			var hourlyAvailabilityDays = new List<IStudentAvailabilityDay>();
			for (var i = 0; i < 4; i++)
			{
				hourlyAvailabilityDays.Add(new StudentAvailabilityDay(agent, firstDay.AddDays(i), new[]
				{
					new StudentAvailabilityRestriction {
						StartTimeLimitation = new StartTimeLimitation(new TimeSpan(4, 0, 0), null),
						EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(21, 0, 0))
					}
				}));
			}
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 10, 10, 10, 10, 10, 10, 10);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, period, agent, hourlyAvailabilityDays, skillDays);
			var schedulingOptions = new SchedulingOptions { ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff, UseStudentAvailability = true };
			
			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, period);

			for (var i = 0; i < 7; i++)
			{
				if (i < 4)
					schedulerStateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(i)).PersonAssignment(true).ShiftLayers.Should().Not.Be.Empty();
				else
					schedulerStateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(i)).HasDayOff().Should().Be.True();
			}	
		}

		[Test]
		public void ShouldScheduleWhenPreferencesConflictWithContractDayOff()
		{
			var date = new DateOnly(2017, 1, 2);
			var period = new DateOnlyPeriod(date, date.AddDays(13));
			var activity = new Activity().WithId();
			var scenario = new Scenario();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var contractSchedule = new ContractSchedule("_");
			var contractScheduleWeek = new ContractScheduleWeek();
			contractScheduleWeek.Add(DayOfWeek.Monday, true);
			contractScheduleWeek.Add(DayOfWeek.Tuesday, true);
			contractScheduleWeek.Add(DayOfWeek.Wednesday, true);
			contractScheduleWeek.Add(DayOfWeek.Thursday, true);
			contractScheduleWeek.Add(DayOfWeek.Friday, true);
			contractScheduleWeek.Add(DayOfWeek.Saturday, false);
			contractScheduleWeek.Add(DayOfWeek.Sunday, false);
			contractSchedule.AddContractScheduleWeek(contractScheduleWeek);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet,contractSchedule, skill).WithSchedulePeriodTwoWeeks(date);
			var shiftcategoryRestriction = new PreferenceRestriction { ShiftCategory = shiftCategory };
			var preferenceDays = new List<IPreferenceDay>();
			for (var i = 0; i < 6; i++)
			{
				preferenceDays.Add(new PreferenceDay(agent, date.AddDays(i), shiftcategoryRestriction).WithId());
			}	
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new []{agent}, preferenceDays, skillDays);
			var schedulingOptions = new SchedulingOptions { UsePreferences = true };

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, period);

			schedulerStateHolder.Schedules[agent].ScheduledDayCollection(period).ForEach(x => x.IsScheduled().Should().Be.True());
		}

		[TestCase(true, ExpectedResult = 7)]
		[TestCase(false, ExpectedResult = 8)]
		public int ShouldConsiderRotations(bool useRotations)
		{
			var activity = new Activity().WithId();
			var date = new DateOnly(2016, 10, 25);
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), shiftCategory));
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneDay(date);
			var alreadyScheduledAgent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skill);
			var alreadyScheduledAgentAss = new PersonAssignment(alreadyScheduledAgent, scenario, date).WithLayer(activity, new TimePeriod(7, 8));
			var rotationRestriction = new RotationRestriction { StartTimeLimitation = new StartTimeLimitation(new TimeSpan(7, 0, 0), new TimeSpan(7, 0, 0)) };
			var personRestriction = new ScheduleDataRestriction(agent, rotationRestriction, date);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, date, new[]{agent, alreadyScheduledAgent}, new IScheduleData[]{ personRestriction, alreadyScheduledAgentAss}, skillDay);
			var schedulingOptions = new SchedulingOptions { UseRotations = useRotations };

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, date.ToDateOnlyPeriod());
			
			return schedulerStateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment().Period.StartDateTime.TimeOfDay.Hours;
		}

		[TestCase(true, false, false, false)]
		[TestCase(false, true, false, false)]
		[TestCase(false, false, true, false)]
		[TestCase(false, false, false, true)]
		public void SolveNightlyRestWhiteSpotShouldConsiderRestrictionDaysOnly(bool onlyPrefrenceDays, bool onlyMustHavePrefrenceDays, bool onlyRotationDays, bool onlyAvailabilityDays)
		{
			var activity = new Activity().WithId();
			var date = new DateOnly(2016, 10, 25);
			var period = new DateOnlyPeriod(date, date.AddDays(6));
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), shiftCategory ));
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 5, 5, 5, 5, 5, 5, 5);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, new ContractWithMaximumTolerance(), skill).WithSchedulePeriodOneWeek(date);
			var ass = new PersonAssignment(agent, scenario, date.AddDays(4)).WithLayer(activity, new TimePeriod(7, 8)).ShiftCategory(shiftCategory);
			var dayOff = new PersonAssignment(agent, scenario, date.AddDays(6)).WithDayOff();
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new IScheduleData[] { ass, dayOff }, skillDays);
			var schedulingOptions = new SchedulingOptions
			{
				UsePreferences = true,
				UseRotations = true,
				UseAvailability = true,
				PreferencesDaysOnly = onlyPrefrenceDays,
				UsePreferencesMustHaveOnly = onlyMustHavePrefrenceDays,
				RotationDaysOnly = onlyRotationDays,
				AvailabilityDaysOnly = onlyAvailabilityDays
			};

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, period);

			schedulerStateHolder.Schedules[agent].ScheduledDay(date.AddDays(4)).IsScheduled().Should().Be.True();
		}

		[Test]
		public void ShouldRespectAbsencePrefrenceOnlyOnPreferenceDay()
		{
			var activity = new Activity().WithId();
			var date = new DateOnly(2016, 10, 25);
			var period = new DateOnlyPeriod(date, date.AddDays(6));
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), shiftCategory));
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 5, 5, 5, 5, 5, 5, 5);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, new ContractWithMaximumTolerance(), skill).WithSchedulePeriodOneWeek(date);
			var absenceRestriction = new PreferenceRestriction { Absence = new Absence().WithId() };
			var preferenceDay = new PreferenceDay(agent, date.AddDays(4), absenceRestriction).WithId();
			var dayOff = new PersonAssignment(agent, scenario, date.AddDays(6)).WithDayOff();
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new IScheduleData[] { preferenceDay, dayOff }, skillDays);
			var schedulingOptions = new SchedulingOptions
			{
				UsePreferences = true,
				PreferencesDaysOnly = true
			};

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, period);

			schedulerStateHolder.Schedules[agent].ScheduledDay(date.AddDays(4)).PersonAbsenceCollection().Length.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotCrashWhenSolvingNightRestWhiteSpotAndHavingConflictingRestrictions()
		{
			var activity = new Activity().WithId();
			var date = new DateOnly(2016, 10, 25);
			var period = new DateOnlyPeriod(date, date.AddDays(6));
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 8, 0, 60), new TimePeriodWithSegment(15, 0, 16, 0, 60), shiftCategory));
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 5, 5, 5, 5, 5, 5, 5);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, new ContractWithMaximumTolerance(), skill).WithSchedulePeriodOneWeek(date);
			var preferenceRestriction = new PreferenceRestriction { ShiftCategory = new ShiftCategory().WithId() };
			var preferenceDay = new PreferenceDay(agent, date.AddDays(3), preferenceRestriction).WithId();
			var rotationRestriction = new RotationRestriction {ShiftCategory = shiftCategory };
			var rotationDay = new ScheduleDataRestriction(agent, rotationRestriction, date.AddDays(3));
			var ass1 = new PersonAssignment(agent, scenario, date.AddDays(3)).WithLayer(activity, new TimePeriod(7, 8)).ShiftCategory(shiftCategory);
			var ass2 = new PersonAssignment(agent, scenario, date.AddDays(5)).WithLayer(activity, new TimePeriod(7, 8)).ShiftCategory(shiftCategory);
			var dayOff = new PersonAssignment(agent, scenario, date.AddDays(6)).WithDayOff();
			SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, new IScheduleData[] {ass1, ass2, rotationDay, preferenceDay, dayOff }, skillDays);
			var schedulingOptions = new SchedulingOptions
			{
				UsePreferences = true,
				PreferencesDaysOnly = true,
				UseRotations = true
			};

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, period);
			});
		}

		public SchedulingRestrictionsDesktopTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}
