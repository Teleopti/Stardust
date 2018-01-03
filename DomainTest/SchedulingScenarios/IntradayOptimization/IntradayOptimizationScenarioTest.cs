using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[TestFixture(BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax)]
	[TestFixture(BreakPreferenceStartTimeByMax.DoNotConsiderBreakPreferenceStartTimeByMax)]
	public abstract class IntradayOptimizationScenarioTest : IConfigureToggleManager, ISetup
	{
		protected readonly BreakPreferenceStartTimeByMax _resourcePlanner_BreakPreferenceStartTimeByMax_46002;

		protected IntradayOptimizationScenarioTest(BreakPreferenceStartTimeByMax resourcePlannerBreakPreferenceStartTimeByMax46002)
		{
			_resourcePlanner_BreakPreferenceStartTimeByMax_46002 = resourcePlannerBreakPreferenceStartTimeByMax46002;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlanner_BreakPreferenceStartTimeByMax_46002 == BreakPreferenceStartTimeByMax.ConsiderBreakPreferenceStartTimeByMax)
				toggleManager.Enable(Toggles.ResourcePlanner_BreakPreferenceStartTimeByMax_46002);
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