using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	//just keep for now to make it easier to add toggle soon
	public abstract class ResourceCalculationScenario : IConfigureToggleManager
	{
		public void Configure(FakeToggleManager toggleManager)
		{
		}
	}
}