using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using log4net;

namespace Teleopti.Ccc.Domain.ApplicationRtaQueue
{
	public class UpdatedScheduleInfoHandler :
		IHandleEvent<PersonWithExternalLogOn>,
		IHandleEvent<UpdatedScheduleDay>
	{
		private readonly ISendDelayedMessages _serviceBus;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
		private readonly IGetUpdatedScheduleChangeFromTeleoptiRtaService _teleoptiRtaService;
		private static readonly ILog Logger = LogManager.GetLogger(typeof (UpdatedScheduleInfoHandler));

		public UpdatedScheduleInfoHandler(
			ISendDelayedMessages serviceBus,
			IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository,
			IGetUpdatedScheduleChangeFromTeleoptiRtaService teleoptiRtaService)
		{
			_serviceBus = serviceBus;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
			_teleoptiRtaService = teleoptiRtaService;
		}

		public void Handle(PersonWithExternalLogOn message)
		{
			Logger.Info("Start consuming PersonalWithExternalLogonOn message.");

			try
			{
				_teleoptiRtaService.GetUpdatedScheduleChange(message.PersonId, message.BusinessUnitId, DateTime.UtcNow);
				Logger.InfoFormat("Message successfully sent to TeleoptiRtaService BU: {0}, Person: {1}, TimeStamp: {2}.",
				                  message.BusinessUnitId, message.PersonId, DateTime.UtcNow);
			}
			catch (Exception exception)
			{
				Logger.Error("Exception occured when calling TeleoptiRtaService", exception);
			}

			DateTime? nextActivityStartTime = _scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
																								 message.PersonId);
			if (!nextActivityStartTime.HasValue)
			{
				Logger.InfoFormat("No next activity found for Person: {0}. Not putting message on the queue", message.PersonId);
				return;
			}
			Logger.InfoFormat("Next activity for Person: {0}, StartTime: {1}.", message.PersonId, nextActivityStartTime);

			_serviceBus.DelaySend(
				TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(nextActivityStartTime.Value, DateTimeKind.Unspecified),
				                                TimeZoneInfo.Local),
				new PersonWithExternalLogOn
					{
						Datasource = message.Datasource,
						BusinessUnitId = message.BusinessUnitId,
						PersonId = message.PersonId,
						Timestamp = DateTime.Now
					});
			Logger.InfoFormat("Delay Message successfully sent to ServiceBus BU: {0}, Person: {1}, SendTime: {2}.",
			                  message.BusinessUnitId, message.PersonId, nextActivityStartTime);
		}

		public void Handle(UpdatedScheduleDay message)
		{
			Logger.Info("Start consuming UpdatedScheduleDay message.");
			
			try
			{
				_teleoptiRtaService.GetUpdatedScheduleChange(message.PersonId, message.BusinessUnitId, DateTime.UtcNow);
				Logger.InfoFormat("Message successfully send to TeleoptiRtaService BU: {0}, Person: {1}, TimeStamp: {2}.",
				                  message.BusinessUnitId, message.PersonId, DateTime.UtcNow);
			}
			catch (Exception exception)
			{
				Logger.Error("Exception occured when calling TeleoptiRtaService", exception);
			}
			
			DateTime? nextActivityStartTime = _scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
																											 message.PersonId);
			if (!nextActivityStartTime.HasValue || nextActivityStartTime < message.ActivityStartDateTime)
			{
				Logger.InfoFormat(
					"No next activity, or schedule update is after next activity start date: {0} for Person: {1}. Not putting message on the queue",
					message.ActivityStartDateTime,
					message.PersonId);
				return;
			}

			_serviceBus.DelaySend(
				TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(nextActivityStartTime.Value, DateTimeKind.Unspecified),
				                                TimeZoneInfo.Local),
				new PersonWithExternalLogOn
					{
						Datasource = message.Datasource,
						BusinessUnitId = message.BusinessUnitId,
						PersonId = message.PersonId,
						Timestamp = DateTime.UtcNow
					});
			Logger.InfoFormat("Delay Message successfully sent to ServiceBus BU: {0}, Person: {1}, SendTime: {2}.",
			                  message.BusinessUnitId, message.PersonId, nextActivityStartTime);
		}
	}
}
