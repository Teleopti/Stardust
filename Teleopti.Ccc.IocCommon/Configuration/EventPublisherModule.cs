using Autofac;
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
			builder.RegisterType<EventContextPopulator>().As<IEventContextPopulator>();

			builder.RegisterType<SyncEventPublisher>().As<ISyncEventPublisher>();
			builder.RegisterType<HangfireEventPublisher>().As<IHangfireEventPublisher>();
			builder.RegisterType<ServiceBusEventPublisher>().As<IServiceBusEventPublisher>();
		}
	}
}