using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class SyncEventsPublisherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.Register(c => c.Resolve<IEventPopulatingPublisher>() as EventPopulatingPublisher).As<IPublishEventsFromEventHandlers>().SingleInstance();
			builder.Register(c => c.Resolve<ISyncEventPublisher>()).As<IEventPublisher>().SingleInstance();
			builder.RegisterType<IgnoreDelayedMessages>().As<ISendDelayedMessages>().SingleInstance();
		}
	}

	public class HangfireOrSyncEventsPublisherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.Register(c => c.Resolve<IEventPopulatingPublisher>() as EventPopulatingPublisher).As<IPublishEventsFromEventHandlers>().SingleInstance();
			builder.RegisterType<HangfireOrSyncEventPublisher>().As<IEventPublisher>().SingleInstance();
			builder.RegisterType<IgnoreDelayedMessages>().As<ISendDelayedMessages>().SingleInstance();
		}
	}

}