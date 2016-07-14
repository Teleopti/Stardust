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
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IReadModelPersonScheduleDayValidator _readModelPersonScheduleDayValidator;
		private readonly IReadModelScheduleProjectionReadOnlyValidator _readModelScheduleProjectionReadOnlyValidator;
		private readonly IReadModelScheduleDayValidator _readModelScheduleDayValidator;
		private readonly IReadModelValidationResultPersister _readModelValidationResultPersister;

		public ReadModelValidator(IPersonRepository personRepository, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IReadModelPersonScheduleDayValidator readModelPersonScheduleDayValidator, IReadModelScheduleProjectionReadOnlyValidator readModelScheduleProjectionReadOnlyValidator, IReadModelScheduleDayValidator readModelScheduleDayValidator, IReadModelValidationResultPersister readModelValidationResultPersister)
		{
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_readModelPersonScheduleDayValidator = readModelPersonScheduleDayValidator;
			_readModelScheduleProjectionReadOnlyValidator = readModelScheduleProjectionReadOnlyValidator;
			_readModelScheduleDayValidator = readModelScheduleDayValidator;
			_readModelValidationResultPersister = readModelValidationResultPersister;
		}

		public void ClearResult(ValidateReadModelType types)
		{
			using(var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{				
				_readModelValidationResultPersister.Reset(types);
				uow.PersistAll();
			}
		}

		public void Validate(ValidateReadModelType types, DateTime start, DateTime end, bool directFix = false)
		{
			var startDate = new DateOnly(start);
			IList<Guid> personIds;
			IScenario scenario;

			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				personIds = _personRepository.LoadAll().Select(x => x.Id.Value).ToList();
				scenario = _currentScenario.Current();
			}

			var dateOnlyPeriod = new DateOnlyPeriod(startDate,new DateOnly(end));
			var extendedDateOnlyPeriodBecauseWeDontWantToConvertToEachPersonsTimeZone = dateOnlyPeriod.Inflate(1);

			foreach (var personIdCollection in personIds.Batch(10))
			{
				using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					var people = _personRepository.FindPeople(personIdCollection);
					foreach (var person in people)
					{
						var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
							new ScheduleDictionaryLoadOptions(false,false),
							extendedDateOnlyPeriodBecauseWeDontWantToConvertToEachPersonsTimeZone.ToDateTimePeriod(TimeZoneInfo.Utc),
							scenario);
						var scheduleDays = schedules.SchedulesForPeriod(dateOnlyPeriod,person).ToLookup(s => s.DateOnlyAsPeriod.DateOnly);

						dateOnlyPeriod.DayCollection().ForEach(day => validate(types,person,scheduleDays[day].First(), directFix));
					}
					uow.PersistAll();
				}
			}			
		}
		
		private void validate(ValidateReadModelType types,IPerson person, IScheduleDay scheduleDay, bool directFix)
		{
			if(types.HasFlag(ValidateReadModelType.ScheduleProjectionReadOnly))
			{
				var isValid = _readModelScheduleProjectionReadOnlyValidator.Validate(person,scheduleDay,directFix);
				if(!isValid) _readModelValidationResultPersister.SaveScheduleProjectionReadOnly(
					makeResult(person,scheduleDay.DateOnlyAsPeriod.DateOnly,false,ValidateReadModelType.ScheduleProjectionReadOnly));
			}

			if(types.HasFlag(ValidateReadModelType.PersonScheduleDay))
			{
				var isValid = _readModelPersonScheduleDayValidator.Validate(person,scheduleDay,directFix);
				if(!isValid)
				{
					_readModelValidationResultPersister.SavePersonScheduleDay(
						makeResult(person,scheduleDay.DateOnlyAsPeriod.DateOnly,false,ValidateReadModelType.PersonScheduleDay));
				}
			}

			if(types.HasFlag(ValidateReadModelType.ScheduleDay))
			{
				var isValid = _readModelScheduleDayValidator.Validate(person,scheduleDay,directFix);
				if(!isValid)
				{
					_readModelValidationResultPersister.SaveScheduleDay(
						makeResult(person,scheduleDay.DateOnlyAsPeriod.DateOnly,false,ValidateReadModelType.ScheduleDay));
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