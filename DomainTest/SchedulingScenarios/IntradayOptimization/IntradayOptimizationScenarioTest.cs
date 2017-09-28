using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[TestFixture(OptimizationCodeBranch.TeamBlock)]
	[TestFixture(OptimizationCodeBranch.Classic)]
	public abstract class IntradayOptimizationScenarioTest : IConfigureToggleManager
	{
		protected readonly OptimizationCodeBranch _resourcePlannerMergeTeamblockClassicIntraday45508;

		protected IntradayOptimizationScenarioTest(OptimizationCodeBranch resourcePlannerMergeTeamblockClassicIntraday45508)
		{
			_resourcePlannerMergeTeamblockClassicIntraday45508 = resourcePlannerMergeTeamblockClassicIntraday45508;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerMergeTeamblockClassicIntraday45508 == OptimizationCodeBranch.TeamBlock)
				toggleManager.Enable(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508);
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508)]
	public enum OptimizationCodeBranch
	{
		Classic,
		TeamBlock
	}
}