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
            Logger.Info("Start consuming PersonalWithExternalLogonOn message.");

			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				startTime =
					_scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow, message.PersonId);
				Logger.InfoFormat("Next activity for Person: {0}, StartTime: {1}.", message.PersonId, startTime);
			}

			if (startTime.Date.Equals(new DateTime().Date)) return;
			
			try
			{
                _teleoptiRtaService.GetUpdatedScheduleChange(message.PersonId, message.BusinessUnitId, DateTime.UtcNow);
				Logger.InfoFormat("Message successfully send to TeleoptiRtaService BU: {0}, Person: {1}, TimeStamp: {2}.", message.BusinessUnitId, message.PersonId, DateTime.UtcNow);
			}
			catch (Exception exception)
			{
                Logger.Error("Exception occured when calling TeleoptiRtaService", exception);
			}

			_serviceBus.DelaySend(startTime.ToLocalTime(), new PersonWithExternalLogOn
				{
				Datasource = message.Datasource,
				BusinessUnitId = message.BusinessUnitId,
				PersonId = message.PersonId,
				Timestamp = DateTime.Now
			});
			Logger.InfoFormat("Delay Message successfully sent to ServiceBus BU: {0}, Person: {1}, SendTime: {2}.", message.BusinessUnitId, message.PersonId, startTime);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "excpetion"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void Consume(UpdatedScheduleDay message)
		{
            Logger.Info("Start consuming UpdatedScheduleDay message.");

		    if (message.ActivityStartDateTime > DateTime.UtcNow.AddDays(1) || message.ActivityEndDateTime < DateTime.UtcNow)
		    {
				Logger.Info("Updated activity is not within the range of today or tomorow. Ignoring this update.");
		        return;
		    }

		    DateTime startTime;
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				startTime =
					_scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow, message.PersonId);
				if (startTime.Date.Equals(new DateTime().Date))
				{
					Logger.Info("No next activity found. Ignoring this update");
					return;
				}
				Logger.InfoFormat("Next activity for Person: {0}, StartTime: {1}.", message.PersonId, startTime);
			}
			
			//send message to the web service.
			try
			{
                _teleoptiRtaService.GetUpdatedScheduleChange(message.PersonId, message.BusinessUnitId, DateTime.UtcNow);
				Logger.InfoFormat("Message successfully send to TeleoptiRtaService BU: {0}, Person: {1}, TimeStamp: {2}.", message.BusinessUnitId, message.PersonId, DateTime.UtcNow);
				
			}
			catch (Exception exception)
			{
                Logger.Error("Exception occured when calling TeleoptiRtaService", exception);
			}
			
			_serviceBus.DelaySend(startTime.ToLocalTime(), new PersonWithExternalLogOn
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
