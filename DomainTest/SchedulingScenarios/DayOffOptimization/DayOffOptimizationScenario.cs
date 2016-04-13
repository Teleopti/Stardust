using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.DayOffOptimization
{
	public abstract class DayOffOptimizationScenario : IConfigureToggleManager
	{
		protected DayOffOptimizationScenario(bool teamBlockDayOffForIndividuals)
		{
			TeamBlockDayOffForIndividuals = teamBlockDayOffForIndividuals;
		}

		protected bool TeamBlockDayOffForIndividuals { get; private set; }

		public void Configure(FakeToggleManager toggleManager)
		{
			if (TeamBlockDayOffForIndividuals)
			{
				toggleManager.Enable(Toggles.ResourcePlanner_TeamBlockDayOffForIndividuals_37998);
			}
			else
			{
				toggleManager.Disable(Toggles.ResourcePlanner_IntradayIslands_36939);
			}
		}
	}
}