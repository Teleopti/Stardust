﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	/* Running teamblock test with toggle teamBlockDayOffForIndividuals false
	 * doesn't really make much sense here because web doesn't currently support teamblock-DO.
	 * However, the test works also with toggle true (for wrong reason) so let's leave it for now.
	 */
	[DomainTest]
	public class DayOffOptimizationTeamBlockTest : DayOffOptimizationScenario
	{
		public Domain.Optimization.DayOffOptimization Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePlanningGroupSettingsRepository PlanningGroupSettingsRepository;
		public OptimizationPreferencesDefaultValueProvider OptimizationPreferencesProvider;
		public GroupScheduleGroupPageDataProvider GroupScheduleGroupPageDataProvider;

		[Test]
		public void ShouldNotMoveDOsForOneAgentOnlyButChangeAfterEachPeriod([Values(true, false)] bool useTeams)
		{
			const int numberOfAttempts = 20;
			var firstDay = new DateOnly(2015, 10, 12); //monday
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("skill", activity);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 2);
			var scenario = ScenarioRepository.Has("_");
			var team = new Team {Site = new Site("site")};
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var agent1 = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1).NumberOfDaysOf(1), ruleSet, skill);
			var agent2 = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1).NumberOfDaysOf(1), ruleSet, skill);
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));
			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay,
				1, 2, 2, 2, 2, 2, 2,
				1, 2, 2, 2, 2, 2, 2));
			PlanningGroupSettingsRepository.HasDefault(x =>
			{
				x.ConsecutiveWorkdays = new MinMax<int>(1, 20);
			}); //just to make sure anything goes
			var optPrefs = OptimizationPreferencesProvider.Fetch();
			optPrefs.Extra.UseTeams= true;
			OptimizationPreferencesProvider.SetFromTestsOnly(optPrefs);

			for (var i = 0; i < numberOfAttempts; i++)
			{
				PersonAssignmentRepository.Clear();
				PersonAssignmentRepository.Has(agent1, scenario, activity, new ShiftCategory("_").WithId(), DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 2), new TimePeriod(8, 0, 16, 0));
				var dayOffTemplate = new DayOffTemplate();
				PersonAssignmentRepository.GetSingle(firstDay.AddWeeks(0).AddDays(6), agent1).SetDayOff(dayOffTemplate);
				PersonAssignmentRepository.GetSingle(firstDay.AddWeeks(1).AddDays(6), agent1).SetDayOff(dayOffTemplate);
				PersonAssignmentRepository.Has(agent2, scenario, activity, new ShiftCategory("_").WithId(), DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 2), new TimePeriod(8, 0, 16, 0));
				PersonAssignmentRepository.GetSingle(firstDay.AddWeeks(0).AddDays(6), agent2).SetDayOff(dayOffTemplate);
				PersonAssignmentRepository.GetSingle(firstDay.AddWeeks(1).AddDays(6), agent2).SetDayOff(dayOffTemplate);

				Target.Execute(planningPeriod.Id.Value);

				var allDOs = PersonAssignmentRepository.LoadAll().Where(x => x.DayOff() != null);
				var movedD01 = allDOs.Single(x => x.Date == firstDay);
				var movedD02 = allDOs.Single(x => x.Date == firstDay.AddWeeks(1));
				if (!movedD01.Person.Equals(movedD02.Person))
					return;
			}

			Assert.Fail(
				$"Tried optimize {numberOfAttempts} number of times but always moving DOs from same agent. Giving up...");
		}
	}
}