using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

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
		private readonly IReadModelFixer _readModelFixer;

		public ReadModelPersonScheduleDayValidator(IPersonRepository personRepository, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IProjectionChangedEventBuilder builder, IPersonScheduleDayReadModelFinder personScheduleDayReadModelFinder, IPersonScheduleDayReadModelsCreator personScheduleDayReadModelsCreator, IReadModelFixer readModelFixer)
		{
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_builder = builder;
			_personScheduleDayReadModelFinder = personScheduleDayReadModelFinder;
			_personScheduleDayReadModelsCreator = personScheduleDayReadModelsCreator;
			_readModelFixer = readModelFixer;
		}

		public bool IsInitialized()
		{
			return _personScheduleDayReadModelFinder.IsInitialized();
		}

		public bool Validate(IPerson person,IScheduleDay scheduleDay,ReadModelValidationMode mode)
		{
		
			var builtReadModel = Build(person,scheduleDay);

			var isValid = false;

			if (mode != ReadModelValidationMode.Reinitialize)
			{
				var fetchedReadModel = FetchFromRepository(person,scheduleDay.DateOnlyAsPeriod.DateOnly);
				if(!scheduleDay.IsScheduled() && fetchedReadModel == null)
					return true;

				isValid = builtReadModel?.Equals(fetchedReadModel) ?? fetchedReadModel == null;
			}
		
			if (!isValid && (mode == ReadModelValidationMode.ValidateAndFix || mode == ReadModelValidationMode.Reinitialize))
			{
				_readModelFixer.FixPersonScheduleDay(new ReadModelData
				{
					Date = scheduleDay.DateOnlyAsPeriod.DateOnly,
					PersonId = person.Id.Value,
					PersonScheduleDay = builtReadModel
				});
			}						
			return isValid;
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
			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var personPeriod = scheduleDay.Person.Period(date);

			ProjectionChangedEventScheduleDay eventScheduleDay = null;
			if (personPeriod != null)
			{
				eventScheduleDay = _builder.BuildEventScheduleDay(scheduleDay);
			}
			
			var readModel = _personScheduleDayReadModelsCreator.MakePersonScheduleDayReadModel(person, eventScheduleDay);			
			return readModel;
		}
	}
}
