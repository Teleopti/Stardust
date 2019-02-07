using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.DayOffOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	public class DayOffOptimizationCascadingTest : DayOffOptimizationScenario
	{
		public FullScheduling Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;

		[Test]
		public void ShouldBaseMoveOnNonShoveledResourceCalculation()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var skillA = SkillRepository.Has("A", activity, 1).IsOpenBetween(8, 16);
			var skillB = SkillRepository.Has("B", activity, 2).IsOpenBetween(8, 16);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);
			var scenario = ScenarioRepository.LoadDefaultScenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var team = new Team { Site = new Site("_") };
			var agents = new List<IPerson>();
			for (var i = 0; i < 2; i++)
			{
				var agent = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1).NumberOfDaysOff(2), ruleSet, skillA, skillB);
				agents.Add(agent);

				PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1), new TimePeriod(8, 0, 16, 0));
				PersonAssignmentRepository.GetSingle(firstDay.AddDays(5), agent).SetDayOff(new DayOffTemplate()); 
				PersonAssignmentRepository.GetSingle(firstDay.AddDays(6), agent).SetDayOff(new DayOffTemplate()); 
			}
			var agentB = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), team, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1).NumberOfDaysOff(2), ruleSet, skillB);
			PersonAssignmentRepository.Has(agentB, scenario, activity, shiftCategory, DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.GetSingle(firstDay.AddDays(5), agentB).SetDayOff(new DayOffTemplate());
			PersonAssignmentRepository.GetSingle(firstDay.AddDays(6), agentB).SetDayOff(new DayOffTemplate());

			SkillDayRepository.Has(skillA.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 2, 2, 2, 1, 1, 1, 1));
			SkillDayRepository.Has(skillB.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 2, 2, 2, 3, 3, 2, 2));

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			agents.Count(agent => PersonAssignmentRepository.GetSingle(firstDay.AddDays(3), agent).DayOff() != null).Should().Be.EqualTo(1);
			agents.Count(agent => PersonAssignmentRepository.GetSingle(firstDay.AddDays(4), agent).DayOff() != null).Should().Be.EqualTo(1);
			agents.Count(agent => PersonAssignmentRepository.GetSingle(firstDay.AddDays(5), agent).DayOff() != null).Should().Be.EqualTo(1);
			agents.Count(agent => PersonAssignmentRepository.GetSingle(firstDay.AddDays(6), agent).DayOff() != null).Should().Be.EqualTo(1);
		}

		public DayOffOptimizationCascadingTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}