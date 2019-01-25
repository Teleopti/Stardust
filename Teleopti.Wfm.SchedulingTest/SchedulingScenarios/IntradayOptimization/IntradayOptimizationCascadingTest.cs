using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
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


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public class IntradayOptimizationCascadingTest : IntradayOptimizationScenarioTest
	{
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public IntradayOptimizationFromWeb Target;

		[Test]
		public void ShouldOnlyConsiderPrimarySkillDuringOptimization()
		{
			var dateOnly = DateOnly.Today;
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var shiftCategory = new ShiftCategory("_").WithId();
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			var activity = new Activity("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 15, 8, 15, 15), new TimePeriodWithSegment(17, 15, 17, 15, 15), shiftCategory));
			var skillA = SkillRepository.Has("skillA", activity, 1).DefaultResolution(15);
			var skillB = SkillRepository.Has("skillB", activity, 2).DefaultResolution(15);
			var agentA = PersonRepository.Has(contract, schedulePeriod, ruleSet, skillA, skillB);
			SkillDayRepository.Has(new List<ISkillDay>
							 {
								skillA.CreateSkillDayWithDemandOnInterval(scenario,dateOnly, 1, new Tuple<TimePeriod, double>(new TimePeriod(17, 0, 17, 15), 2)),
								skillB.CreateSkillDayWithDemandOnInterval(scenario,dateOnly, 1, new Tuple<TimePeriod, double>(new TimePeriod(8, 0, 8, 15), 10))
							 });
			PersonAssignmentRepository.Has(agentA, scenario, activity, shiftCategory, dateOnly, new TimePeriod(8, 0, 17, 0));

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(dateOnly, agentA).Period
					.Should().Be.EqualTo(dateOnly.ToDateTimePeriod(new TimePeriod(8, 15, 17, 15), agentA.PermissionInformation.DefaultTimeZone()));
		}

		[Test]
		public void ShouldOnlyConsiderPrimarySkillWhenFindingBestShift()
		{
			var dateOnly = DateOnly.Today;
			var scenario = ScenarioRepository.Has("some name");
			var schedulePeriod = new SchedulePeriod(dateOnly, SchedulePeriodType.Week, 1);
			var worktimeDirective = new WorkTimeDirective(TimeSpan.FromHours(36), TimeSpan.FromHours(63), TimeSpan.FromHours(11), TimeSpan.FromHours(36));
			var contract = new Contract("contract") { WorkTimeDirective = worktimeDirective, PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(9) };
			var shiftCategory = new ShiftCategory("_").WithId();
			var planningPeriod = PlanningPeriodRepository.Has(dateOnly, 1);
			var activity = new Activity("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 45, 8, 15, 15), new TimePeriodWithSegment(16, 45, 17, 15, 15), shiftCategory));
			ruleSet.AddLimiter(new ActivityTimeLimiter(activity, TimeSpan.FromHours(9), OperatorLimiter.Equals));
			var skillA = SkillRepository.Has("skillA", activity, 1).DefaultResolution(15);
			var skillB = SkillRepository.Has("skillB", activity, 2).DefaultResolution(15);
			var agentA = PersonRepository.Has(contract, schedulePeriod, ruleSet, skillA, skillB);
			SkillDayRepository.Has(new List<ISkillDay>
							 {
								skillA.CreateSkillDayWithDemandOnInterval(scenario,dateOnly,1, new Tuple<TimePeriod, double>(new TimePeriod(7, 45, 8, 0), 2)),
								skillB.CreateSkillDayWithDemandOnInterval(scenario,dateOnly,0, new Tuple<TimePeriod, double>(new TimePeriod(17, 0, 17, 15), 100)) //this huge demand should not be considered
							 });
			PersonAssignmentRepository.Has(agentA, scenario, activity, shiftCategory, dateOnly, new TimePeriod(8, 0, 17, 0));

			Target.Execute(planningPeriod.Id.Value);

			PersonAssignmentRepository.GetSingle(dateOnly, agentA).Period
					.Should().Be.EqualTo(dateOnly.ToDateTimePeriod(new TimePeriod(7, 45, 16, 45), agentA.PermissionInformation.DefaultTimeZone()));
		}
	}
}