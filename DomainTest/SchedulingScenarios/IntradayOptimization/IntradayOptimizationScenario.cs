using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	public abstract class IntradayOptimizationScenario : IConfigureToggleManager
	{
		private readonly bool _cascading;

		protected IntradayOptimizationScenario(bool cascading)
		{
			_cascading = cascading;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
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