﻿using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public class PersonScheduleDayReadModelHandler : 
		IHandleEvent<ProjectionChangedEvent>, 
		IHandleEvent<ProjectionChangedEventForPersonScheduleDay>
	{
		private readonly IPersonScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IPersonScheduleDayReadModelPersister _scheduleDayReadModelRepository;

		public PersonScheduleDayReadModelHandler(
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

			//if (!message.IsInitialLoad)
			//{
			//	_scheduleDayReadModelRepository.ClearPeriodForPerson(dateOnlyPeriod, message.PersonId);
			//}

			var readModels = _scheduleDayReadModelsCreator.GetReadModels(message);
			_scheduleDayReadModelRepository.UpdateReadModels(dateOnlyPeriod, message.PersonId, message.BusinessUnitId, readModels, skipClear: message.IsInitialLoad);
			//foreach (var readModel in readModels)
			//{
			//	_scheduleDayReadModelRepository.SaveReadModels(readModel);
			//}
		}

	}

}