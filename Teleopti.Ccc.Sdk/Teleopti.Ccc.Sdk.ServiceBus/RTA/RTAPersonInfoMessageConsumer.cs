using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.TeleoptiRtaService;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.RTA
{
	public class RTAPersonInfoMessageConsumer : ConsumerOf<RTAPersonInfoMessage>, ConsumerOf<RTAUpdatedScheduleDayMessage>
	{
		private readonly IServiceBus _serviceBus;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public RTAPersonInfoMessageConsumer(IServiceBus serviceBus, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_serviceBus = serviceBus;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void Consume(RTAPersonInfoMessage message)
		{
			DateTime startTime;
			// get the next activity start time for that person.
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				startTime = DateTime.SpecifyKind(_scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow, message.PersonId), DateTimeKind.Utc);
			}

			// send delay message to the service bus according to the next activity start time.
			if (startTime.Date.Equals(new DateTime().Date)) return;

			// send RTA Webservice message consist of personid.
			// Call Webservice method through web service proxy instead of sending message through message broker.
			try
			{
				var teleoptiRtaServiceClient = new TeleoptiRtaServiceClient();
				teleoptiRtaServiceClient.GetUpdatedScheduleChange(message.PersonId, message.BusinessUnitId, message.Datasource,
				                                                  DateTime.UtcNow);
			}
			catch (Exception e)
			{
				var a = e.Message;
			}
			_serviceBus.DelaySend(startTime, new RTAPersonInfoMessage
				{
				Datasource = message.Datasource,
				BusinessUnitId = message.BusinessUnitId,
				PersonId = message.PersonId,
				Timestamp = DateTime.UtcNow
			});
		}

		public void Consume(RTAUpdatedScheduleDayMessage message)
		{
			//if (message.ActivityStartDateTime.Date == DateTime.UtcNow.Date || message.ActivityStartDateTime.Date == DateTime.UtcNow.AddDays(1).Date)
			if (message.ActivityStartDateTime > DateTime.UtcNow.AddDays(1) || message.ActivityEndDateTime < DateTime.UtcNow)
				return;
			
			//send message to the web service.
			var teleoptiRtaServiceClient = new TeleoptiRtaServiceClient();
			teleoptiRtaServiceClient.GetUpdatedScheduleChange(message.PersonId, message.BusinessUnitId, message.Datasource, DateTime.UtcNow);

			DateTime startTime;
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				startTime = _scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow, message.PersonId);
			}

			// send delay message to the service bus according to the next activity start time.
			if (!startTime.Date.Equals(new DateTime().Date))
			{
				_serviceBus.DelaySend(startTime, new RTAPersonInfoMessage
					{
						Datasource = message.Datasource,
						BusinessUnitId = message.BusinessUnitId,
						PersonId = message.PersonId,
						Timestamp = DateTime.UtcNow
					});
			}
		}
	}
}
