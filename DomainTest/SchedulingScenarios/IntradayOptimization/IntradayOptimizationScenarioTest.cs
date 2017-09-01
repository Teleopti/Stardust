using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[TestFixture(OptimizationCodeBranch.TeamBlock, true)]
	[TestFixture(OptimizationCodeBranch.TeamBlock, false)]
	[TestFixture(OptimizationCodeBranch.Classic, true)]
	[TestFixture(OptimizationCodeBranch.Classic, false)]
	public abstract class IntradayOptimizationScenarioTest : IConfigureToggleManager
	{
		protected readonly OptimizationCodeBranch _resourcePlannerMergeTeamblockClassicIntraday45508;
		private readonly bool _resourcePlannerSpeedUpShiftsWithinDay45694;

		protected IntradayOptimizationScenarioTest(OptimizationCodeBranch resourcePlannerMergeTeamblockClassicIntraday45508, bool resourcePlannerSpeedUpShiftsWithinDay45694)
		{
			_resourcePlannerMergeTeamblockClassicIntraday45508 = resourcePlannerMergeTeamblockClassicIntraday45508;
			_resourcePlannerSpeedUpShiftsWithinDay45694 = resourcePlannerSpeedUpShiftsWithinDay45694;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerMergeTeamblockClassicIntraday45508 == OptimizationCodeBranch.TeamBlock)
				toggleManager.Enable(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508);
			if (_resourcePlannerSpeedUpShiftsWithinDay45694)
				toggleManager.Enable(Toggles.ResourcePlanner_SpeedUpShiftsWithinDay_45694);
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508)]
	public enum OptimizationCodeBranch
	{
		Classic,
		TeamBlock
	}
}