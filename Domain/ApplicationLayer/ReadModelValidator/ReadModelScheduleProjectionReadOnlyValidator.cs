using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class ReadModelScheduleProjectionReadOnlyValidator : IReadModelScheduleProjectionReadOnlyValidator
	{
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProjectionChangedEventBuilder _builder;
		private readonly IReadModelFixer _readModelFixer;


		public ReadModelScheduleProjectionReadOnlyValidator(IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister, IPersonRepository personRepository, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IProjectionChangedEventBuilder builder, IReadModelFixer readModelFixer)
		{
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_builder = builder;
			_readModelFixer = readModelFixer;
		}

		public bool IsInitialized()
		{
			return _scheduleProjectionReadOnlyPersister.IsInitialized();
		}

		public bool Validate(IPerson person,IScheduleDay scheduleDay,ReadModelValidationMode mode)
		{			
			var mappedReadModels = Build(person,scheduleDay).ToArray();

			var isValid = false;
			if (mode != ReadModelValidationMode.Reinitialize)
			{
				var fetchedReadModels = FetchFromRepository(person, scheduleDay.DateOnlyAsPeriod.DateOnly).ToArray();
				isValid = mappedReadModels.Length == fetchedReadModels.Length
						  && mappedReadModels.Zip(fetchedReadModels, (a, b) =>
						  {
							  if (a == null) return b == null;
							  return a.Equals(b);
						  }).All(x => x);
			}
			
			if (!isValid && (mode == ReadModelValidationMode.ValidateAndFix || mode == ReadModelValidationMode.Reinitialize))
			{
				_readModelFixer.FixScheduleProjectionReadOnly(new ReadModelData
				{
					Date = scheduleDay.DateOnlyAsPeriod.DateOnly,
					PersonId = person.Id.Value,
					ScheduleProjectionReadOnly = mappedReadModels
				});
			}

			return isValid;
		}

		public IEnumerable<ScheduleProjectionReadOnlyModel> FetchFromRepository(IPerson person,
			DateOnly date)
		{
			return _scheduleProjectionReadOnlyPersister.ForPerson(date,person.Id.GetValueOrDefault(),_currentScenario.Current().Id.GetValueOrDefault())
				.ToList()
				.OrderBy(l => l.StartDateTime);
		}

		public IEnumerable<ScheduleProjectionReadOnlyModel> Build(Guid personId,DateOnly date)
		{
			return Build(_personRepository.Get(personId),date);
		}

		public IEnumerable<ScheduleProjectionReadOnlyModel> Build(IPerson person,DateOnly date)
		{
			var scenario = _currentScenario.Current();
			var period = new DateOnlyPeriod(date,date);
			var extendedDateOnlyPeriodBecauseWeDontWantToConvertToEachPersonsTimeZone = period.Inflate(1);

			var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false,false),
				extendedDateOnlyPeriodBecauseWeDontWantToConvertToEachPersonsTimeZone.ToDateTimePeriod(TimeZoneInfo.Utc),
				scenario);
			var scheduleDays = schedules.SchedulesForPeriod(period,person).ToLookup(s => s.DateOnlyAsPeriod.DateOnly);

			return Build(person,scheduleDays[date].First());
		}

		public IEnumerable<ScheduleProjectionReadOnlyModel> Build(IPerson person,IScheduleDay scheduleDay)
		{
			var scenario = _currentScenario.Current();
			if(scheduleDay != null)
			{
				var projection = scheduleDay.ProjectionService().CreateProjection();
				var layers = _builder.BuildProjectionChangedEventLayers(projection, scheduleDay.Person);

				return layers.Select(layer => new ScheduleProjectionReadOnlyModel
				{
					PersonId = person.Id.GetValueOrDefault(),
					ScenarioId = scenario.Id.GetValueOrDefault(),
					BelongsToDate = scheduleDay.DateOnlyAsPeriod.DateOnly,
					PayloadId = layer.PayloadId,
					WorkTime = layer.WorkTime,
					ContractTime = layer.ContractTime,
					StartDateTime = layer.StartDateTime,
					EndDateTime = layer.EndDateTime,
					Name = layer.Name,
					ShortName = layer.ShortName,
					DisplayColor = layer.DisplayColor
				});
			}

			return new List<ScheduleProjectionReadOnlyModel>();
		}
	}
}
