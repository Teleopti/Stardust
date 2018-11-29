using System;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public class PersonScheduleDayReadModelUpdaterPersister
	{
		private readonly IPersonScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IPersonScheduleDayReadModelPersister _scheduleDayReadModelRepository;
		private readonly ITrackingMessageSender _trackingMessageSender;
		private static readonly ILog logger = LogManager.GetLogger(typeof(PersonScheduleDayReadModelUpdaterPersister));

		public PersonScheduleDayReadModelUpdaterPersister(IPersonScheduleDayReadModelsCreator scheduleDayReadModelsCreator, IPersonScheduleDayReadModelPersister scheduleDayReadModelRepository, ITrackingMessageSender trackingMessageSender)
		{
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
			_trackingMessageSender = trackingMessageSender;
		}

		public void Execute(ProjectionChangedEventBase @event)
		{
			createReadModel(@event);
			if (_trackingMessageSender != null && @event.CommandId != Guid.Empty)
				_trackingMessageSender.SendTrackingMessage(@event, new TrackingMessage
				{
					TrackId = @event.CommandId,
					Status = TrackingMessageStatus.Success
				});
		}
		
		public void Execute(ProjectionChangedEventForPersonScheduleDay @event)
		{
			createReadModel(@event);
		}
		
		private void createReadModel(ProjectionChangedEventBase message)
		{
			logger.Debug($"Updating model PersonScheduleDayReadModel for person {message.PersonId}");

			if (!message.IsDefaultScenario)
			{
				logger.Debug("Skipping update of model PersonScheduleDayReadModel because its not in default scenario");
				return;
			}
			if (message.ScheduleDays == null || message.ScheduleDays.Count == 0)
			{
				logger.Debug("Skipping update of model PersonScheduleDayReadModel because the event did not contain any days");
				return;
			}

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(message.ScheduleDays.Min(s => s.Date.Date)),
				new DateOnly(message.ScheduleDays.Max(s => s.Date.Date)));

			var readModels = _scheduleDayReadModelsCreator.MakeReadModels(message);
			_scheduleDayReadModelRepository.UpdateReadModels(dateOnlyPeriod, message.PersonId, message.LogOnBusinessUnitId, readModels, message.IsInitialLoad);
		}
	}
}