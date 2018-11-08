using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[DontSendEventsAtPersist]
	[FullPermissions]
	public abstract class IntradayOptimizationScenarioTest : IExtendSystem, IIsolateSystem
	{		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<ResourceCalculateWithNewContext>();
		}

		public virtual void Isolate(IIsolate isolate)
		{
		}
	}
}