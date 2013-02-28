using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.TeleoptiRtaService;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Rta;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.Rta
{
	public class UpdatedScheduleInfoConsumer : ConsumerOf<PersonWithExternalLogOn>, ConsumerOf<UpdatedScheduleDay>
	{
		private readonly IServiceBus _serviceBus;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ITeleoptiRtaService _teleoptiRtaService;
        private readonly static ILog Logger = LogManager.GetLogger(typeof(UpdatedScheduleInfoConsumer));

        public UpdatedScheduleInfoConsumer(IServiceBus serviceBus, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, IUnitOfWorkFactory unitOfWorkFactory, ITeleoptiRtaService teleoptiRtaService)
		{
			_serviceBus = serviceBus;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
	        _teleoptiRtaService = teleoptiRtaService;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "exception"),
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void Consume(PersonWithExternalLogOn message)
		{
			DateTime startTime;
		    string infoMessage = "";
            Logger.Info("Start consuming PersonalWithExternalLogonOn message.");

			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				startTime =
					_scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow, message.PersonId).ToLocalTime();
                
                infoMessage = "Next activity start time =" + startTime +  " for personId=" + message.PersonId;
                Logger.Info(infoMessage);
			}

			if (startTime.Date.Equals(new DateTime().Date)) return;
			
			try
			{
			    infoMessage = "Calling TeleoptiRtaService for person=" + message.PersonId + " at " + DateTime.UtcNow;
                Logger.Info(infoMessage);

                _teleoptiRtaService.GetUpdatedScheduleChange(message.PersonId, message.BusinessUnitId, DateTime.UtcNow);

                infoMessage = "Message successfully send to TeleoptiRtaService for person=" + message.PersonId + " at " + DateTime.UtcNow;
                Logger.Info(infoMessage);
				
			}
			catch (Exception exception)
			{
                Logger.Error("Exception occured when calling TeleoptiRtaService", exception);
			}

			_serviceBus.DelaySend(startTime, new PersonWithExternalLogOn
				{
				Datasource = message.Datasource,
				BusinessUnitId = message.BusinessUnitId,
				PersonId = message.PersonId,
				Timestamp = DateTime.Now
			});

            infoMessage = "Delay Message successfully send to Service Bus where startTime ="+ startTime + " for person=" + message.PersonId + " and Business Unit at "+ message.BusinessUnitId;
            Logger.Info(infoMessage);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "excpetion"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void Consume(UpdatedScheduleDay message)
		{
            string infoMessage = "";
            Logger.Info("Start consuming UpdatedScheduleDay message.");

			//if (message.ActivityStartDateTime.Date == DateTime.UtcNow.Date || message.ActivityStartDateTime.Date == DateTime.UtcNow.AddDays(1).Date)
		    if (message.ActivityStartDateTime > DateTime.UtcNow.AddDays(1) || message.ActivityEndDateTime < DateTime.UtcNow)
		    {
		        infoMessage = "Updated activity is not within today or tomorow range. Ignoring this update.";
		        Logger.Info(infoMessage);
		        return;
		    }

		    DateTime startTime;
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				startTime =
					_scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow, message.PersonId).ToLocalTime();
infoMessage = "Next activity start time =" + startTime + " for person "+ message.PersonId;
                Logger.Info(infoMessage);
			}

			if (startTime.Date.Equals(new DateTime().Date))
				return;

			//send message to the web service.
			try
			{
                infoMessage = "Calling TeleoptiRtaService for person="+ message.PersonId + " at " + DateTime.UtcNow;
                Logger.Info(infoMessage);

                _teleoptiRtaService.GetUpdatedScheduleChange(message.PersonId, message.BusinessUnitId, DateTime.UtcNow);

                infoMessage = "Message successfully send to TeleoptiRtaService for person= "+ message.PersonId +" at "+ DateTime.UtcNow;
                Logger.Info(infoMessage);
				
			}
			catch (Exception exception)
			{
                Logger.Error("Exception occured when calling TeleoptiRtaService", exception);
			}
			
			_serviceBus.DelaySend(startTime, new PersonWithExternalLogOn
				{
					Datasource = message.Datasource,
					BusinessUnitId = message.BusinessUnitId,
					PersonId = message.PersonId,
					Timestamp = DateTime.Now
				});

            infoMessage = "Delay Message successfully send to Service Bus where startTime ="+ startTime +" for person="+message.PersonId+" and Business Unit at " + message.BusinessUnitId;
            Logger.Info(infoMessage);
		}
	}
}
