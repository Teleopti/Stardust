using Autofac;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Staffing.PerformanceTest
{
	public class UpdateReadModelPerformanceTestAttribute : IoCTestAttribute
	{
		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeConnectionString("Tenancy", InfraTestConfigReader.ApplicationConnectionString());
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString());
			return config;
		}

		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddService<Database>();
		}

		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);
			isolate.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
			isolate.UseTestDouble<NoMessageSender>().For<IMessageSender>();
			isolate.UseTestDouble<MutableNow>().For<INow>();
		}

		protected override void BeforeInject(IComponentContext container)
		{
			base.BeforeInject(container);
			container.Resolve<IHangfireClientStarter>().Start();
		}
	}
}