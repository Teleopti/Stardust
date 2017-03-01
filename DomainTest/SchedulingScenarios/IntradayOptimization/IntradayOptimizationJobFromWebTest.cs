using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[TestFixture]
	[DomainTest]
	[Toggle(Toggles.Wfm_ResourcePlanner_SchedulingOnStardust_42874)]
	public class IntradayOptimizationJobFromWebTest
	{
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeAgentGroupRepository AgentGroupRepository;
		public IIntradayOptimizationFromWeb Target;
		public FakeJobResultRepository JobResultRepository;

		[Test]
		public void ShouldSaveJobResult()
		{
			var team = new Team { Site = new Site("site") };
			var agentGroup = new AgentGroup("Europe")
				.AddFilter(new TeamFilter(team));

			var planningPeriod = PlanningPeriodRepository.Has(DateOnly.Today, 1, agentGroup);

			Target.Execute(Guid.NewGuid());

			var jobResult = JobResultRepository.LoadAll().Single();
			jobResult.JobCategory.Should().Be.EqualTo(JobCategory.WebIntradayOptimiztion);
			jobResult.Period.Should().Be.EqualTo(planningPeriod.Range);
		}
	}
}