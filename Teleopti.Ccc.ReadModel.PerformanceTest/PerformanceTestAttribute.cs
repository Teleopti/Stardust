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
			config.FakeInfraTestConfig();
			return config;
		}

		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			
			extend.AddService<TestConfiguration>();
			extend.AddService<Http>();
			extend.AddService<DataCreator>();
			extend.AddService<Database>();
			extend.AddService<AnalyticsDatabase>();
		}

		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);

			isolate.UseTestDouble<NoMessageSender>().For<IMessageSender>();

		}

		protected override void Startup(IComponentContext container)
		{
			base.Startup(container);
			container.Resolve<IHangfireClientStarter>().Start();
		}
	}
}