using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.DayOffOptimization
{
	[DomainTest]
	[UseIocForFatClient]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	public class DayOffOptimizationCascadingDesktopTest : DayOffOptimizationScenario
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public DayOffOptimizationDesktop Target;
		public ResourceCalculateWithNewContext ResourceCalculation;

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldBaseMoveOnNonShoveledResourceCalculation_BasedOnAndersCase(bool beInResourceCalculatedStateAtStartup)
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var skillA = new Skill("A").For(activity).WithId().CascadingIndex(1).IsOpenBetween(8, 16);
			var skillB = new Skill("B").For(activity).WithId().CascadingIndex(2).IsOpenBetween(8, 16);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var team = new Team { Site = new Site("_") };
			var agents = new List<IPerson>();
			for (var i = 0; i < 2; i++)
			{
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skillA, skillB).WithSchedulePeriodOneWeek(firstDay);
				agent.SchedulePeriod(firstDay).SetDaysOff(2);
				agents.Add(agent);
			}
			var skillDaysA = skillA.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 2, 2, 2, 1, 1, 1, 1);
			var skillDaysB = skillB.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 2, 2, 1, 1);
			var asses = new List<IPersonAssignment>();
			foreach (var agent in agents)
			{
				for (var i = 0; i < 7; i++)
				{
					var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(8, 16));
					if (i == 5 || i == 6) //saturday or sunday
					{
						ass.SetDayOff(new DayOffTemplate());
					}
					asses.Add(ass);
				}
			}
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, agents, asses, skillDaysA.Union(skillDaysB));
			var optPrefs = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag() } };
			if(beInResourceCalculatedStateAtStartup)
				ResourceCalculation.ResourceCalculate(period, new ResourceCalculationData(stateHolder.SchedulingResultState, false, false));

			Target.Execute(period, agents, optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoOptimizationCallback());

			agents.Count(agent => stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(3)).HasDayOff()).Should().Be.EqualTo(1);
			agents.Count(agent => stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(4)).HasDayOff()).Should().Be.EqualTo(1);
			agents.Count(agent => stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff()).Should().Be.EqualTo(1);
			agents.Count(agent => stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(6)).HasDayOff()).Should().Be.EqualTo(1);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldBaseMoveOnNonShoveledResourceCalculation(bool beInResourceCalculatedStateAtStartup)
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var skillA = new Skill("A").For(activity).WithId().CascadingIndex(1).IsOpenBetween(8, 16);
			var skillB = new Skill("B").For(activity).WithId().CascadingIndex(2).IsOpenBetween(8, 16);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var team = new Team { Site = new Site("_") };
			var agents = new List<IPerson>();
			for (var i = 0; i < 2; i++)
			{
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skillA, skillB).WithSchedulePeriodOneWeek(firstDay);
				agent.SchedulePeriod(firstDay).SetDaysOff(1);
				agents.Add(agent);
			}
			var skillDaysA = skillA.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
			 0.5, 0.4, 0, 20, 20, 20, 20);
			var skillDaysB = skillB.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
			 0, 20, 20, 20, 20, 20, 20);
			var asses = new List<IPersonAssignment>();
			foreach (var agent in agents)
			{
				for (var i = 0; i < 7; i++)
				{
					var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(i));
					ass.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
					ass.SetShiftCategory(shiftCategory);
					if (i == 5) //saturday
					{
						ass.SetDayOff(new DayOffTemplate());
					}
					asses.Add(ass);
				}
			}
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, agents, asses, skillDaysA.Union(skillDaysB));
			var optPrefs = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag() } };
			if (beInResourceCalculatedStateAtStartup)
				ResourceCalculation.ResourceCalculate(period, new ResourceCalculationData(stateHolder.SchedulingResultState, false, false));

			Target.Execute(period, agents, optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoOptimizationCallback());

			agents.Count(agent => stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(0)).HasDayOff()).Should().Be.EqualTo(1); 
			agents.Count(agent => stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(1)).HasDayOff()).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldBeInShoveledStateAfterDoOpt()
		{
			var date = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var activity = new Activity("_");
			var skillA = new Skill("A").For(activity).WithId().CascadingIndex(1).IsOpenBetween(8, 16);
			var skillB = new Skill("B").For(activity).WithId().CascadingIndex(2).IsOpenBetween(8, 16);
			var scenario = new Scenario("_");
			var agent1 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB).WithSchedulePeriodOneDay(date);
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skillA, skillB).WithSchedulePeriodOneDay(date);
			var ass1 = new PersonAssignment(agent1, scenario, date).WithLayer(activity, new TimePeriod(8, 16));
			var ass2 = new PersonAssignment(agent2, scenario, date).WithLayer(activity, new TimePeriod(8, 16));
			var skillDayA = skillA.CreateSkillDayWithDemand(scenario, date, 1);
			var skillDayB = skillB.CreateSkillDayWithDemand(scenario, date, 1);
			SchedulerStateHolder.Fill(scenario, period, new[]{agent1, agent2}, new []{ass1, ass2}, new[]{skillDayA, skillDayB});

			Target.Execute(period, new[]{agent1, agent2}, new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag() } }, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new NoOptimizationCallback());
			
			skillDayA.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(1); 
			skillDayB.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(1); 
		}

		public DayOffOptimizationCascadingDesktopTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}