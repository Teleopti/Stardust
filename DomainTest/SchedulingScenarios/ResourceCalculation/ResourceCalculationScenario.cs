using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[FullPermissions]
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