using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{	
	public class ReadModelValidator : IReadModelValidator
	{
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProjectionChangedEventBuilder _builder;
		private readonly IPersonScheduleDayReadModelsCreator _personScheduleDayReadModelsCreator;
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelFinder;
		private readonly IScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private IList<ValidateReadModelType> _targetTypes = new List<ValidateReadModelType>();

		public ReadModelValidator(IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister, IPersonRepository personRepository,
			IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IProjectionChangedEventBuilder builder,
			IPersonScheduleDayReadModelsCreator personScheduleDayReadModelsCreator,
			IPersonScheduleDayReadModelFinder personScheduleDayReadModelFinder,
			IScheduleDayReadModelsCreator scheduleDayReadModelsCreator,
			IScheduleDayReadModelRepository scheduleDayReadModelRepository)
		{
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_builder = builder;
			_personScheduleDayReadModelsCreator = personScheduleDayReadModelsCreator;
			_personScheduleDayReadModelFinder = personScheduleDayReadModelFinder;
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
		}

		
		public void Validate(DateTime start, DateTime end, Action<ReadModelValidationResult> reportProgress,
			bool ignoreValid = false)
		{

			var people = _personRepository.LoadAllPeopleWithHierarchyDataSortByName(new DateOnly(start));
			var scenario = _currentScenario.Current();
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(start), new DateOnly(end));

			people.ForEach(person =>
			{
				var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
					new ScheduleDictionaryLoadOptions(false, false),
					dateOnlyPeriod.ToDateTimePeriod(TimeZoneInfo.Utc),
					scenario);
				dateOnlyPeriod.DayCollection().ForEach(day =>
				{
					var scheduleDay = schedules.SchedulesForDay(day).SingleOrDefault();
					if (_targetTypes.Contains(ValidateReadModelType.ScheduleProjectionReadOnly))
					{
						var isInvalid = !ValidateReadModelScheduleProjectionReadOnly(person, day, scheduleDay);
						if (isInvalid || !ignoreValid) reportProgress(makeResult(person,day,!isInvalid,ValidateReadModelType.ScheduleProjectionReadOnly));
					}
					if (_targetTypes.Contains(ValidateReadModelType.PersonScheduleDay))
					{
						var isInvalid = !ValidateReadModelPersonScheduleDay(person, day, scheduleDay);
						if (isInvalid || !ignoreValid)
						{
							reportProgress(makeResult(person, day, !isInvalid, ValidateReadModelType.PersonScheduleDay));
						}
					}

					if (_targetTypes.Contains(ValidateReadModelType.ScheduleDay))
					{
						var isInvalid = !ValidateReadModelScheduleDay(person, day, scheduleDay);
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
	
		#region ReadModel.PersonScheduleDay

		public bool ValidateReadModelPersonScheduleDay(IPerson person, DateOnly date, IScheduleDay scheduleDay)
		{
			if (scheduleDay == null) return true;
			var fetchedReadModel = FetchReadModelPersonScheduleDay(person, date);
			var builtReadModel = BuildReadModelPersonScheduleDay(person, scheduleDay);
			if(builtReadModel == null) return fetchedReadModel == null;
			return builtReadModel.Equals(fetchedReadModel);
		}

		public PersonScheduleDayReadModel FetchReadModelPersonScheduleDay(IPerson person, DateOnly date)
		{
			return _personScheduleDayReadModelFinder.ForPerson(date, person.Id.GetValueOrDefault());
		}

		public PersonScheduleDayReadModel BuildReadModelPersonScheduleDay(IPerson person, IScheduleDay scheduleDay)
		{
			var eventScheduleDay = _builder.BuildEventScheduleDay(scheduleDay);
			return _personScheduleDayReadModelsCreator.MakePersonScheduleDayReadModel(person, eventScheduleDay);
		}

		#endregion

		#region ReadModel.ScheduleDay

		public bool ValidateReadModelScheduleDay(IPerson person, DateOnly date, IScheduleDay scheduleDay)
		{
			if(scheduleDay == null) return true;

			var fetchedReadModel = FetchReadModelScheduleDay(person, date);
			var builtReadModel = BuildReadModelScheduleDay(person, scheduleDay);

			if (builtReadModel == null) return fetchedReadModel == null;
			return builtReadModel.Equals(fetchedReadModel);				
		}

		public ScheduleDayReadModel FetchReadModelScheduleDay(IPerson person, DateOnly date)
		{
			return _scheduleDayReadModelRepository.ReadModelsOnPerson(date, date, person.Id.Value).FirstOrDefault();
		}

		public ScheduleDayReadModel BuildReadModelScheduleDay(IPerson person, IScheduleDay scheduleDay)
		{
			var eventScheduleDay = _builder.BuildEventScheduleDay(scheduleDay);
			return _scheduleDayReadModelsCreator.GetReadModel(eventScheduleDay, person);
		}

		#endregion

		#region ScheduleProjectionReadOnly

		public bool ValidateReadModelScheduleProjectionReadOnly(IPerson person, DateOnly date, IScheduleDay scheduleDay)
		{
			if(scheduleDay == null) return true;

			var fetchedReadModels = FetchReadModelScheduleProjectionReadOnly(person, date);							
			var mappedReadModels = BuildReadModelScheduleProjectionReadOnly(person,scheduleDay);

			var isValid = mappedReadModels.Count() != fetchedReadModels.Count()
							|| mappedReadModels.Zip(fetchedReadModels, (a, b) =>
							{
								if (a == null) return b == null;
								return a.Equals(b);								
							}).All(x => x);
			return isValid;
		}

		public IEnumerable<ScheduleProjectionReadOnlyModel> FetchReadModelScheduleProjectionReadOnly(IPerson person,
			DateOnly date)
		{
			return _scheduleProjectionReadOnlyPersister.ForPerson(date, person.Id.GetValueOrDefault(), _currentScenario.Current().Id.GetValueOrDefault())
				.ToList()
				.OrderBy(l => l.StartDateTime);
		}

		public IEnumerable<ScheduleProjectionReadOnlyModel> BuildReadModelScheduleProjectionReadOnly(Guid personId, DateOnly date)
		{
			return BuildReadModelScheduleProjectionReadOnly(_personRepository.Get(personId), date);
		}

		public IEnumerable<ScheduleProjectionReadOnlyModel> BuildReadModelScheduleProjectionReadOnly(IPerson person, DateOnly date)
		{
			var scenario = _currentScenario.Current();
			var schedule = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false),
				date.ToDateTimePeriod(TimeZoneInfo.Utc),
				scenario);
			var scheduleDay = schedule.SchedulesForDay(date).SingleOrDefault();
			return BuildReadModelScheduleProjectionReadOnly(person, scheduleDay);
		}

		public IEnumerable<ScheduleProjectionReadOnlyModel> BuildReadModelScheduleProjectionReadOnly(IPerson person, IScheduleDay scheduleDay)
		{
			var scenario = _currentScenario.Current();
			if (scheduleDay != null)
			{
				var projection = scheduleDay.ProjectionService().CreateProjection();
				var layers = _builder.BuildProjectionChangedEventLayers(projection);

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

		#endregion

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