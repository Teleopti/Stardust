using Autofac;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Scheduling.PerformanceTest.Infrastructure
{
	public class PerformanceInfrastructureTestAttribute : InfrastructureTestAttribute
	{
		protected override void Startup(IComponentContext container)
		{
			//container.Resolve<HangfireClientStarter>().Start();
		}
	}
}