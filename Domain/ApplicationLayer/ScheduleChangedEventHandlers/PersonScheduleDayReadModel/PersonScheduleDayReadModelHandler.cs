﻿using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public class PersonScheduleDayReadModelHandler : 
		IHandleEvent<ProjectionChangedEvent>, 
		IHandleEvent<ProjectionChangedEventForPersonScheduleDay>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPersonScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IPersonScheduleDayReadModelRepository _scheduleDayReadModelRepository;

		public PersonScheduleDayReadModelHandler(IUnitOfWorkFactory unitOfWorkFactory,
									IPersonScheduleDayReadModelsCreator scheduleDayReadModelsCreator,
									IPersonScheduleDayReadModelRepository scheduleDayReadModelRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ProjectionChangedEvent @event)
		{
			createReadModel(@event);
		}

		private void createReadModel(ProjectionChangedEventBase message)
		{
			if (!message.IsDefaultScenario) return;
			if (message.ScheduleDays == null || message.ScheduleDays.Count == 0) return;

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(message.ScheduleDays.Min(s => s.Date.Date)),
			                                        new DateOnly(message.ScheduleDays.Max(s => s.Date.Date)));

			if (!message.IsInitialLoad)
			{
				_scheduleDayReadModelRepository.ClearPeriodForPerson(dateOnlyPeriod, message.PersonId);
			}

			var readModels = _scheduleDayReadModelsCreator.GetReadModels(message);
			foreach (var readModel in readModels)
			{
				_scheduleDayReadModelRepository.SaveReadModel(readModel);
			}
		}

		public void Handle(ProjectionChangedEventForPersonScheduleDay @event)
		{
			createReadModel(@event);
		}
	}

}