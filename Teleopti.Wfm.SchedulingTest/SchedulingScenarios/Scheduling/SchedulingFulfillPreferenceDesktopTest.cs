using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
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
	public class SchedulingFulfillPreferenceDesktopTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;

		[Test]
		public void ShouldNotScheduleWithoutPreferences()
		{
			var date = new DateOnly(2015, 10, 12);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).WithId().InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc)
				.WithPersonPeriod(ruleSet, skill)
				.WithSchedulePeriodOneWeek(date);
			var skillDays = skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 1);
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = new ShiftCategory()};
			var prefDay = new PreferenceDay(agent, date, preferenceRestriction);
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] {agent}, new[]{prefDay}, skillDays);
			var schedulingOptions = new SchedulingOptions
			{
				UsePreferences = true
			};

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] {agent}, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1));

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment(true).ShiftLayers.Any()
				.Should().Be.False();
		}
		
		[Test]
		public void SchedulePreferencesOnlyShouldNotAddOtherDaysOff()
		{
			var date = new DateOnly(2015, 10, 12);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).WithId().InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc)
				.WithPersonPeriod(ruleSet,new ContractScheduleWorkingMondayToFriday(),skill)
				.WithSchedulePeriodOneWeek(date);
			var skillDays = skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 1);
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = shiftCategory};
			var prefDay = new PreferenceDay(agent, date, preferenceRestriction);
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] {agent}, new[]{prefDay}, skillDays);
			var schedulingOptions = new SchedulingOptions
			{
				UsePreferences = true,
				PreferencesDaysOnly = true
			};

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] {agent}, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1));

			stateHolder.Schedules[agent].ScheduledDay(date.AddDays(6)).HasDayOff().Should().Be.False();
		}
		
		[Test]
		public void ScheduleMustHavesOnlyShouldNotAddOtherDaysOff()
		{
			var date = new DateOnly(2015, 10, 12);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).WithId().InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc)
				.WithPersonPeriod(ruleSet,new ContractScheduleWorkingMondayToFriday(),skill)
				.WithSchedulePeriodOneWeek(date);
			var skillDays = skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 1);
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = shiftCategory, MustHave = true};
			var prefDay = new PreferenceDay(agent, date, preferenceRestriction);
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] {agent}, new[]{prefDay}, skillDays);
			var schedulingOptions = new SchedulingOptions
			{
				UsePreferences = true,
				UsePreferencesMustHaveOnly = true
			};

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] {agent}, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1));

			stateHolder.Schedules[agent].ScheduledDay(date.AddDays(6)).HasDayOff().Should().Be.False();
		}
		
		[Test]
		public void ScheduleMustHavesOnlyShouldNotAddOtherDaysOffOnPreference()
		{
			var date = new DateOnly(2015, 10, 12);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).WithId().InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc)
				.WithPersonPeriod(ruleSet,new ContractScheduleWorkingMondayToFriday(),skill)
				.WithSchedulePeriodOneWeek(date);
			var skillDays = skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 1);
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = shiftCategory};
			var prefDayOnSaturday = new PreferenceDay(agent, date.AddDays(6), preferenceRestriction);
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] {agent}, new[]{prefDayOnSaturday}, skillDays);
			var schedulingOptions = new SchedulingOptions
			{
				UsePreferences = true,
				UsePreferencesMustHaveOnly = true
			};

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] {agent}, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1));

			stateHolder.Schedules[agent].ScheduledDay(date.AddDays(6)).HasDayOff().Should().Be.False();
		}
		
		[Test]
		public void ShouldNotRemoveDaysOffIfScheduledUsingPreferenceDayOff()
		{
			var date = new DateOnly(2015, 10, 12);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).WithId().InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc)
				.WithPersonPeriod(ruleSet,new ContractScheduleWorkingMondayToFriday(),skill)
				.WithSchedulePeriodOneWeek(date);
			var skillDays = skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 1);
			var preferenceRestriction = new PreferenceRestriction {DayOffTemplate = new DayOffTemplate()};
			var prefDay = new PreferenceDay(agent, date.AddDays(6), preferenceRestriction);
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] {agent}, new[]{prefDay}, skillDays);
			var schedulingOptions = new SchedulingOptions
			{
				UsePreferences = true,
				PreferencesDaysOnly = true
			};

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] {agent}, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1));

			stateHolder.Schedules[agent].ScheduledDay(date.AddDays(6)).HasDayOff().Should().Be.True();
		}
		
		[Test]
		public void ShouldNotRemoveDaysOffIfItWasThereBeforeScheduling()
		{
			var date = new DateOnly(2015, 10, 12);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).WithId().InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));

			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc)
				.WithPersonPeriod(ruleSet,new ContractScheduleWorkingMondayToFriday(),skill)
				.WithSchedulePeriodOneWeek(date);
			var skillDays = skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 1);
			var assignment = new PersonAssignment(agent,scenario,date).WithDayOff();
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] {agent}, new[]{assignment}, skillDays);
			var schedulingOptions = new SchedulingOptions
			{
				UsePreferences = true,
				PreferencesDaysOnly = true
			};

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] {agent}, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1));

			stateHolder.Schedules[agent].ScheduledDay(date).HasDayOff().Should().Be.True();
		}

		public SchedulingFulfillPreferenceDesktopTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}