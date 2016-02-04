using System;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public class PersonScheduleDayReadModelUpdater :
		IHandleEvent<ProjectionChangedEvent>, 
		IHandleEvent<ProjectionChangedEventForPersonScheduleDay>,
		IHandleEvent<PersonTerminalDateChangedEvent>,
		IRunOnServiceBus
	{
		private readonly IPersonScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IPersonScheduleDayReadModelPersister _scheduleDayReadModelRepository;
		private readonly ITrackingMessageSender _trackingMessageSender;

		private readonly static ILog Logger = LogManager.GetLogger(typeof(PersonScheduleDayReadModelUpdater));

		public PersonScheduleDayReadModelUpdater(
			IPersonScheduleDayReadModelsCreator scheduleDayReadModelsCreator,
			IPersonScheduleDayReadModelPersister scheduleDayReadModelRepository,
			ITrackingMessageSender trackingMessageSender)
		{
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
			_trackingMessageSender = trackingMessageSender;
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			createReadModel(@event);
			if (_trackingMessageSender != null && @event.TrackId != Guid.Empty)
				_trackingMessageSender.SendTrackingMessage(@event, new TrackingMessage
				{
					TrackId = @event.TrackId,
					Status = TrackingMessageStatus.Success
				});
		}

		public void Handle(ProjectionChangedEventForPersonScheduleDay @event)
		{
			createReadModel(@event);
		}

		public void Handle(PersonTerminalDateChangedEvent @event)
		{
			if (@event.TerminationDate.HasValue)
				_scheduleDayReadModelRepository.UpdateReadModels(new DateOnlyPeriod(new DateOnly(@event.TerminationDate.Value).AddDays(1), DateOnly.MaxValue), @event.PersonId, @event.LogOnBusinessUnitId, null, true);

		}

		private void createReadModel(ProjectionChangedEventBase message)
		{
			if (Logger.IsDebugEnabled)
				Logger.DebugFormat("Updating model PersonScheduleDayReadModel for person {0}", message.PersonId);

			if (!message.IsDefaultScenario)
			{
				if (Logger.IsDebugEnabled)
					Logger.Debug("Skipping update of model PersonScheduleDayReadModel because its not in default scenario");
				return;
			}
			if (message.ScheduleDays == null || message.ScheduleDays.Count == 0)
			{
				if (Logger.IsDebugEnabled)
					Logger.Debug("Skipping update of model PersonScheduleDayReadModel because the event did not contain any days");
				return;
			}

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(message.ScheduleDays.Min(s => s.Date.Date)),
			                                        new DateOnly(message.ScheduleDays.Max(s => s.Date.Date)));

			var readModels = _scheduleDayReadModelsCreator.MakeReadModels(message);
			_scheduleDayReadModelRepository.UpdateReadModels(dateOnlyPeriod, message.PersonId, message.LogOnBusinessUnitId, readModels, initialLoad: message.IsInitialLoad);
		}
	}
}