using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.TeleoptiRtaService;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.RTA
{
	public class UpdatedScheduleInfoConsumer : ConsumerOf<PersonWithExternalLogon>, ConsumerOf<UpdatedScheduleDay>
	{
		private readonly IServiceBus _serviceBus;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public UpdatedScheduleInfoConsumer(IServiceBus serviceBus, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_serviceBus = serviceBus;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void Consume(PersonWithExternalLogon message)
		{
			DateTime startTime;
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				startTime = DateTime.SpecifyKind(_scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow, message.PersonId), DateTimeKind.Utc);
			}

			if (startTime.Date.Equals(new DateTime().Date)) return;

			try
			{
				var teleoptiRtaServiceClient = new TeleoptiRtaServiceClient();
				teleoptiRtaServiceClient.GetUpdatedScheduleChange(message.PersonId, message.BusinessUnitId, message.Datasource,DateTime.UtcNow);
			}
			catch (Exception exp)
			{
				var exception = exp.Message;
			    return;
			}

			_serviceBus.DelaySend(startTime, new PersonWithExternalLogon
				{
				Datasource = message.Datasource,
				BusinessUnitId = message.BusinessUnitId,
				PersonId = message.PersonId,
				Timestamp = DateTime.UtcNow
			});
		}

		public void Consume(UpdatedScheduleDay message)
		{
			//if (message.ActivityStartDateTime.Date == DateTime.UtcNow.Date || message.ActivityStartDateTime.Date == DateTime.UtcNow.AddDays(1).Date)
			if (message.ActivityStartDateTime > DateTime.UtcNow.AddDays(1) || message.ActivityEndDateTime < DateTime.UtcNow)
				return;
			
			//send message to the web service.
		    try
		    {
		        var teleoptiRtaServiceClient = new TeleoptiRtaServiceClient();
		        teleoptiRtaServiceClient.GetUpdatedScheduleChange(message.PersonId, message.BusinessUnitId, message.Datasource,
		                                                          DateTime.UtcNow);
		    }
		    catch (Exception exp)
		    {
		        var excpetion = exp.Message;
		        return;
		    }

		    DateTime startTime;
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				startTime = _scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow, message.PersonId);
			}

			if (!startTime.Date.Equals(new DateTime().Date))
			{
				_serviceBus.DelaySend(startTime, new PersonWithExternalLogon
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
