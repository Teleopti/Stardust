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
		IHandleEvent<PersonTerminatedEvent>
	{
		private readonly IPersonScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IPersonScheduleDayReadModelPersister _scheduleDayReadModelRepository;
		private readonly IEventTracker _eventTracker;

		private readonly static ILog Logger = LogManager.GetLogger(typeof(PersonScheduleDayReadModelUpdater));

		public PersonScheduleDayReadModelUpdater(
			IPersonScheduleDayReadModelsCreator scheduleDayReadModelsCreator,
			IPersonScheduleDayReadModelPersister scheduleDayReadModelRepository,
			IEventTracker eventTracker)
		{
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
			_eventTracker = eventTracker;
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			createReadModel(@event);
			if (_eventTracker != null)
				_eventTracker.SendTrackingMessage(@event.InitiatorId, @event.BusinessUnitId, new Guid("e6b86ea3-6479-48a2-b8d4-54bd6cbbdbc5"));
		}

		public void Handle(ProjectionChangedEventForPersonScheduleDay @event)
		{
			createReadModel(@event);
		}

		public void Handle(PersonTerminatedEvent @event)
		{
			_scheduleDayReadModelRepository.UpdateReadModels(new DateOnlyPeriod(new DateOnly(@event.TerminationDate).AddDays(1), DateOnly.MaxValue), @event.PersonId, @event.BusinessUnitId, null, false);
		}

		private void createReadModel(ProjectionChangedEventBase message)
		{
			if (Logger.IsDebugEnabled)
				Logger.Debug("Updating model PersonScheduleDayReadModel");

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
			_scheduleDayReadModelRepository.UpdateReadModels(dateOnlyPeriod, message.PersonId, message.BusinessUnitId, readModels, initialLoad: message.IsInitialLoad);
		}
	}
}