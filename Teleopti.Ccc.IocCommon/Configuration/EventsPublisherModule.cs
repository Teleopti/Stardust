using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class LocalEventsPublisherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SyncEventsPublisher>().As<IEventsPublisher>().SingleInstance();
			builder.RegisterType<EventPublisher>().As<IEventPublisher>().SingleInstance();
			builder.RegisterType<AutofacResolve>().As<IResolve>().SingleInstance();
		}
	}

	public class ServiceBusEventsPublisherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SyncEventsPublisher>().As<IEventsPublisher>().SingleInstance();
			builder.RegisterType<ServiceBusEventPublisher>().As<IEventPublisher>().SingleInstance();
		}
	}

	public class DenormalizationQueueEventsPublisherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<DenormalizationQueueEventsPublisher>().As<IEventsPublisher>().SingleInstance();
			builder.RegisterType<SaveToDenormalizationQueue>().As<ISaveToDenormalizationQueue>().SingleInstance();
		}
	}

}