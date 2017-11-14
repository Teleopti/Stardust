using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[TestFixture(OptimizationCodeBranch.TeamBlock, BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	[TestFixture(OptimizationCodeBranch.TeamBlock, BreakPreferenceStartTimeByMax.DoNotConsiderBreakPreferenceStartTimeByMax, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	[TestFixture(OptimizationCodeBranch.Classic, BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	[TestFixture(OptimizationCodeBranch.Classic, BreakPreferenceStartTimeByMax.DoNotConsiderBreakPreferenceStartTimeByMax, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	[TestFixture(OptimizationCodeBranch.TeamBlock, BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	[TestFixture(OptimizationCodeBranch.TeamBlock, BreakPreferenceStartTimeByMax.DoNotConsiderBreakPreferenceStartTimeByMax, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	[TestFixture(OptimizationCodeBranch.Classic, BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	[TestFixture(OptimizationCodeBranch.Classic, BreakPreferenceStartTimeByMax.DoNotConsiderBreakPreferenceStartTimeByMax, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	public abstract class IntradayOptimizationScenarioTest : IConfigureToggleManager, ISetup
	{
		protected readonly OptimizationCodeBranch _resourcePlannerMergeTeamblockClassicIntraday45508;
		protected readonly BreakPreferenceStartTimeByMax _resourcePlanner_BreakPreferenceStartTimeByMax_46002;
		private readonly RemoveImplicitResCalcContext _resourcePlannerRemoveImplicitResCalcContext46680;

		protected IntradayOptimizationScenarioTest(OptimizationCodeBranch resourcePlannerMergeTeamblockClassicIntraday45508, 
							BreakPreferenceStartTimeByMax resourcePlannerBreakPreferenceStartTimeByMax46002,
							RemoveImplicitResCalcContext resourcePlannerRemoveImplicitResCalcContext46680)
		{
			_resourcePlannerMergeTeamblockClassicIntraday45508 = resourcePlannerMergeTeamblockClassicIntraday45508;
			_resourcePlanner_BreakPreferenceStartTimeByMax_46002 = resourcePlannerBreakPreferenceStartTimeByMax46002;
			_resourcePlannerRemoveImplicitResCalcContext46680 = resourcePlannerRemoveImplicitResCalcContext46680;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerMergeTeamblockClassicIntraday45508 == OptimizationCodeBranch.TeamBlock)
				toggleManager.Enable(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508);
			
			if(_resourcePlanner_BreakPreferenceStartTimeByMax_46002 == BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax)
				toggleManager.Enable(Toggles.ResourcePlanner_BreakPreferenceStartTimeByMax_46002);
			
			if(_resourcePlannerRemoveImplicitResCalcContext46680 == RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)
				toggleManager.Enable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680);
			else
				toggleManager.Disable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680); //need to disable explicitly because toggle will be set to true default later
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<ResourceCalculateWithNewContext>();
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