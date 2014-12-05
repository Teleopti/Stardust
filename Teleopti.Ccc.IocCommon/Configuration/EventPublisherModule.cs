using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class EventPublisherModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public EventPublisherModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SyncEventsPublisher>().As<IEventsPublisher>().SingleInstance();

			builder.RegisterType<EventContextPopulator>().As<IEventContextPopulator>().SingleInstance();
			builder.RegisterType<EventPopulatingPublisher>().As<IEventPopulatingPublisher>().SingleInstance();

			builder.RegisterType<SyncEventPublisher>().As<ISyncEventPublisher>().SingleInstance();
			builder.RegisterType<HangfireEventPublisher>().As<IHangfireEventPublisher>().SingleInstance();
			builder.RegisterType<ServiceBusEventPublisher>().As<IServiceBusEventPublisher>().SingleInstance();

			builder.Register(c => c.Resolve<IServiceBusEventPublisher>()).As<IEventPublisher>().SingleInstance();

			builder.RegisterType<CannotPublishEventsFromEventHandlers>().As<IPublishEventsFromEventHandlers>().SingleInstance();
			builder.RegisterType<CannotSendDelayedMessages>().As<ISendDelayedMessages>().SingleInstance();
		}
	}
}