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
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[DomainTest]
	[TestFixture(true)]
	[TestFixture(false)]
	public class DayOffOptimizationResultTest : DayOffOptimizationScenario
	{
		public IScheduleOptimization Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePlanningGroupRepository PlanningGroupRepository;

		[Test]
		public void ShouldIncludeAgentsInOtherPlanningGroupsWhenCalculatingStaffing()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var irrelevantActivity = ActivityRepository.Has("irrelevant");
			var skill = SkillRepository.Has("relevant skill", activity, new TimePeriod(8, 16));
			var irrelevantSkill = SkillRepository.Has("irrelevant skill", irrelevantActivity); // To see we don't include this in the result
			
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Day, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var filterContract = new Contract("relevant contract").WithId();
			var agent = PersonRepository.Has(filterContract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var agentOutSideGroup = PersonRepository.Has(new Contract("_"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill, irrelevantSkill);

			var planningGroup = new PlanningGroup("_").AddFilter(new ContractFilter(filterContract));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1, SchedulePeriodType.Day, planningGroup);
			PlanningGroupRepository.Has(planningGroup);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 2));
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.Has(agentOutSideGroup, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay), new TimePeriod(8, 0, 16, 0));

			var result = Target.Execute(planningPeriod.Id.Value);
			var skillResult = result.SkillResultList.ToList();
			skillResult.Count.Should().Be.EqualTo(1);
			skillResult.First().SkillName.Should().Be.EqualTo(skill.Name);
			var dayCount = skillResult.First().SkillDetails.ToList();
			dayCount.Count.Should().Be.EqualTo(1);
			dayCount.First().RelativeDifference.Should().Be.EqualTo(0);
		}

		public DayOffOptimizationResultTest(bool teamBlockDayOffForIndividuals) : base(teamBlockDayOffForIndividuals)
		{
		}
	}
}