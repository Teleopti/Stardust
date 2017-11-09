﻿using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[TestFixture(OptimizationCodeBranch.TeamBlock, BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax, true)]
	[TestFixture(OptimizationCodeBranch.TeamBlock, BreakPreferenceStartTimeByMax.DoNotConsiderBreakPreferenceStartTimeByMax, true)]
	[TestFixture(OptimizationCodeBranch.Classic, BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax, true)]
	[TestFixture(OptimizationCodeBranch.Classic, BreakPreferenceStartTimeByMax.DoNotConsiderBreakPreferenceStartTimeByMax, true)]
	[TestFixture(OptimizationCodeBranch.TeamBlock, BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax, false)]
	[TestFixture(OptimizationCodeBranch.TeamBlock, BreakPreferenceStartTimeByMax.DoNotConsiderBreakPreferenceStartTimeByMax, false)]
	[TestFixture(OptimizationCodeBranch.Classic, BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax, false)]
	[TestFixture(OptimizationCodeBranch.Classic, BreakPreferenceStartTimeByMax.DoNotConsiderBreakPreferenceStartTimeByMax, false)]
	public abstract class IntradayOptimizationScenarioTest : IConfigureToggleManager
	{
		protected readonly OptimizationCodeBranch _resourcePlannerMergeTeamblockClassicIntraday45508;
		protected readonly BreakPreferenceStartTimeByMax _resourcePlanner_BreakPreferenceStartTimeByMax_46002;
		private readonly bool _resourcePlannerRemoveImplicitResCalcContext46680;

		protected IntradayOptimizationScenarioTest(OptimizationCodeBranch resourcePlannerMergeTeamblockClassicIntraday45508, 
							BreakPreferenceStartTimeByMax resourcePlannerBreakPreferenceStartTimeByMax46002,
							bool resourcePlannerRemoveImplicitResCalcContext46680)
		{
			_resourcePlannerMergeTeamblockClassicIntraday45508 = resourcePlannerMergeTeamblockClassicIntraday45508;
			_resourcePlanner_BreakPreferenceStartTimeByMax_46002 = resourcePlannerBreakPreferenceStartTimeByMax46002;
			_resourcePlannerRemoveImplicitResCalcContext46680 = resourcePlannerRemoveImplicitResCalcContext46680;
			
			if(_resourcePlannerRemoveImplicitResCalcContext46680)
				Assert.Ignore("Klagge & Rågge fixar snart");
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerMergeTeamblockClassicIntraday45508 == OptimizationCodeBranch.TeamBlock)
				toggleManager.Enable(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508);
			
			if(_resourcePlanner_BreakPreferenceStartTimeByMax_46002 == BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax)
				toggleManager.Enable(Toggles.ResourcePlanner_BreakPreferenceStartTimeByMax_46002);
			
			if(_resourcePlannerRemoveImplicitResCalcContext46680)
				toggleManager.Enable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680);
			else
				toggleManager.Disable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680); //need to disable explicitly because toggle will be set to true default later
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