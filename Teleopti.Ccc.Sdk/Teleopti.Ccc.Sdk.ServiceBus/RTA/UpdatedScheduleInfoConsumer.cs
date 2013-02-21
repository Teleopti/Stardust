using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.TeleoptiRtaService;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Rta
{
	public class UpdatedScheduleInfoConsumer : ConsumerOf<PersonWithExternalLogOn>, ConsumerOf<UpdatedScheduleDay>
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "exception"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void Consume(PersonWithExternalLogOn message)
		{
			DateTime startTime;
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				startTime = DateTime.SpecifyKind(_scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow, message.PersonId), DateTimeKind.Utc);
			}

			if (startTime.Date.Equals(new DateTime().Date)) return;
			
			try
			{
				using (var teleoptiRtaServiceClient = new TeleoptiRtaServiceClient())
				{
					teleoptiRtaServiceClient.GetUpdatedScheduleChange(message.PersonId, message.BusinessUnitId, message.Datasource,
																	  DateTime.UtcNow);
				}
			}
			catch (Exception exp)
			{
				var exception = exp.Message;
			}

			_serviceBus.DelaySend(startTime, new PersonWithExternalLogOn
				{
				Datasource = message.Datasource,
				BusinessUnitId = message.BusinessUnitId,
				PersonId = message.PersonId,
				Timestamp = DateTime.UtcNow
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "excpetion"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void Consume(UpdatedScheduleDay message)
		{
			//if (message.ActivityStartDateTime.Date == DateTime.UtcNow.Date || message.ActivityStartDateTime.Date == DateTime.UtcNow.AddDays(1).Date)
			if (message.ActivityStartDateTime > DateTime.UtcNow.AddDays(1) || message.ActivityEndDateTime < DateTime.UtcNow)
				return;
			
			//send message to the web service.
			try
			{
				using (var teleoptiRtaServiceClient = new TeleoptiRtaServiceClient())
				{
					teleoptiRtaServiceClient.GetUpdatedScheduleChange(message.PersonId, message.BusinessUnitId, message.Datasource,
					                                                  DateTime.UtcNow);
				}
			}
			catch (Exception exp)
			{
				var excpetion = exp.Message;
			}

			DateTime startTime;
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				startTime = _scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow, message.PersonId);
			}

			if (!startTime.Date.Equals(new DateTime().Date))
			{
				_serviceBus.DelaySend(startTime, new PersonWithExternalLogOn
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
