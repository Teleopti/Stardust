using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	public abstract class IntradayOptimizationScenario : IConfigureToggleManager
	{
		private readonly bool _intradayIslands;

		protected IntradayOptimizationScenario(bool intradayIslands)
		{
			_intradayIslands = intradayIslands;
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
		}
	}
}