using Autofac;
using Hangfire.Server;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class EventPublisherModule : Module
	{
		private readonly IocConfiguration _configuration;

		public EventPublisherModule(IocConfiguration configuration)
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
			builder.RegisterType<SyncAllEventPublisher>().SingleInstance();
			builder.RegisterType<HangfireAsSyncEventPublisher>().SingleInstance().ApplyAspects();
			builder.RegisterType<StardustEventPublisher>().SingleInstance();
			
			builder.RegisterType<RtaEventPublisher>().As<IRtaEventPublisher>().SingleInstance().ApplyAspects();
			builder.RegisterType<MultiEventPublisher>().As<IEventPublisher>().AsSelf().SingleInstance();
			builder.RegisterType<LogExceptions>().As<ISyncEventProcessingExceptionHandler>().SingleInstance();

			builder.RegisterType<CurrentEventPublisher>()
				.As<ICurrentEventPublisher>()
				.As<IEventPublisherScope>()
				.SingleInstance();

			builder.Register(c => c.Resolve<HangfireEventPublisher>()).As<IRecurringEventPublisher>().SingleInstance();
			if (!_configuration.Args().BehaviorTestServer)
				builder.RegisterType<RecurringEventPublishingUpdater>().AsSelf().As<IBackgroundProcess>().SingleInstance();
			builder.RegisterType<RecurringEventPublishings>().SingleInstance();
			builder.RegisterType<AllTenantRecurringEventPublisher>().SingleInstance();

			builder.RegisterType<CannotPublishToHangfire>().As<IHangfireEventClient>().SingleInstance();
			builder.RegisterType<CannotSendDelayedMessages>().As<IDelayedMessageSender>().SingleInstance();

			builder.RegisterType<CommonEventProcessor>().ApplyAspects().SingleInstance();
			builder.RegisterType<HangfireEventProcessor>().SingleInstance();

			if (_configuration.Args().BehaviorTestServer)
			{
				builder.Register(c => c.Resolve<MultiEventPublisher>()).As<IEventPublisher>().SingleInstance();
				builder.RegisterType<IgnoreDelayedMessages>().As<IDelayedMessageSender>().SingleInstance();
				builder.RegisterType<ThrowExceptions>().As<ISyncEventProcessingExceptionHandler>().SingleInstance();
			}

			if (_configuration.Args().AllEventPublishingsAsSync)
			{
				builder.Register(c => c.Resolve<SyncAllEventPublisher>()).As<IEventPublisher>().SingleInstance();
				builder.RegisterType<IgnoreDelayedMessages>().As<IDelayedMessageSender>().SingleInstance();
			}
		}
	}
	
}