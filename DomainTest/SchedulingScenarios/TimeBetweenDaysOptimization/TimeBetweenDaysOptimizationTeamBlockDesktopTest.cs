﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.TimeBetweenDaysOptimization
{
	[DomainTest]
	[UseIocForFatClient]
	public class TimeBetweenDaysOptimizationTeamBlockDesktopTest
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public OptimizationDesktopExecuter Target;
		public FakeRuleSetBagRepository RuleSetBagRepository;

		[Test, Ignore("Until Bug76273 is solved")]
		public void ShouldHonorMaxStaffBug76273()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_");
			var tp = new TimePeriod(7, 17);
			var skill = new Skill().WithId().For(activity).IsOpen(tp, tp, tp, tp, tp, tp, tp);
			var scenario = new Scenario("_");
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 9, 0, 60), new TimePeriodWithSegment(15, 0, 17, 0, 60), shiftCategory));
			var team = new Team { Site = new Site("_") };
			var agent1 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneWeek(firstDay);
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, team, skill).WithSchedulePeriodOneWeek(firstDay);
			agent1.SchedulePeriod(firstDay).SetDaysOff(2);
			agent2.SchedulePeriod(firstDay).SetDaysOff(2);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				5,
				1,
				5,
				5,
				25,
				5,
				5);

			var dt1 = new DateTime(2015, 10, 12, 16, 0, 0, DateTimeKind.Utc);
			var dt2 = new DateTime(2015, 10, 12, 16, 15, 0, DateTimeKind.Utc);
			var dtp = new DateTimePeriod(dt1, dt2);
			foreach (var skillDay in skillDays)
			{
				skillDay.SplitSkillDataPeriods(skillDay.SkillDataPeriodCollection);
				foreach (var skillDataPeriod in skillDay.SkillDataPeriodCollection)
				{
					if (skillDataPeriod.Period == dtp || 
						skillDataPeriod.Period == dtp.MovePeriod(TimeSpan.FromMinutes(15)) ||
						skillDataPeriod.Period == dtp.MovePeriod(TimeSpan.FromMinutes(30)) ||
						skillDataPeriod.Period == dtp.MovePeriod(TimeSpan.FromMinutes(45)))
					{
						skillDataPeriod.MinimumPersons = 1;
						skillDataPeriod.MaximumPersons = 1;
					}
				}
			}
			var asses1 = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent1, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(7, 15))).ToList();
			var asses2 = Enumerable.Range(0, 7).Select(i => new PersonAssignment(agent2, scenario, firstDay.AddDays(i)).ShiftCategory(shiftCategory).WithLayer(activity, new TimePeriod(9, 17))).ToList();
			asses1[5].SetDayOff(new DayOffTemplate()); //saturday
			asses1[6].SetDayOff(new DayOffTemplate()); //sunday
			asses2[5].SetDayOff(new DayOffTemplate()); //saturday
			asses2[6].SetDayOff(new DayOffTemplate()); //sunday
			asses1.AddRange(asses2);
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] { agent1, agent2 }, asses1, skillDays);
			var advanced = new AdvancedPreferences{UseMaximumStaffing = true, UseMinimumStaffing = true};
			var optPreferences = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag(), OptimizationStepTimeBetweenDays = true },
				Extra = { UseTeams = false, TeamGroupPage = new GroupPageLight("_", GroupPageType.RuleSetBag) },
				Advanced = advanced
			};
			var daysOffPreferences = new DaysOffPreferences();
			var dayOffOptimizationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(daysOffPreferences);
			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent1, agent2 }, period, optPreferences, dayOffOptimizationPreferenceProvider);
			skillDays[4].SkillStaffPeriodCollection.Last().CalculatedLoggedOn.Should().Be.EqualTo(1);
		}


		[Test]
		public void ShouldNotCrashOnAgentWithLeavingDate()
		{
			var date = new DateOnly(2015, 10, 12);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("_"), new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var ruleSetBag = new RuleSetBag(ruleSet){Description = new Description("_")};
			RuleSetBagRepository.Has(ruleSetBag);
			var team = new Team {Site = new Site("_")};
			var contract = new ContractWithMaximumTolerance();
			var agent1 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSetBag, contract, team).WithSchedulePeriodOneWeek(date);
			var agent2 = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSetBag, contract, team).WithSchedulePeriodOneWeek(date);
			var stateHolder = SchedulerStateHolder.Fill(new Scenario("_"), period, new []{agent1, agent2}, new List<IScheduleData>(), new List<ISkillDay>());
			agent1.TerminatePerson(date.AddDays(1), new PersonAccountUpdaterDummy());
			var optPreferences = new OptimizationPreferences
			{
				General = {ScheduleTag = new ScheduleTag(), OptimizationStepTimeBetweenDays = true},
				Extra = {UseTeams = true, TeamGroupPage = new GroupPageLight("_", GroupPageType.RuleSetBag)}
			};

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent1, agent2 }, period, optPreferences, null);
			});
		}
	}
}
