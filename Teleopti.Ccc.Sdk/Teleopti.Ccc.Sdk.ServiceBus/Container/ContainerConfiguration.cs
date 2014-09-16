using System;
using Autofac;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.Sagas.Persisters;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.Sdk.ServiceBus.AgentBadge;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
using Teleopti.Messaging.Client.SignalR;

namespace Teleopti.Ccc.Sdk.ServiceBus.Container
{
	public class ContainerConfiguration
	{
		private readonly IContainer _container;

		public ContainerConfiguration(IContainer container)
		{
			_container = container;
		}

		public void Configure()
		{
			Configure(null);
		}

		public void Configure(Func<IComponentContext, SignalRClient> sharedSignalRClient)
		{
			var build = new ContainerBuilder();
			build.RegisterGeneric(typeof(InMemorySagaPersister<>)).As(typeof(ISagaPersister<>));

			build.RegisterModule(new CommonModule
			{
				SharedSignalRClient = sharedSignalRClient
			});

			build.RegisterModule<ShiftTradeModule>();
			build.RegisterModule<AuthorizationContainerInstaller>();
			build.RegisterModule<AuthenticationContainerInstaller>();
			build.RegisterModule<AuthenticationModule>();
			build.RegisterModule<SerializationContainerInstaller>();
			build.RegisterModule<ServiceBusCommonModule>();
			build.RegisterModule<PayrollContainerInstaller>();
			build.RegisterModule<RequestContainerInstaller>();
			build.RegisterModule<SchedulingContainerInstaller>();
			build.RegisterModule<ExportForecastContainerInstaller>();
			build.RegisterModule<ImportForecastContainerInstaller>();
			build.RegisterModule<ForecastContainerInstaller>();
			build.RegisterModule<CommandDispatcherModule>();
			build.RegisterModule<LocalServiceBusEventsPublisherModule>();
			build.RegisterModule<LocalServiceBusPublisherModule>();
			build.RegisterModule<CommandHandlersModule>();
			build.RegisterModule<EventHandlersModule>();
			build.RegisterType<DoNotifySmsLink>().As<IDoNotifySmsLink>();
			build.RegisterType<AgentBadgeCalculator>().As<IAgentBadgeCalculator>();
			build.RegisterModule(SchedulePersistModule.ForOtherModules());

			build.Update(_container);
		}

	}
}