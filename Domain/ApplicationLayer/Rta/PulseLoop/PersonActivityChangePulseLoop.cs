using System;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop
{
	public class PersonActivityChangePulseLoop : 
		IHandleEvent<PersonActivityChangePulseEvent>,
		IHandleEvent<ScheduleProjectionReadOnlyChanged>,
		IRunOnServiceBus
	{
		private readonly IDelayedMessageSender _serviceBus;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
		private readonly IPersonRepository _personRepository;
        private readonly INotifyRtaToCheckForActivityChange _teleoptiRtaService;
        private readonly static ILog Logger = LogManager.GetLogger(typeof(PersonActivityChangePulseLoop));

		public PersonActivityChangePulseLoop(
			IDelayedMessageSender serviceBus, 
			IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, 
			INotifyRtaToCheckForActivityChange teleoptiRtaService, 
			IPersonRepository personRepository)
		{
			_serviceBus = serviceBus;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
	        _teleoptiRtaService = teleoptiRtaService;
			_personRepository = personRepository;
		}

		public void Handle(PersonActivityChangePulseEvent message)
		{
            Logger.Info("Start consuming PersonalWithExternalLogonOn message.");
			
			if (!message.PersonHaveExternalLogOn && !doesPersonHaveExternalLogOn(message.PersonId)) return;
			
			try
			{
                _teleoptiRtaService.CheckForActivityChange(message.PersonId, message.LogOnBusinessUnitId, DateTime.UtcNow);
				Logger.InfoFormat("Message successfully send to TeleoptiRtaService BU: {0}, Person: {1}, TimeStamp: {2}.", message.LogOnBusinessUnitId, message.PersonId, DateTime.UtcNow);
			}
			catch (Exception exception)
			{
                Logger.Error("Exception occured when calling TeleoptiRtaService", exception);
			}

			DateTime? startTime = _scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow, message.PersonId);
			if (!startTime.HasValue)
			{
				Logger.InfoFormat("No next activity found for Person: {0}. Not putting message on the queue", message.PersonId);
				return;
			}
			Logger.InfoFormat("Next activity for Person: {0}, StartTime: {1}.", message.PersonId, startTime);

			_serviceBus.DelaySend(
				TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(startTime.Value, DateTimeKind.Unspecified), TimeZoneInfo.Local),
				new PersonActivityChangePulseEvent
					{
						LogOnDatasource = message.LogOnDatasource,
						LogOnBusinessUnitId = message.LogOnBusinessUnitId,
						PersonId = message.PersonId,
						Timestamp = DateTime.UtcNow
					});
			Logger.InfoFormat("Delay Message successfully sent to ServiceBus BU: {0}, Person: {1}, SendTime: {2}.", message.LogOnBusinessUnitId, message.PersonId, startTime);
		}

		public void Handle(ScheduleProjectionReadOnlyChanged message)
		{
            Logger.Info("Start consuming ScheduleProjectionReadOnlyChanged message.");

			if (!doesPersonHaveExternalLogOn(message.PersonId)) return;
			
			try
			{
                _teleoptiRtaService.CheckForActivityChange(message.PersonId, message.LogOnBusinessUnitId, DateTime.UtcNow);
				Logger.InfoFormat("Message successfully send to TeleoptiRtaService BU: {0}, Person: {1}, TimeStamp: {2}.", message.LogOnBusinessUnitId, message.PersonId, DateTime.UtcNow);
			}
			catch (Exception exception)
			{
                Logger.Error("Exception occured when calling TeleoptiRtaService", exception);
			}

			DateTime? startTime = _scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow, message.PersonId);
			if (!startTime.HasValue || startTime < message.ActivityStartDateTime)
			{
				Logger.InfoFormat(
					"No next activity, or schedule update is after next activity start date: {0} for Person: {1}. Not putting message on the queue",
					message.ActivityStartDateTime,
					message.PersonId);
				return;
			}

			Logger.InfoFormat("Next activity for Person: {0}, StartTime: {1}.", message.PersonId, startTime);
			_serviceBus.DelaySend(
				TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(startTime.Value, DateTimeKind.Unspecified), TimeZoneInfo.Local),
				new PersonActivityChangePulseEvent
					{
						LogOnDatasource = message.LogOnDatasource,
						LogOnBusinessUnitId = message.LogOnBusinessUnitId,
						PersonId = message.PersonId,
						Timestamp = DateTime.UtcNow
					});
			Logger.InfoFormat("Delay Message successfully sent to ServiceBus BU: {0}, Person: {1}, SendTime: {2}.", message.LogOnBusinessUnitId, message.PersonId, startTime);
		}

		private bool doesPersonHaveExternalLogOn(Guid personId)
		{
			if (!_personRepository.DoesPersonHaveExternalLogOn(DateOnly.Today, personId))
			{
				Logger.InfoFormat("Person: {0} is not connected to an External Log On, discarding mesage", personId);
				return false;
			}
			return true;
		}
	}
}
