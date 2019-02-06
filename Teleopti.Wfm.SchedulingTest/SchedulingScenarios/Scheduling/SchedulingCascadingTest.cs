using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class SchedulingCascadingTest : SchedulingScenario
	{
		public FullScheduling Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository AssignmentRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeDayOffTemplateRepository DayOffTemplateRepository;
		public IResourceOptimizationHelperExtended ResourceCalculation;
		public FakePlanningPeriodRepository PlanningPeriodRepository;

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldBaseBestShiftOnNonShoveledResourceCalculation(bool resourceCalculationHasBeenMade)
		{
			const int numberOfAgents = 50;
			DayOffTemplateRepository.Has(DayOffFactory.CreateDayOff());
			var earlyInterval = new TimePeriod(7, 45, 8, 0);
			var lateInterval = new TimePeriod(15, 45, 16, 0);
			var date = DateOnly.Today;
			var activity = ActivityRepository.Has("_");
			var skillA = SkillRepository.Has("A", activity, 1).DefaultResolution(15);
			var skillB = SkillRepository.Has("B", activity, 2).DefaultResolution(15);
			var scenario = ScenarioRepository.LoadDefaultScenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
					new TimePeriodWithSegment(earlyInterval, TimeSpan.FromMinutes(15)),
					new TimePeriodWithSegment(lateInterval, TimeSpan.FromMinutes(15)), shiftCategory));
			var contract = new Contract("_")
			{
				WorkTimeDirective =
					new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))
			};
			Enumerable.Range(0, numberOfAgents).ForEach(i => PersonRepository.Has(contract, new SchedulePeriod(date, SchedulePeriodType.Day, 1), ruleSet, skillA, skillB));
			SkillDayRepository.Has(skillA.CreateSkillDayWithDemand(scenario, date, 1), skillB.CreateSkillDayWithDemandOnInterval(scenario, date, 1, new Tuple<TimePeriod, double>(lateInterval, 1000)));
			if(resourceCalculationHasBeenMade)
				ResourceCalculation.ResourceCalculateAllDays(new NoSchedulingProgress(), false);
			var planningPeriod = PlanningPeriodRepository.Has(date,SchedulePeriodType.Day, 1);
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			var allAssignmentsStartTime = AssignmentRepository.LoadAll().Select(x => x.Period.StartDateTime.TimeOfDay);
			allAssignmentsStartTime.Count(x => x == new TimeSpan(7, 45, 0))
				.Should().Be.EqualTo(numberOfAgents/2);
			allAssignmentsStartTime.Count(x => x == new TimeSpan(8, 0, 0))
				.Should().Be.EqualTo(numberOfAgents/2);
		}

		public SchedulingCascadingTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}