using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
#pragma warning disable 612,618
    public class LegacyMessagesConsumer : ConsumerOf<ScheduleChanged>, ConsumerOf<DenormalizeScheduleProjection>
#pragma warning restore 612,618
    {
        private readonly IServiceBus _bus;

        public LegacyMessagesConsumer(IServiceBus bus)
        {
            _bus = bus;
        }

#pragma warning disable 612,618
        public void Consume(DenormalizeScheduleProjection message)
#pragma warning restore 612,618
        {
            _bus.SendToSelf(new ScheduleChangedEvent
                {
                    LogOnBusinessUnitId = message.LogOnBusinessUnitId,
                    LogOnDatasource = message.LogOnDatasource,
                    PersonId = message.PersonId,
                    ScenarioId = message.ScenarioId,
                    SkipDelete = message.SkipDelete,
                    StartDateTime = message.StartDateTime,
                    EndDateTime = message.EndDateTime,
                    Timestamp = message.Timestamp
                });
        }

#pragma warning disable 612,618
        public void Consume(ScheduleChanged message)
#pragma warning restore 612,618
        {
            _bus.SendToSelf(new ScheduleChangedEvent
                {
                    LogOnBusinessUnitId = message.LogOnBusinessUnitId,
                    LogOnDatasource = message.LogOnDatasource,
                    PersonId = message.PersonId,
                    ScenarioId = message.ScenarioId,
                    SkipDelete = message.SkipDelete,
                    StartDateTime = message.StartDateTime,
                    EndDateTime = message.EndDateTime,
                    Timestamp = message.Timestamp
                });
        }
    }
}