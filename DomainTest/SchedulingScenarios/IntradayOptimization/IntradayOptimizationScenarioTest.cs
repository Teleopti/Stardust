using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[TestFixture(OptimizationCodeBranch.TeamBlock, BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax)]
	[TestFixture(OptimizationCodeBranch.TeamBlock, BreakPreferenceStartTimeByMax.DoNotConsiderBreakPreferenceStartTimeByMax)]
	[TestFixture(OptimizationCodeBranch.Classic, BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax)]
	[TestFixture(OptimizationCodeBranch.Classic, BreakPreferenceStartTimeByMax.DoNotConsiderBreakPreferenceStartTimeByMax)]
	public abstract class IntradayOptimizationScenarioTest : IConfigureToggleManager
	{
		protected readonly OptimizationCodeBranch _resourcePlannerMergeTeamblockClassicIntraday45508;
		protected readonly BreakPreferenceStartTimeByMax _resourcePlanner_BreakPreferenceStartTimeByMax_46002;

		protected IntradayOptimizationScenarioTest(OptimizationCodeBranch resourcePlannerMergeTeamblockClassicIntraday45508, BreakPreferenceStartTimeByMax resourcePlannerBreakPreferenceStartTimeByMax46002)
		{
			_resourcePlannerMergeTeamblockClassicIntraday45508 = resourcePlannerMergeTeamblockClassicIntraday45508;
			_resourcePlanner_BreakPreferenceStartTimeByMax_46002 = resourcePlannerBreakPreferenceStartTimeByMax46002;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerMergeTeamblockClassicIntraday45508 == OptimizationCodeBranch.TeamBlock)
				toggleManager.Enable(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508);
			
			if(_resourcePlanner_BreakPreferenceStartTimeByMax_46002 == BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax)
				toggleManager.Enable(Toggles.ResourcePlanner_BreakPreferenceStartTimeByMax_46002);
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508)]
	public enum OptimizationCodeBranch
	{
		Classic,
		TeamBlock,
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_BreakPreferenceStartTimeByMax_46002)]
	public enum BreakPreferenceStartTimeByMax
	{
		ConsiderBreakPreferenceStartTimeByMax,
		DoNotConsiderBreakPreferenceStartTimeByMax
	}
}