using System;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.FeatureFlags;
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
			builder.RegisterType<AutofacResolve>().As<IResolve>().SingleInstance();

			builder.RegisterType<SyncEventsPublisher>().As<IEventsPublisher>().SingleInstance();

			builder.RegisterType<EventContextPopulator>().As<IEventContextPopulator>().SingleInstance();
			builder.RegisterType<EventPopulatingPublisher>().As<IEventPopulatingPublisher>().SingleInstance();

			builder.RegisterType<SyncEventPublisher>().As<ISyncEventPublisher>().SingleInstance();
			builder.RegisterType<HangfireEventPublisher>().As<IHangfireEventPublisher>().SingleInstance();
			builder.RegisterType<ServiceBusEventPublisher>().As<IServiceBusEventPublisher>().SingleInstance();
			builder.RegisterType<ResolveEventHandlers>().As<IResolveEventHandlers>().SingleInstance();

			if (_configuration.Toggle(Toggles.RTA_HangfireEventProcessing_31237))
			{
				builder.RegisterType<HangfireOrBusEventPublisher>().As<IEventPublisher>().SingleInstance();
			}
			else
			{
				builder.Register(c => c.Resolve<IServiceBusEventPublisher>()).As<IEventPublisher>().SingleInstance();
			}

			builder.RegisterType<CurrentEventPublisher>()
				.As<ICurrentEventPublisher>()
				.As<IEventPublisherScope>()
				.SingleInstance();

			builder.RegisterType<CannotPublishToHangfire>().As<IHangfireEventClient>();
			builder.RegisterType<CannotPublishEventsFromEventHandlers>().As<IPublishEventsFromEventHandlers>().SingleInstance();
			builder.RegisterType<CannotSendDelayedMessages>().As<ISendDelayedMessages>().SingleInstance();
		}
	}
}