using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	public abstract class IntradayOptimizationScenarioTest : IExtendSystem, IIsolateSystem
	{		
		public void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddService<ResourceCalculateWithNewContext>();
		}

		public virtual void Isolate(IIsolate isolate)
		{
		}
	}
}