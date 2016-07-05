using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{	
	public class ReadModelValidator : IReadModelValidator
	{		
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;

		private readonly IReadModelPersonScheduleDayValidator _readModelPersonScheduleDayValidator;
		private readonly IReadModelScheduleProjectionReadOnlyValidator _readModelScheduleProjectionReadOnlyValidator;
		private readonly IReadModelScheduleDayValidator _readModelScheduleDayValidator;
		
		public ReadModelValidator(IPersonRepository personRepository, 
								IScheduleStorage scheduleStorage, 
								ICurrentScenario currentScenario, 
								IReadModelPersonScheduleDayValidator readModelPersonScheduleDayValidator, 
								IReadModelScheduleProjectionReadOnlyValidator readModelScheduleProjectionReadOnlyValidator, 
								IReadModelScheduleDayValidator readModelScheduleDayValidator)
		{
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_readModelPersonScheduleDayValidator = readModelPersonScheduleDayValidator;
			_readModelScheduleProjectionReadOnlyValidator = readModelScheduleProjectionReadOnlyValidator;
			_readModelScheduleDayValidator = readModelScheduleDayValidator;
		}

		public void Validate(ValidateReadModelType types, DateTime start, DateTime end, Action<ReadModelValidationResult> reportProgress,
			bool ignoreValid = false)
		{
			var startDate = new DateOnly(start);
			var people = _personRepository.LoadAllPeopleWithHierarchyDataSortByName(startDate);
			var scenario = _currentScenario.Current();
			var dateOnlyPeriod = new DateOnlyPeriod(startDate, new DateOnly(end));

			var dayCollections = dateOnlyPeriod.DayCollection().Batch(30);

			foreach (var dayCollection in dayCollections)
			{
				var days = dayCollection.ToArray();
				if (!days.Any()) continue;
				var period = new DateOnlyPeriod(days[0], days[days.Length-1]);
				var extendedDateOnlyPeriodBecauseWeDontWantToConvertToEachPersonsTimeZone = period.Inflate(1);
				foreach (var person in people)
				{
					var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
						new ScheduleDictionaryLoadOptions(false, false),
						extendedDateOnlyPeriodBecauseWeDontWantToConvertToEachPersonsTimeZone.ToDateTimePeriod(TimeZoneInfo.Utc),
						scenario);
					var scheduleDays = schedules.SchedulesForPeriod(period, person).ToLookup(s => s.DateOnlyAsPeriod.DateOnly);

					days.ForEach(day => validate(types, person, scheduleDays[day].First(), reportProgress, ignoreValid));
				}
			}
		}
		
		private void validate(ValidateReadModelType types,IPerson person, IScheduleDay scheduleDay,
			Action<ReadModelValidationResult> reportProgress, bool ignoreValid)
		{
			if(types.HasFlag(ValidateReadModelType.ScheduleProjectionReadOnly))
			{
				var isInvalid = !_readModelScheduleProjectionReadOnlyValidator.Validate(person,scheduleDay);
				if(isInvalid || !ignoreValid) reportProgress(makeResult(person,scheduleDay.DateOnlyAsPeriod.DateOnly,!isInvalid,ValidateReadModelType.ScheduleProjectionReadOnly));
			}
			if(types.HasFlag(ValidateReadModelType.PersonScheduleDay))
			{
				var isInvalid = !_readModelPersonScheduleDayValidator.Validate(person,scheduleDay);
				if(isInvalid || !ignoreValid)
				{
					reportProgress(makeResult(person,scheduleDay.DateOnlyAsPeriod.DateOnly,!isInvalid,ValidateReadModelType.PersonScheduleDay));
				}
			}

			if(types.HasFlag(ValidateReadModelType.ScheduleDay))
			{
				var isInvalid = !_readModelScheduleDayValidator.Validate(person,scheduleDay);
				if(isInvalid || !ignoreValid)
				{
					reportProgress(makeResult(person,scheduleDay.DateOnlyAsPeriod.DateOnly,!isInvalid,ValidateReadModelType.ScheduleDay));
				}
			}
		}
		
		private ReadModelValidationResult makeResult(IPerson person,DateOnly date,bool isValid,
			ValidateReadModelType type)
		{
			return new ReadModelValidationResult
			{
				PersonId = person.Id.GetValueOrDefault(),
				Date = date.Date,
				IsValid = isValid,
				Type = type
			};
		}
	}
}