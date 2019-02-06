using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.ResourceCalculation
{
	[DefaultData]
	public abstract class ResourceCalculationScenario : IConfigureToggleManager, IExtendSystem
	{
		public void Configure(FakeToggleManager toggleManager)
		{
		}
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<ResourceCalculateWithNewContext>();
		}
	}
}