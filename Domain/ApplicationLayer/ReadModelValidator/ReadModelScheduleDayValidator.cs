﻿using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class ReadModelScheduleDayValidator : IReadModelScheduleDayValidator
	{
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProjectionChangedEventBuilder _builder;
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private readonly IScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;

		public ReadModelScheduleDayValidator(IPersonRepository personRepository, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IProjectionChangedEventBuilder builder, IScheduleDayReadModelRepository scheduleDayReadModelRepository, IScheduleDayReadModelsCreator scheduleDayReadModelsCreator)
		{
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_builder = builder;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
		}

		public bool Validate(IPerson person,IScheduleDay scheduleDay)
		{
			if(scheduleDay == null) return true;

			var fetchedReadModel = FetchFromRepository(person,scheduleDay.DateOnlyAsPeriod.DateOnly);
			var builtReadModel = Build(person,scheduleDay);

			if(builtReadModel == null) return fetchedReadModel == null;
			return builtReadModel.Equals(fetchedReadModel);
		}

		public ScheduleDayReadModel FetchFromRepository(IPerson person,DateOnly date)
		{
			return _scheduleDayReadModelRepository.ReadModelsOnPerson(date,date,person.Id.GetValueOrDefault()).FirstOrDefault();
		}

		public ScheduleDayReadModel Build(IPerson person,IScheduleDay scheduleDay)
		{
			var eventScheduleDay = _builder.BuildEventScheduleDay(scheduleDay);
			return _scheduleDayReadModelsCreator.GetReadModel(eventScheduleDay,person);
		}

		public ScheduleDayReadModel Build(Guid personId,DateOnly date)
		{
			var person = _personRepository.Get(personId);
			var scenario = _currentScenario.Current();
			var schedule = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false,false),
				date.ToDateTimePeriod(TimeZoneInfo.Utc),
				scenario);
			var scheduleDay = schedule.SchedulesForDay(date).SingleOrDefault();

			return Build(person,scheduleDay);
		}
	}
}
