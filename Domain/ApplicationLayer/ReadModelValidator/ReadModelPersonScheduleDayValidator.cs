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
			if (!scheduleDay.IsScheduled() && fetchedReadModel == null)
				return true;
			return builtReadModel.Equals(fetchedReadModel);
		}

		public PersonScheduleDayReadModel FetchFromRepository(IPerson person,DateOnly date)
		{
			return _personScheduleDayReadModelFinder.ForPerson(date,person.Id.GetValueOrDefault());
		}

		public PersonScheduleDayReadModel Build(Guid personId,DateOnly date)
		{
			var person = _personRepository.Get(personId);
			var period = new DateOnlyPeriod(date,date);
			var extendedDateOnlyPeriodBecauseWeDontWantToConvertToEachPersonsTimeZone = period.Inflate(1);

			var scenario = _currentScenario.Current();
			var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false,false),
				extendedDateOnlyPeriodBecauseWeDontWantToConvertToEachPersonsTimeZone.ToDateTimePeriod(TimeZoneInfo.Utc),
				scenario);

			var scheduleDays = schedules.SchedulesForPeriod(period,person).ToLookup(s => s.DateOnlyAsPeriod.DateOnly);

			return Build(person,scheduleDays[date].First());		
		}

		public PersonScheduleDayReadModel Build(IPerson person,IScheduleDay scheduleDay)
		{
			var eventScheduleDay = _builder.BuildEventScheduleDay(scheduleDay);
			var readModel = _personScheduleDayReadModelsCreator.MakePersonScheduleDayReadModel(person, eventScheduleDay);
			if (readModel != null)
				readModel.BusinessUnitId = _currentScenario.Current().BusinessUnit.Id.GetValueOrDefault();
			return readModel;
		}
	}
}
