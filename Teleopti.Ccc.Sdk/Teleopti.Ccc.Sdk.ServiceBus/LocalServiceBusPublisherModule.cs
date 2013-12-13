using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class LocalServiceBusPublisherModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<EventContextPopulator>().As<IEventContextPopulator>();
			builder.RegisterType<LocalServiceBusPublisher>()
			   .As<IPublishEventsFromEventHandlers>()
			   .As<ISendDelayedMessages>()
			   .SingleInstance();
			builder.RegisterType<GetUpdatedScheduleChangeFromTeleoptiRtaService>()
			       .As<IGetUpdatedScheduleChangeFromTeleoptiRtaService>()
			       .SingleInstance();
		}
	}
}