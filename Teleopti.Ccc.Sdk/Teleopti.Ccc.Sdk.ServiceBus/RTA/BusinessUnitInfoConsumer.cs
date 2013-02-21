using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.RTA
{
    public class BusinessUnitInfoConsumer : ConsumerOf<BusinessUnitInfo>
    {
        private readonly IStatisticRepository _statisticRepository;
        private readonly IServiceBus _serviceBus;

        public BusinessUnitInfoConsumer(IStatisticRepository statisticRepository, IServiceBus serviceBus)
        {
            _statisticRepository = statisticRepository;
            _serviceBus = serviceBus;
        }

        public void Consume(BusinessUnitInfo message)
        {
           var persons = _statisticRepository.PersonIdsWithExternalLogOn();

            foreach (var person in persons)
            {
                _serviceBus.Send(new PersonWithExternalLogon { Datasource = message.Datasource,
                                                            BusinessUnitId = message.BusinessUnitId,
                                                            PersonId = person, 
                                                            Timestamp = DateTime.UtcNow});
            }
        }
    }
}
