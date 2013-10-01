using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using log4net;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class UpdatedScheduleInfoHandler : 
		IHandleEvent<PersonActivityStarting>,
		IHandleEvent<ScheduleProjectionReadOnlyChanged>
	{
		private readonly ISendDelayedMessages _serviceBus;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
        private readonly IGetUpdatedScheduleChangeFromTeleoptiRtaService _teleoptiRtaService;
        private readonly static ILog Logger = LogManager.GetLogger(typeof(UpdatedScheduleInfoHandler));

		public UpdatedScheduleInfoHandler(
			ISendDelayedMessages serviceBus, 
			IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, 
			IGetUpdatedScheduleChangeFromTeleoptiRtaService teleoptiRtaService)
		{
			_serviceBus = serviceBus;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
	        _teleoptiRtaService = teleoptiRtaService;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "exception"),
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void Handle(PersonActivityStarting message)
		{
            Logger.Info("Start consuming PersonalWithExternalLogonOn message.");
			
			try
			{
                _teleoptiRtaService.GetUpdatedScheduleChange(message.PersonId, message.BusinessUnitId, DateTime.UtcNow);
				Logger.InfoFormat("Message successfully send to TeleoptiRtaService BU: {0}, Person: {1}, TimeStamp: {2}.", message.BusinessUnitId, message.PersonId, DateTime.UtcNow);
			}
			catch (Exception exception)
			{
                Logger.Error("Exception occured when calling TeleoptiRtaService", exception);
			}

			DateTime? startTime = _scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow, message.PersonId);
			if (startTime == null)
			{
				Logger.InfoFormat("No next activity found for Person: {0}. Not putting message on the queue", message.PersonId);
				return;
			}
			Logger.InfoFormat("Next activity for Person: {0}, StartTime: {1}.", message.PersonId, startTime);

			_serviceBus.DelaySend(startTime.Value, new PersonActivityStarting
				{
				Datasource = message.Datasource,
				BusinessUnitId = message.BusinessUnitId,
				PersonId = message.PersonId,
				Timestamp = DateTime.Now
			});
			Logger.InfoFormat("Delay Message successfully sent to ServiceBus BU: {0}, Person: {1}, SendTime: {2}.", message.BusinessUnitId, message.PersonId, startTime);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "excpetion"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void Handle(ScheduleProjectionReadOnlyChanged message)
		{
            Logger.Info("Start consuming ScheduleProjectionReadOnlyChanged message.");
			
			try
			{
                _teleoptiRtaService.GetUpdatedScheduleChange(message.PersonId, message.BusinessUnitId, DateTime.UtcNow);
				Logger.InfoFormat("Message successfully send to TeleoptiRtaService BU: {0}, Person: {1}, TimeStamp: {2}.", message.BusinessUnitId, message.PersonId, DateTime.UtcNow);
			}
			catch (Exception exception)
			{
                Logger.Error("Exception occured when calling TeleoptiRtaService", exception);
			}

			DateTime? startTime = _scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow, message.PersonId);
			if (startTime == null || startTime < message.ActivityStartDateTime)
			{
				Logger.InfoFormat(
					"No next activity, or schedule update is after next activity start date: {0} for Person: {1}. Not putting message on the queue",
					message.ActivityStartDateTime,
					message.PersonId);
				return;
			}

			Logger.InfoFormat("Next activity for Person: {0}, StartTime: {1}.", message.PersonId, startTime);
			_serviceBus.DelaySend(startTime.Value, new PersonActivityStarting
				{
					Datasource = message.Datasource,
					BusinessUnitId = message.BusinessUnitId,
					PersonId = message.PersonId,
					Timestamp = DateTime.Now
				});
			Logger.InfoFormat("Delay Message successfully sent to ServiceBus BU: {0}, Person: {1}, SendTime: {2}.", message.BusinessUnitId, message.PersonId, startTime);
		}
	}
}
