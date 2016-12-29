using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Requests.PerformanceTuningTest
{
	public class RequestPerformanceTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			SetupSystem(system, configuration);
		}

		public static void SetupSystem(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new CommonModule(configuration));
			system.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
			system.UseTestDouble<MultiAbsenceRequestsHandler>().For<MultiAbsenceRequestsHandler>();
			system.UseTestDouble<NoMessageSender>().For<IMessageSender>();
			system.UseTestDouble<UpdateStaffingLevelReadModel>().For<UpdateStaffingLevelReadModel>();
			system.UseTestDouble<MutableNow>().For<INow>();
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