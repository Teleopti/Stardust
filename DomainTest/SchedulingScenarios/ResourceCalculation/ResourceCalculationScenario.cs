using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	//keeping this empty base class so we easily could run tests multiple times when res calc toggles are added
	public abstract class ResourceCalculationScenario : IConfigureToggleManager
	{
		public void Configure(FakeToggleManager toggleManager)
		{
		}
	}
}