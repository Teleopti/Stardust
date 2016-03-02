using Autofac;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.InfrastructureTest;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	public class RtaPerformanceTestAttribute : InfrastructureTestAttribute
	{
		protected override void Startup(IComponentContext container)
		{
			base.Startup(container);
			container.Resolve<IHangfireClientStarter>().Start();
		}

		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
			return config;
		}

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.AddService<TestConfiguration>();
			system.AddService<Http>();
			system.AddService<Database>();
			system.AddService<RtaStates>();
			system.AddService<StatesArePersisted>();
			system.AddService<HangFireUtilitiesWrapperForLogTime>();
		}
	}
}