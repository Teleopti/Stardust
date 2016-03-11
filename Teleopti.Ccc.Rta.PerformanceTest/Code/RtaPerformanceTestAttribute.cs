using Autofac;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.InfrastructureTest;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class RtaPerformanceTestAttribute : InfrastructureTestAttribute
	{
		protected override void Startup(IComponentContext container)
		{
			base.Startup(container);
			container.Resolve<IHangfireClientStarter>().Start();
		}
		
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.AddService<TestConfiguration>();
			system.AddService<Http>();
			system.AddService<DataCreator>();
			system.AddService<StatesSender>();
			system.AddService<StatesArePersisted>();
		}
	}
}