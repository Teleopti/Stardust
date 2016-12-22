﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[TestFixture(false)]
	[TestFixture(true)]
	[DomainTest]
	public class DayOffOptimizationCascadingDesktopTest : DayOffOptimizationScenario
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public IDayOffOptimizationDesktop Target;

		public DayOffOptimizationCascadingDesktopTest(bool teamBlockDayOffForIndividuals) : base(teamBlockDayOffForIndividuals)
		{	
		}

		[Test]
		public void ShouldBaseMoveOnNonShoveledResourceCalculation_BasedOnAndersCase()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity = new Activity("_");
			var skillA = new Skill("A", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity }.WithId().WithCascadingIndex(1);
			var skillB = new Skill("B", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity }.WithId().WithCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 16, 0));
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 16, 0));
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var team = new Team { Site = new Site("_") };
			var agents = new List<IPerson>();
			for (var i = 0; i < 2; i++)
			{
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
				var personPeriod = new PersonPeriod(firstDay.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = new RuleSetBag(ruleSet) };
				personPeriod.AddPersonSkill(new PersonSkill(skillA, new Percent(1)));
				personPeriod.AddPersonSkill(new PersonSkill(skillB, new Percent(1)));
				agent.AddPersonPeriod(personPeriod);
				var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
				agent.AddSchedulePeriod(schedulePeriod);
				schedulePeriod.SetDaysOff(2); //thought this was the default one?
				agents.Add(agent);
			}
			var skillDaysA = skillA.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 2, 2, 2, 1, 1, 1, 1);
			var skillDaysB = skillB.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 2, 2, 1, 1);
			var asses = new List<IPersonAssignment>();
			foreach (var agent in agents)
			{
				for (var i = 0; i < 7; i++)
				{
					var ass = new PersonAssignment(agent, scenario, firstDay.AddDays(i));
					ass.AddActivity(activity, new TimePeriod(8, 0, 16, 0));
					ass.SetShiftCategory(shiftCategory);
					if (i == 5 || i == 6) //saturday or sunday
					{
						ass.SetDayOff(new DayOffTemplate());
					}
					asses.Add(ass);
				}
			}
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, agents, asses, skillDaysA.Union(skillDaysB));
			var optPrefs = new OptimizationPreferences { General = { ScheduleTag = new ScheduleTag() } };
			var scheduleDays = agents.SelectMany(agent => stateHolder.Schedules.SchedulesForPeriod(period, agent)).ToList();

			Target.Execute(period, scheduleDays, new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new GroupPageLight("_", GroupPageType.SingleAgent), () => new WorkShiftFinderResultHolder(), (o, args) => { });

			agents.Count(agent => stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(3)).HasDayOff()).Should().Be.EqualTo(1);
			agents.Count(agent => stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(4)).HasDayOff()).Should().Be.EqualTo(1);
			agents.Count(agent => stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(5)).HasDayOff()).Should().Be.EqualTo(1);
			agents.Count(agent => stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(6)).HasDayOff()).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldBaseMoveOnNonShoveledResourceCalculation()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = new DateOnlyPeriod(firstDay, firstDay.AddWeeks(1));
			var activity = new Activity("_");
			var skillA = new Skill("A", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity }.WithId().WithCascadingIndex(1);
			var skillB = new Skill("B", "_", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)) { Activity = activity }.WithId().WithCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithOpenHours(skillA, new TimePeriod(8, 0, 16, 0));
			WorkloadFactory.CreateWorkloadWithOpenHours(skillB, new TimePeriod(8, 0, 16, 0));
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var team = new Team { Site = new Site("_") };
			var agents = new List<IPerson>();
			for (var i = 0; i < 2; i++)
			{
				var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc);
				var personPeriod = new PersonPeriod(firstDay.AddWeeks(-1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team) { RuleSetBag = new RuleSetBag(ruleSet) };
				personPeriod.AddPersonSkill(new PersonSkill(skillA, new Percent(1)));
				personPeriod.AddPersonSkill(new PersonSkill(skillB, new Percent(1)));
				agent.AddPersonPeriod(personPeriod);
				var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
				schedulePeriod.SetDaysOff(1);
				agent.AddSchedulePeriod(schedulePeriod);
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
			var scheduleDays = agents.SelectMany(agent => stateHolder.Schedules.SchedulesForPeriod(period, agent)).ToList();

			Target.Execute(period, scheduleDays, new NoSchedulingProgress(), optPrefs, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()), new GroupPageLight("_", GroupPageType.SingleAgent), () => new WorkShiftFinderResultHolder(), (o, args) => { });

			agents.Count(agent => stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(0)).HasDayOff()).Should().Be.EqualTo(1); 
			agents.Count(agent => stateHolder.Schedules[agent].ScheduledDay(firstDay.AddDays(1)).HasDayOff()).Should().Be.EqualTo(1);
		}
	}
}