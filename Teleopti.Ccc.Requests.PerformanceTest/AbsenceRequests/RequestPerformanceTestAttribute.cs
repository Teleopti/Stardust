using Autofac;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Requests.PerformanceTest.AbsenceRequests
{
	public class RequestPerformanceTestAttribute : IoCTestAttribute
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
			system.AddModule(new CommonModule(configuration));
			system.UseTestDouble<MultiAbsenceRequestsHandler>().For<MultiAbsenceRequestsHandler>();
			system.UseTestDouble<NoMessageSender>().For<IMessageSender>();
			system.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
			system.UseTestDouble<ArrangeRequestsByProcessOrder>().For<ArrangeRequestsByProcessOrder>();
			system.AddService<Database>();
			system.AddModule(new TenantServerModule(configuration));
		}

		protected override void Startup(IComponentContext container)
		{
			base.Startup(container);
			container.Resolve<HangfireClientStarter>().Start();
		}
	}
}