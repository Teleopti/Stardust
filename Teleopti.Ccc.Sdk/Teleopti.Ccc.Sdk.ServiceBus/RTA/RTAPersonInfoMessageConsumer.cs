using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.TeleoptiRtaService;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.RTA
{
    public class RTAPersonInfoMessageConsumer : ConsumerOf<RTAPersonInfoMessage>, ConsumerOf<RTAUpdatedScheduleDayMessage>
    {
        private readonly IServiceBus _serviceBus;
        private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public RTAPersonInfoMessageConsumer(IServiceBus serviceBus, IMessageBroker messageBroker, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, IUnitOfWorkFactory unitOfWorkFactory) 
        {
            _serviceBus = serviceBus;
            _scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public void Consume(RTAPersonInfoMessage message)
        {
            DateTime startTime;
            // send RTA Webservice message consist of personid.
            // Call Webservice method through web service proxy instead of sending message through message broker.

            var teleoptiRtaServiceClient = new TeleoptiRtaServiceClient();
            teleoptiRtaServiceClient.GetUpdatedScheduleChange(message.PersonId, message.ActivityStartDateTime, message.ActivityEndDateTime);
            
            // get the next activity start time for that person.
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                startTime = _scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,message.PersonId);
            }

            // send delay message to the service bus according to the next activity start time.
            if (!startTime.Date.Equals(new DateTime().Date))
            {
                _serviceBus.DelaySend(startTime, new RTAPersonInfoMessage() { Datasource = message.Datasource, 
                                                                              BusinessUnitId = message.BusinessUnitId,
                                                                              PersonId = message.PersonId,
                                                                              Timestamp = DateTime.UtcNow,
                                                                              ActivityStartDateTime = startTime,
                                                                              ActivityEndDateTime = startTime});
            }
        }

        public void Consume(RTAUpdatedScheduleDayMessage message)
        {
            //if (message.ActivityStartDateTime.Date == DateTime.UtcNow.Date || message.ActivityStartDateTime.Date == DateTime.UtcNow.AddDays(1).Date)
            if ( message.ActivityStartDateTime <= DateTime.UtcNow.AddDays(1) && message.ActivityEndDateTime >= DateTime.UtcNow) 
            {
                //send message to the web service.
                var teleoptiRtaServiceClient = new TeleoptiRtaServiceClient();
                teleoptiRtaServiceClient.GetUpdatedScheduleChange(message.PersonId, message.ActivityStartDateTime, message.ActivityEndDateTime);
            }
        }
    }
}
