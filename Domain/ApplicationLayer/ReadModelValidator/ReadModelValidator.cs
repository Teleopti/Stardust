using System;
using System.Collections.Generic;
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

		private IList<ValidateReadModelType> _targetTypes = new List<ValidateReadModelType>();

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


		public void Validate(DateTime start, DateTime end, Action<ReadModelValidationResult> reportProgress,
			bool ignoreValid = false)
		{

			var people = _personRepository.LoadAllPeopleWithHierarchyDataSortByName(new DateOnly(start));
			var scenario = _currentScenario.Current();
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(start), new DateOnly(end));

			people.ForEach(person =>
			{
				var extendedDateOnlyPeriod = new DateOnlyPeriod(dateOnlyPeriod.StartDate.AddDays(-1), dateOnlyPeriod.EndDate.AddDays(1));

				var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
					new ScheduleDictionaryLoadOptions(false, false),
					extendedDateOnlyPeriod.ToDateTimePeriod(TimeZoneInfo.Utc),
					scenario);
				dateOnlyPeriod.DayCollection().ForEach(day =>
				{
					var scheduleDay = schedules.SchedulesForDay(day).SingleOrDefault();
					if (_targetTypes.Contains(ValidateReadModelType.ScheduleProjectionReadOnly))
					{
						var isInvalid = !_readModelScheduleProjectionReadOnlyValidator.Validate(person, day, scheduleDay);
						if (isInvalid || !ignoreValid) reportProgress(makeResult(person,day,!isInvalid,ValidateReadModelType.ScheduleProjectionReadOnly));
					}
					if (_targetTypes.Contains(ValidateReadModelType.PersonScheduleDay))
					{
						var isInvalid = !_readModelPersonScheduleDayValidator.Validate(person, day, scheduleDay);
						if (isInvalid || !ignoreValid)
						{
							reportProgress(makeResult(person, day, !isInvalid, ValidateReadModelType.PersonScheduleDay));
						}
					}

					if (_targetTypes.Contains(ValidateReadModelType.ScheduleDay))
					{
						var isInvalid = !_readModelScheduleDayValidator.Validate(person, day, scheduleDay);
						if (isInvalid || !ignoreValid)
						{
							reportProgress(makeResult(person, day, !isInvalid, ValidateReadModelType.ScheduleDay));
						}
					}
				});
			});
		}

		public void SetTargetTypes(IList<ValidateReadModelType> types)
		{
			_targetTypes = types;
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