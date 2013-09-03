using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationRtaQueue;
using Teleopti.Ccc.Domain.Repositories;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.Rta
{
    public class BusinessUnitInfoConsumer : ConsumerOf<BusinessUnitInfo>
    {
        private readonly IStatisticRepository _statisticRepository;
        private readonly IServiceBus _serviceBus;
        private readonly static ILog Logger = LogManager.GetLogger(typeof(BusinessUnitInfoConsumer));

        public BusinessUnitInfoConsumer(IStatisticRepository statisticRepository, IServiceBus serviceBus)
        {
            _statisticRepository = statisticRepository;
            _serviceBus = serviceBus;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Consume(BusinessUnitInfo message)
        {
           var persons = _statisticRepository.PersonIdsWithExternalLogOn(message.BusinessUnitId);

            foreach (var person in persons)
            {
                try
                {
                    _serviceBus.Send(new PersonActivityStarting
                        {
                            Datasource = message.Datasource,
                            BusinessUnitId = message.BusinessUnitId,
                            PersonId = person,
                            Timestamp = DateTime.UtcNow
                        });
                    
                    Logger.DebugFormat("Sending PersonActivityStarting Message to Service Bus for Person={0} and Bussiness Unit Id={1}", person, message.BusinessUnitId);
                }
                catch (Exception exception)
                {
                    Logger.Error("Exception occured while sending PersonActivityStarting message to Service Bus" , exception);
                    return;
                }
                
            }
        }
    }
}
