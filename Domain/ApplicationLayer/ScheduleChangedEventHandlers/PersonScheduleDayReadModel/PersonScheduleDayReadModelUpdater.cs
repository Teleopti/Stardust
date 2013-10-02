using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public class PersonScheduleDayReadModelUpdater :
		IHandleEvent<ProjectionChangedEvent>, 
		IHandleEvent<ProjectionChangedEventForPersonScheduleDay>
	{
		private readonly IPersonScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IPersonScheduleDayReadModelPersister _scheduleDayReadModelRepository;

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

		private void createReadModel(ProjectionChangedEventBase message)
		{
			if (!message.IsDefaultScenario) return;
			if (message.ScheduleDays == null || message.ScheduleDays.Count == 0) return;

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(message.ScheduleDays.Min(s => s.Date.Date)),
			                                        new DateOnly(message.ScheduleDays.Max(s => s.Date.Date)));

			var readModels = _scheduleDayReadModelsCreator.MakeReadModels(message);
			_scheduleDayReadModelRepository.UpdateReadModels(dateOnlyPeriod, message.PersonId, message.BusinessUnitId, readModels, initialLoad: message.IsInitialLoad);
		}
	}
}