using System.Configuration;
using Autofac;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.Sagas.Persisters;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Sdk.ServiceBus.AgentBadge;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Sdk.ServiceBus.Container
{
	public class ContainerConfiguration
	{
		private readonly IContainer _defaultBusContainer;

		public ContainerConfiguration(IContainer defaultBusContainer)
		{
			_defaultBusContainer = defaultBusContainer;
		}

		public void Configure()
		{
			var build = new ContainerBuilder();
			build.RegisterGeneric(typeof(InMemorySagaPersister<>)).As(typeof(ISagaPersister<>));

			build.RegisterModule<ShiftTradeModule>();
			build.RegisterModule<RepositoryModule>();
			build.RegisterModule<AuthorizationContainerInstaller>();
			build.RegisterModule<AuthenticationContainerInstaller>();
			build.RegisterModule<AuthenticationModule>();
			build.RegisterModule<DateAndTimeModule>();
			build.RegisterModule<SerializationContainerInstaller>();
			build.RegisterModule<ApplicationInfrastructureContainerInstaller>();
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
			build.RegisterType<NewtonsoftJsonSerializer>().As<IJsonSerializer>();
			build.RegisterType<DoNotifySmsLink>().As<IDoNotifySmsLink>();
			build.RegisterType<AgentBadgeCalculator>().As<IAgentBadgeCalculator>();
			build.RegisterModule(SchedulePersistModule.ForOtherModules());
			build.RegisterModule(new ToggleNetModule(ConfigurationManager.AppSettings["FeatureToggle"], ConfigurationManager.AppSettings["ToggleMode"]));

			build.Update(_defaultBusContainer);
		}
	}
}