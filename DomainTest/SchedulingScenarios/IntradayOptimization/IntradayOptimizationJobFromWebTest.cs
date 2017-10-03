using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[Toggle(Toggles.Wfm_ResourcePlanner_SchedulingOnStardust_42874)]
	public class IntradayOptimizationFromWebTest : IntradayOptimizationScenarioTest
	{
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakePlanningGroupRepository PlanningGroupRepository;
		public IntradayOptimizationFromWeb Target;
		public FakeJobResultRepository JobResultRepository;
		public FakePersonRepository PersonRepository;

		[Test]
		public void ShouldNotSaveJobResultForEmptyPlanningGroup()
		{
			var team = new Team { Site = new Site("site") };
			var planningGroup = new PlanningGroup("Europe").AddFilter(new TeamFilter(team));
			var planningPeriod = PlanningPeriodRepository.Has(DateOnly.Today, 1, planningGroup);

			Target.Execute(planningPeriod.Id.GetValueOrDefault(), true);

			JobResultRepository.LoadAll().Should().Be.Empty();
		}

		[Test]
		public void ShouldSaveJobResult()
		{
			var team = new Team { Site = new Site("site") };
			var planningGroup = new PlanningGroup("Europe").AddFilter(new TeamFilter(team));
			PersonRepository.Add(PersonFactory.CreatePersonWithPersonPeriodFromTeam(DateOnly.Today, team));

			var planningPeriod = PlanningPeriodRepository.Has(DateOnly.Today, 1, planningGroup);

			Target.Execute(planningPeriod.Id.GetValueOrDefault(), true);

			var jobResult = JobResultRepository.LoadAll().Single();
			jobResult.JobCategory.Should().Be.EqualTo(JobCategory.WebIntradayOptimiztion);
			jobResult.Period.Should().Be.EqualTo(planningPeriod.Range);
		}

		public IntradayOptimizationFromWebTest(OptimizationCodeBranch resourcePlannerMergeTeamblockClassicIntraday45508, BreakPreferenceStartTimeByMax resourcePlannerBreakPreferenceStartTimeByMax46002) : base(resourcePlannerMergeTeamblockClassicIntraday45508, resourcePlannerBreakPreferenceStartTimeByMax46002)
		{
		}
	}
}