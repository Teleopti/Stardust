using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
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
		private readonly IScheduleProjectionReadOnlyPersister _persister;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProjectionChangedEventBuilder _builder;

		public ReadModelValidator(IScheduleProjectionReadOnlyPersister persister, IPersonRepository personRepository, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IProjectionChangedEventBuilder builder)
		{
			_persister = persister;
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_builder = builder;
		}

		public void Validate(DateTime start, DateTime end, Action<ScheduleProjectionReadOnlyValidationResult> reportProgress, bool ignoreValid = false)
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
					var readModelLayers =
						_persister.ForPerson(day, person.Id.GetValueOrDefault(), scenario.Id.GetValueOrDefault())
							.ToList()
							.OrderBy(l => l.StartDateTime);

					var mappedLayers = BuildReadModel(person, schedules.SchedulesForDay(day).SingleOrDefault());

					var isInValid = mappedLayers.Count() != readModelLayers.Count()
									|| mappedLayers.Zip(readModelLayers, IsReadModelDifferent).Any(x => x);

					if (isInValid || !ignoreValid)
						reportProgress(new ScheduleProjectionReadOnlyValidationResult
						{
							PersonId = person.Id.GetValueOrDefault(),
							Date = day.Date,
							IsValid = !isInValid
						});
				});
			});

		}


		public static bool IsReadModelDifferent(ScheduleProjectionReadOnlyModel a, ScheduleProjectionReadOnlyModel b)
		{
			return  !a.Equals(b);
		}

		public IEnumerable<ScheduleProjectionReadOnlyModel> BuildReadModel(Guid personId, DateOnly date)
		{
			return BuildReadModel(_personRepository.Get(personId), date);
		}

		public IEnumerable<ScheduleProjectionReadOnlyModel> BuildReadModel(IPerson person, DateOnly date)
		{
			var scenario = _currentScenario.Current();
			var schedule = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false),
				date.ToDateTimePeriod(TimeZoneInfo.Utc),
				scenario);
			var scheduleDay = schedule.SchedulesForDay(date).SingleOrDefault();
			if (scheduleDay != null)
			{
				var projection = scheduleDay.ProjectionService().CreateProjection();
				var layers = _builder.BuildProjectionChangedEventLayers(projection);

				return layers.Select(layer => new ScheduleProjectionReadOnlyModel
				{
					PersonId = person.Id.Value,
					ScenarioId = scenario.Id.Value,
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
		public IEnumerable<ScheduleProjectionReadOnlyModel> BuildReadModel(IPerson person, IScheduleDay scheduleDay)
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

	}
}