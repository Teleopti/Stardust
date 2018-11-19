using System;
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
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	public class DayOffOptimizationResultTest : DayOffOptimizationScenario
	{
		public FullScheduling Target;
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
			var filterContract = new ContractWithMaximumTolerance().WithId();
			var agent = PersonRepository.Has(filterContract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var agentOutSideGroup = PersonRepository.Has(new ContractWithMaximumTolerance(), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill, irrelevantSkill);

			var planningGroup = new PlanningGroup().AddFilter(new ContractFilter(filterContract));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1, SchedulePeriodType.Day, planningGroup);
			PlanningGroupRepository.Has(planningGroup);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 2));
			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.Has(agentOutSideGroup, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay), new TimePeriod(8, 0, 16, 0));

			var result = Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			var skillResult = result.SkillResultList.ToList();
			skillResult.Count.Should().Be.EqualTo(1);
			skillResult.First().SkillName.Should().Be.EqualTo(skill.Name);
			var dayCount = skillResult.First().SkillDetails.ToList();
			dayCount.Count.Should().Be.EqualTo(1);
			dayCount.First().RelativeDifference.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldShowAgentWithMissingShiftAsNotScheduled()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var activityConectedToSkill = ActivityRepository.Has("_");
			var activityConnectedToRuleSet = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("_", activityConectedToSkill, new TimePeriod(8, 16));

			var scenario = ScenarioRepository.Has("_");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activityConnectedToRuleSet,
				new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var contract = new Contract("_")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8),TimeSpan.FromHours(0)),
				WorkTime = new WorkTime(new TimeSpan(0, 0, 0, 0))
			};
			var contractSchedule = new ContractSchedule("_");
			var contractScheduleWeek = new ContractScheduleWeek();
			contractScheduleWeek.SetWorkdaysExcept();
			contractSchedule.AddContractScheduleWeek(contractScheduleWeek);
			PersonRepository.Has(contract, contractSchedule, new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 1);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1));
			
			var result = Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			result.ScheduledAgentsCount.Should().Be.EqualTo(0);
			result.BusinessRulesValidationResults.Count().Should().Be.EqualTo(1);
			result.BusinessRulesValidationResults.First().ValidationErrors.First().ErrorResource.Should().Be.EqualTo("AgentHasDaysWithoutAnySchedule");
		}

		[Test]
		public void ShouldNotShowSkillsThatNoneOfTheScheduledAgentsHas()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var relevantSkill = SkillRepository.Has("relevant skill", activity, new TimePeriod(8, 16));
			var irrellevantSkill = SkillRepository.Has("irrelevant skill", activity, new TimePeriod(8, 16));

			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var filterContract1 = new ContractWithMaximumTolerance();
			var filterContract2 = new ContractWithMaximumTolerance();
			var agentToBeOptimized = PersonRepository.Has(filterContract1, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, relevantSkill);
			var agentNotToBeOptimized = PersonRepository.Has(filterContract2, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, irrellevantSkill, relevantSkill);

			var planningGroup = PlanningGroupRepository.Has(new PlanningGroup().AddFilter(new ContractFilter(filterContract1)));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 2, SchedulePeriodType.Day, planningGroup);

			SkillDayRepository.Has(relevantSkill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1));
			SkillDayRepository.Has(irrellevantSkill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1));

			PersonAssignmentRepository.Has(agentToBeOptimized, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(1)), new TimePeriod(8, 0, 16, 0));
			PersonAssignmentRepository.Has(agentNotToBeOptimized, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(1)), new TimePeriod(8, 0, 16, 0));

			var result = Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			result.SkillResultList.Count().Should().Be.EqualTo(1);
			result.SkillResultList.First().SkillName.Should().Be.EqualTo("relevant skill");
		}
		[Test]
		public void ShouldReturnSkillsForRelevantPersonPeriod()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var relevantSkill = SkillRepository.Has("relevant skill", activity, new TimePeriod(8, 16));
			var irrelevantSkill = SkillRepository.Has("irrelevant skill", activity, new TimePeriod(8, 16));

			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriodMonth = new SchedulePeriod(new DateOnly(2015, 1, 1), SchedulePeriodType.Month, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var contract = new ContractWithMaximumTolerance();
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriodMonth, ruleSet, irrelevantSkill);
			var personPeriod = new PersonPeriod(new DateOnly(2015,1,1), new PersonContract(contract,new PartTimePercentage("_"),new ContractSchedule("_") ), new Team());
			personPeriod.AddPersonSkill(new PersonSkill(relevantSkill,new Percent(100)));
			agent.AddPersonPeriod(personPeriod);
			
			var planningGroup = PlanningGroupRepository.Has(new PlanningGroup().AddFilter(new ContractFilter(contract)));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 2, SchedulePeriodType.Day, planningGroup);

			SkillDayRepository.Has(relevantSkill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1));
			SkillDayRepository.Has(irrelevantSkill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1));

			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(1)), new TimePeriod(8, 0, 16, 0));
			
			var result = Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			result.SkillResultList.Count().Should().Be.EqualTo(1);
			result.SkillResultList.First().SkillName.Should().Be.EqualTo("relevant skill");
		}

		[Test]
		public void ShouldOnlyIncludeActiveSkills()
		{
			var firstDay = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var activeSkill = SkillRepository.Has("active skill", activity, new TimePeriod(8, 16));
			var inactiveSkill = SkillRepository.Has("inactive skill", activity, new TimePeriod(8, 16));

			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(new DateOnly(2015, 1, 1), SchedulePeriodType.Week, 1);
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));
			var contract = new ContractWithMaximumTolerance();
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, activeSkill,inactiveSkill);
			agent.DeactivateSkill(inactiveSkill, agent.PersonPeriodCollection.First());

			var planningGroup = PlanningGroupRepository.Has(new PlanningGroup().AddFilter(new ContractFilter(contract)));
			var planningPeriod = PlanningPeriodRepository.Has(firstDay, 2, SchedulePeriodType.Day, planningGroup);

			SkillDayRepository.Has(activeSkill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1));
			SkillDayRepository.Has(inactiveSkill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1));

			PersonAssignmentRepository.Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(firstDay, firstDay.AddDays(1)), new TimePeriod(8, 0, 16, 0));

			var result = Target.DoSchedulingAndDO(planningPeriod.Id.Value);
			result.SkillResultList.Count().Should().Be.EqualTo(1);
			result.SkillResultList.First().SkillName.Should().Be.EqualTo("active skill");
		}
		
		[Test]
		public void HintsShouldIncludeSelectedAgentsOnly()
		{
			var date = new DateOnly(2015, 10, 12); //mon
			var activity = ActivityRepository.Has("_");
			var skill = SkillRepository.Has("relevant skill", activity, new TimePeriod(8, 16));

			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(date, SchedulePeriodType.Week, 1);
			var filterContract = new Contract("_");
			var contractScheduleWorkingMondayToFriday = new ContractScheduleWorkingMondayToFriday();
			var partTimePercentage = new PartTimePercentage("_");
			var team = new Team {Site = new Site("site")};
			var agentToSchedule = PersonRepository.Has(filterContract, contractScheduleWorkingMondayToFriday, partTimePercentage, team, schedulePeriod, skill);
			PersonRepository.Has(new Contract("_2"), contractScheduleWorkingMondayToFriday, partTimePercentage, team, schedulePeriod, skill);

			var planningGroup = new PlanningGroup().AddFilter(new ContractFilter(filterContract));
			var planningPeriod = PlanningPeriodRepository.Has(date, 1, SchedulePeriodType.Week, planningGroup);
			PlanningGroupRepository.Has(planningGroup);
			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 2));

			var result = Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var validationResults = result.BusinessRulesValidationResults.ToList();
			validationResults.Count.Should().Be.EqualTo(1);
			validationResults.Single().ResourceId.Should().Be.EqualTo(agentToSchedule.Id.Value);
		}

		public DayOffOptimizationResultTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}