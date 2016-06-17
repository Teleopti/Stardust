using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

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

		public IEnumerable<ScheduleProjectionReadOnlyValidationResult> Validate(DateTime start, DateTime end)
		{

			var people = _personRepository.LoadAllPeopleWithHierarchyDataSortByName(new DateOnly(start));
			var scenario = _currentScenario.Current();
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(start), new DateOnly(end));
			var ret = new List<ScheduleProjectionReadOnlyValidationResult>();

			people.ForEach(person =>
			{
				dateOnlyPeriod.DayCollection().ForEach(day =>
				{
					var readModelLayers =
						_persister.ForPerson(day, person.Id.GetValueOrDefault(), scenario.Id.GetValueOrDefault())
							.ToList()
							.OrderBy(l => l.StartDateTime);

					var mappedLayers = BuildReadModel(person, day);

					if (mappedLayers.Count() != readModelLayers.Count() 
					|| mappedLayers.Zip(readModelLayers, IsReadModelDifferent).Any(x => x))
					{
						ret.Add(new ScheduleProjectionReadOnlyValidationResult
						{
							PersonId = person.Id.GetValueOrDefault(),
							Date = day.Date,
							IsValid = false
						});
					}				
				
				});
			});
			return ret;
		}


		public static bool IsReadModelDifferent(ScheduleProjectionReadOnlyModel a, ScheduleProjectionReadOnlyModel b)
		{
			return  !a.Equals(b);
		}

		public IEnumerable<ScheduleProjectionReadOnlyModel> BuildReadModel(IPerson person, DateOnly date)
		{
			var scenario = _currentScenario.Current();
			var schedule = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false),
				date.ToDateTimePeriod(TimeZoneInfo.Utc),
				scenario);
			var scheduleDay = schedule.SchedulesForDay(date).Single();
			var projection = scheduleDay.ProjectionService().CreateProjection();
			var layers = _builder.BuildProjectionChangedEventLayers(projection);

			return layers.Select(layer => new ScheduleProjectionReadOnlyModel
			{
				PersonId = person.Id.Value,
				ScenarioId = scenario.Id.Value,
				BelongsToDate = date,
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

	}
}