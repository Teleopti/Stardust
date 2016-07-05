using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class ReadModelPersonScheduleDayValidator : IReadModelPersonScheduleDayValidator
	{
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProjectionChangedEventBuilder _builder;
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelFinder;
		private readonly IPersonScheduleDayReadModelsCreator _personScheduleDayReadModelsCreator;

		public ReadModelPersonScheduleDayValidator(IPersonRepository personRepository, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IProjectionChangedEventBuilder builder, IPersonScheduleDayReadModelFinder personScheduleDayReadModelFinder, IPersonScheduleDayReadModelsCreator personScheduleDayReadModelsCreator)
		{
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_builder = builder;
			_personScheduleDayReadModelFinder = personScheduleDayReadModelFinder;
			_personScheduleDayReadModelsCreator = personScheduleDayReadModelsCreator;
		}


		public bool Validate(IPerson person,IScheduleDay scheduleDay)
		{
			var fetchedReadModel = FetchFromRepository(person,scheduleDay.DateOnlyAsPeriod.DateOnly);
			var builtReadModel = Build(person,scheduleDay);
			if(builtReadModel == null) return fetchedReadModel == null;
			return builtReadModel.Equals(fetchedReadModel);
		}

		public PersonScheduleDayReadModel FetchFromRepository(IPerson person,DateOnly date)
		{
			return _personScheduleDayReadModelFinder.ForPerson(date,person.Id.GetValueOrDefault());
		}

		public PersonScheduleDayReadModel Build(Guid personId,DateOnly date)
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

		public PersonScheduleDayReadModel Build(IPerson person,IScheduleDay scheduleDay)
		{
			var eventScheduleDay = _builder.BuildEventScheduleDay(scheduleDay);
			return _personScheduleDayReadModelsCreator.MakePersonScheduleDayReadModel(person,eventScheduleDay);
		}
	}
}
