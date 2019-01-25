using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	[UseIocForFatClient]
	public class IntradayOptimizationRestrictionsDesktopTest : IntradayOptimizationScenarioTest
	{
		public OptimizationDesktopExecuter Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		
		[Test]
		public void ShouldConsiderRotations()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var restriction = new RotationRestriction {ShiftCategory = shiftCategory};
			var personRestriction = new ScheduleDataRestriction(agent, restriction, dateOnly);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass, personRestriction }, skillDay);
			var optimizationPreferences = new OptimizationPreferences
			{
				General = {ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true, UseRotations = true, RotationsValue = 1.0d}
			};

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, dateOnly.ToDateOnlyPeriod(), optimizationPreferences, null);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldConsiderAvailabilities()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var restriction = new AvailabilityRestriction {NotAvailable = true};
			var personRestriction = new ScheduleDataRestriction(agent, restriction, dateOnly);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass, personRestriction }, skillDay);
			var optimizationPreferences = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true, UseAvailabilities = true, AvailabilitiesValue = 1.0d }
			};

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, dateOnly.ToDateOnlyPeriod(), optimizationPreferences, null);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldConsiderPreferences()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), new ShiftCategory("_").WithId()));
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(17, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var preferenceRestriction = new PreferenceRestriction {ShiftCategory = shiftCategory};
			var preferenceDay = new PreferenceDay(agent, dateOnly, preferenceRestriction);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass, preferenceDay }, skillDay);
			var optimizationPreferences = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true, UsePreferences = true, PreferencesValue = 1.0d }
			};

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, dateOnly.ToDateOnlyPeriod(), optimizationPreferences, null);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Minute.Should().Be.EqualTo(0);
		}
		
		[Test]
		public void ShouldRespectStartTimeTolerance_TakeAnyShiftWhenNoPreference()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(21, 0, 21, 0, 15), new ShiftCategory("_").WithId()));
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(19, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass }, skillDay);
			var optimizationPreferences = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true, UsePreferences = true, PreferencesValue = 1.0d },
			};

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, dateOnly.ToDateOnlyPeriod(), optimizationPreferences, null);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Hour.Should().Be.EqualTo(12);
		}
		
		[Test]
		public void ShouldRespectStartTimeTolerance_TakeAnyShiftWhenNoSpecifiedTolerance()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(12, 0, 12, 0, 15), new TimePeriodWithSegment(21, 0, 21, 0, 15), new ShiftCategory("_").WithId()));
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new Tuple<int, TimeSpan>(19, TimeSpan.FromMinutes(360)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(shiftCategory).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var preferenceRestriction = new PreferenceRestriction {StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8))};
			var preferenceDay = new PreferenceDay(agent, dateOnly, preferenceRestriction);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass, preferenceDay }, skillDay);
			var optimizationPreferences = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true, UsePreferences = true, PreferencesValue = 0d }
			};

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, dateOnly.ToDateOnlyPeriod(), optimizationPreferences, null);

			stateHolder.Schedules[agent].ScheduledDay(dateOnly).PersonAssignment().Period.StartDateTime.Hour.Should().Be.EqualTo(12);
		}
		
		[TestCase(1, ExpectedResult = 0)]
		[TestCase(0.6, ExpectedResult = 2)]
		[TestCase(0, ExpectedResult = 5)]
		public int ShouldRespectStartTimeTolerance_PreferencesValue(double preferenceValue)
		{
			var scenario = new Scenario();
			var activity = new Activity();
			var shiftCategory = new ShiftCategory("_").WithId();
			var date = new DateOnly(2017, 9, 25);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 7, 0, 60), new TimePeriodWithSegment(15, 0, 15, 0, 60), shiftCategory));
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(date);
			var scheduleData = new List<IScheduleData>();
			var skillDays = new List<ISkillDay>();
			for (var i = 0; i < 5; i++)
			{
				scheduleData.Add(new PersonAssignment(agent, scenario, date.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16)));
				skillDays.Add(skill.CreateSkillDayWithDemandPerHour(scenario, date.AddDays(i), TimeSpan.FromMinutes(1), new Tuple<int, TimeSpan>(7, TimeSpan.FromMinutes(360))));
				scheduleData.Add(new PreferenceDay(agent, date.AddDays(i), new PreferenceRestriction { StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8))}));
			}
			scheduleData.Add(new PersonAssignment(agent, scenario, date.AddDays(5)).WithDayOff());
			scheduleData.Add(new PersonAssignment(agent, scenario, date.AddDays(6)).WithDayOff());
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, scheduleData, skillDays);
			var optimizationPreferences = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true, UsePreferences = true, PreferencesValue = preferenceValue },
			};

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, period, optimizationPreferences, null);

			return stateHolder.Schedules.SchedulesForPeriod(period, agent).Count(x => x.PersonAssignment().Period.StartDateTime.Hour == 7);
		}
		
		[Test]
		public void ShouldRespectStartTimeTolerance_NotCrashWhenUsingActivityRestriction()
		{
			var scenario = new Scenario("_");
			var phoneActivity = ActivityFactory.CreateActivity("_");
			var dateOnly = new DateOnly(2010, 1, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(9, 0, 9, 0, 15), new TimePeriodWithSegment(18, 0, 18, 0, 15), new ShiftCategory("_").WithId()));
			var skill = new Skill("_").For(phoneActivity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();	
			var skillDay = skill.CreateSkillDayWithDemandPerHour(scenario, dateOnly, TimeSpan.FromMinutes(60), new List<Tuple<int, TimeSpan>>());
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(dateOnly);
			var ass = new PersonAssignment(agent, scenario, dateOnly).ShiftCategory(new ShiftCategory("_").WithId()).WithLayer(phoneActivity, new TimePeriod(8, 17));
			var preferenceRestriction = new PreferenceRestriction {StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8))};
			preferenceRestriction.AddActivityRestriction(new ActivityRestriction(phoneActivity));
			var preferenceDay = new PreferenceDay(agent, dateOnly, preferenceRestriction);
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly, new[] { agent }, new IScheduleData[] { ass, preferenceDay }, skillDay);
			var optimizationPreferences = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepShiftsWithinDay = true, UsePreferences = true, PreferencesValue = 1d },
			};

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, dateOnly.ToDateOnlyPeriod(), optimizationPreferences, null);
			});
		}
	}
}