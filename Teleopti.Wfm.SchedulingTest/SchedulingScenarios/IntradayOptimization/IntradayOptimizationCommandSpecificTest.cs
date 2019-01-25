using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public class IntradayOptimizationCommandSpecificTest
	{
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public IntradayOptimizationCommandHandler Target;
		public IPersonWeekViolatingWeeklyRestSpecification CheckWeeklyRestRule;
		public IScheduleStorage ScheduleStorage;
		public FakePlanningPeriodRepository PlanningPeriodRepository;

		[Test]
		public void ShouldNotResolveWeeklyRestIfCommandSaysItShouldNotRun()
		{
			var weeklyRest = TimeSpan.FromHours(38);
			var phoneActivity = ActivityFactory.CreateActivity("phone");
			phoneActivity.InWorkTime = true;
			var skill = SkillRepository.Has("skill", phoneActivity);
			var dateOnly = new DateOnly(2015, 10, 12);
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), weeklyRest);
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(phoneActivity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var agent = PersonRepository.Has(contract, new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, schedulePeriod, ruleSet, skill);
			PersonAssignmentRepository.Has(agent, scenario, phoneActivity, shiftCategory, new DateOnlyPeriod(dateOnly, dateOnly.AddDays(7)), new TimePeriod(8, 0, 16, 0));

			
			Target.Execute(new IntradayOptimizationCommand
			{
				Period = planningPeriod.Range,
				RunResolveWeeklyRestRule = false,
				PlanningPeriodId = planningPeriod.Id.Value
			});

			var agentRange = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(agent, new ScheduleDictionaryLoadOptions(false, false, false), planningPeriod.Range, scenario)[agent];
			CheckWeeklyRestRule.IsSatisfyBy(agentRange, planningPeriod.Range, weeklyRest).Should().Be.False();
		}
	}
}