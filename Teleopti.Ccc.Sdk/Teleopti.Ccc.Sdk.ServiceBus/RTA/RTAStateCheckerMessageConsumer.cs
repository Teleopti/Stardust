using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.RTA
{
    public class RTAStateCheckerMessageConsumer : ConsumerOf<RTAStateCheckerMessage>
    {
        private readonly IStatisticRepository _statisticRepository;
        private readonly IServiceBus _serviceBus;

        public RTAStateCheckerMessageConsumer(IStatisticRepository statisticRepository, IServiceBus serviceBus)
        {
            _statisticRepository = statisticRepository;
            _serviceBus = serviceBus;
        }

        public void Consume(RTAStateCheckerMessage message)
        {
           var persons = _statisticRepository.PersonIdsWithExternalLogOn();

            foreach (var person in persons)
            {
                _serviceBus.Send(new RTAPersonInfoMessage { Datasource = message.Datasource,
                                                            BusinessUnitId = message.BusinessUnitId,
                                                            PersonId = person, 
                                                            ActivityStartDateTime = message.Timestamp, 
                                                            ActivityEndDateTime = message.Timestamp,
                                                            Timestamp = message.Timestamp});
            }
        }
    }
}
