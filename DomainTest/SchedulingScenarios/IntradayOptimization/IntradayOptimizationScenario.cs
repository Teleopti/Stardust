using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	public abstract class IntradayOptimizationScenario : IConfigureToggleManager
	{
		private readonly bool _intradayIslands;
		private readonly bool _cascading;

		protected IntradayOptimizationScenario(bool intradayIslands, bool cascading)
		{
			_intradayIslands = intradayIslands;
			_cascading = cascading;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if (_intradayIslands)
			{
				toggleManager.Enable(Toggles.ResourcePlanner_IntradayIslands_36939);
			}
			else
			{
				toggleManager.Disable(Toggles.ResourcePlanner_IntradayIslands_36939);
			}
			if (_cascading)
			{
				toggleManager.Enable(Toggles.ResourcePlanner_CascadingSkills_38524);
			}
			else
			{
				toggleManager.Disable(Toggles.ResourcePlanner_CascadingSkills_38524);
			}
		}
	}
}