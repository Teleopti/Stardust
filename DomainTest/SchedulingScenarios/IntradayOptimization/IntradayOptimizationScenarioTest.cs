using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[TestFixture(BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	[TestFixture(BreakPreferenceStartTimeByMax.DoNotConsiderBreakPreferenceStartTimeByMax, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)]
	[TestFixture(BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	[TestFixture(BreakPreferenceStartTimeByMax.DoNotConsiderBreakPreferenceStartTimeByMax, RemoveImplicitResCalcContext.RemoveImplicitResCalcContextFalse)]
	public abstract class IntradayOptimizationScenarioTest : IConfigureToggleManager, ISetup
	{
		protected readonly BreakPreferenceStartTimeByMax _resourcePlanner_BreakPreferenceStartTimeByMax_46002;
		protected readonly RemoveImplicitResCalcContext _resourcePlannerRemoveImplicitResCalcContext46680;

		protected IntradayOptimizationScenarioTest(BreakPreferenceStartTimeByMax resourcePlannerBreakPreferenceStartTimeByMax46002,
							RemoveImplicitResCalcContext resourcePlannerRemoveImplicitResCalcContext46680)
		{
			_resourcePlanner_BreakPreferenceStartTimeByMax_46002 = resourcePlannerBreakPreferenceStartTimeByMax46002;
			_resourcePlannerRemoveImplicitResCalcContext46680 = resourcePlannerRemoveImplicitResCalcContext46680;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlanner_BreakPreferenceStartTimeByMax_46002 == BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax)
				toggleManager.Enable(Toggles.ResourcePlanner_BreakPreferenceStartTimeByMax_46002);
			
			if(_resourcePlannerRemoveImplicitResCalcContext46680 == RemoveImplicitResCalcContext.RemoveImplicitResCalcContextTrue)
				toggleManager.Enable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680);
			else
				toggleManager.Disable(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680); //need to disable explicitly because toggle will be set to true default later
		}

		public virtual void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<ResourceCalculateWithNewContext>();
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_BreakPreferenceStartTimeByMax_46002)]
	public enum BreakPreferenceStartTimeByMax
	{
		ConsiderBreakPreferenceStartTimeByMax,
		DoNotConsiderBreakPreferenceStartTimeByMax
	}
}