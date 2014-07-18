using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public class PersonScheduleDayReadModelUpdater :
		IHandleEvent<ProjectionChangedEvent>, 
		IHandleEvent<ProjectionChangedEventForPersonScheduleDay>,
		IHandleEvent<PersonTerminatedEvent>
	{
		private readonly IPersonScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IPersonScheduleDayReadModelPersister _scheduleDayReadModelRepository;

		private readonly static ILog Logger = LogManager.GetLogger(typeof(PersonScheduleDayReadModelUpdater));

		public PersonScheduleDayReadModelUpdater(
			IPersonScheduleDayReadModelsCreator scheduleDayReadModelsCreator,
			IPersonScheduleDayReadModelPersister scheduleDayReadModelRepository)
		{
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			createReadModel(@event);
		}

		public void Handle(ProjectionChangedEventForPersonScheduleDay @event)
		{
			createReadModel(@event);
		}

		public void Handle(PersonTerminatedEvent @event)
		{
			_scheduleDayReadModelRepository.UpdateReadModels(new DateOnlyPeriod(new DateOnly(@event.TerminationDate), DateOnly.MaxValue), @event.PersonId, @event.BusinessUnitId, null, false);
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