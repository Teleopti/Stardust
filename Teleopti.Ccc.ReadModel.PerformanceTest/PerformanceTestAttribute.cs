using Autofac;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.ReadModel.PerformanceTest
{
	public class PerformanceTestAttribute : IoCTestAttribute
	{
		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeConnectionString("Tenancy", InfraTestConfigReader.ConnectionString);
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
			return config;
		}

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);
			system.AddService<TestConfiguration>();
			system.AddService<Http>();
			system.AddService<DataCreator>();
			system.AddService<Database>();
			system.AddService<AnalyticsDatabase>();

			system.UseTestDouble<NoMessageSender>().For<IMessageSender>();

		}

		protected override void Startup(IComponentContext container)
		{
			base.Startup(container);
			container.Resolve<HangfireClientStarter>().Start();
		}
	}
}