using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationRtaQueue;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class LocalServiceBusPublisherModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
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