using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Aop;
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
			builder.RegisterType<ResolveEventHandlers>().SingleInstance();

			builder.RegisterType<EventInfrastructureInfoPopulator>().As<IEventInfrastructureInfoPopulator>().SingleInstance();
			builder.RegisterType<EventPopulatingPublisher>().As<IEventPopulatingPublisher>().SingleInstance();

			builder.RegisterType<HangfireEventPublisher>().SingleInstance();
			builder.RegisterType<SyncInFatClientProcessEventPublisher>().SingleInstance();
			builder.RegisterType<SyncEventPublisher>().SingleInstance();
			builder.RegisterType<ServiceBusEventPublisher>().SingleInstance();
			builder.RegisterType<SyncAllEventPublisher>().SingleInstance();
			builder.RegisterType<ServiceBusAsSyncEventPublisher>().SingleInstance().ApplyAspects();
			builder.RegisterType<HangfireAsSyncEventPublisher>().SingleInstance().ApplyAspects();
			builder.RegisterType<MultiEventPublisherServiceBusAsSync>().SingleInstance();
			builder.RegisterType<StardustEventPublisher>().SingleInstance();

			builder.RegisterType<MultiEventPublisher>().As<IEventPublisher>().SingleInstance();
			builder.RegisterType<LogExceptions>().As<ISyncEventPublisherExceptionHandler>().SingleInstance();

			builder.RegisterType<CurrentEventPublisher>()
				.As<ICurrentEventPublisher>()
				.As<IEventPublisherScope>()
				.SingleInstance();

			builder.Register(c => c.Resolve<HangfireEventPublisher>()).As<IRecurringEventPublisher>().SingleInstance();
			builder.RegisterType<TenantTickEventPublisher>().SingleInstance();
			builder.RegisterType<AllTenantRecurringEventPublisher>().SingleInstance();

			builder.RegisterType<CannotPublishToHangfire>().As<IHangfireEventClient>().SingleInstance();
			builder.RegisterType<CannotSendDelayedMessages>().As<IDelayedMessageSender>().SingleInstance();

			builder.RegisterType<CommonEventProcessor>().ApplyAspects().SingleInstance();
			builder.RegisterType<HangfireEventProcessor>().SingleInstance();
			builder.RegisterType<ServiceBusEventProcessor>().SingleInstance();




			if (_configuration.Args().BehaviorTestServer)
			{
				builder.Register(c => c.Resolve<MultiEventPublisherServiceBusAsSync>()).As<IEventPublisher>().SingleInstance();
				builder.RegisterType<IgnoreDelayedMessages>().As<IDelayedMessageSender>().SingleInstance();
				builder.RegisterType<ThrowExceptions>().As<ISyncEventPublisherExceptionHandler>().SingleInstance();
			}

			if (_configuration.Args().AllEventPublishingsAsSync)
			{
				builder.Register(c => c.Resolve<SyncAllEventPublisher>()).As<IEventPublisher>().SingleInstance();
				builder.RegisterType<IgnoreDelayedMessages>().As<IDelayedMessageSender>().SingleInstance();
			}


		}
	}
	
}