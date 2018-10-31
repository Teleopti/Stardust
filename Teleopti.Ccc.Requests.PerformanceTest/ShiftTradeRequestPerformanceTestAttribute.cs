using System;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Requests.PerformanceTest
{
	public class ShiftTradeRequestPerformanceTestAttribute : IoCTestAttribute
	{
		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeConnectionString("Tenancy", InfraTestConfigReader.ConnectionString);
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
			return config;
		}

		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			
			extend.AddService<Database>();
		}

		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate (isolate);

			isolate.UseTestDouble<NoMessageSender>().For<IMessageSender>();
			isolate.UseTestDouble<StardustJobFeedback>().For<IStardustJobFeedback>();
			isolate.UseTestDouble<ShiftTradeRequestHandler>().For<ShiftTradeRequestHandler>();

		}

		protected override void Startup (IComponentContext container)
		{
			base.Startup (container);

			// normal test injection is not working...
			((MutableNow) container.Resolve<INow>()).Is (new DateTime (2016, 04, 01, 10, 00, 00, DateTimeKind.Utc));
			
			container.Resolve<IHangfireClientStarter>().Start();
		}
	}
}