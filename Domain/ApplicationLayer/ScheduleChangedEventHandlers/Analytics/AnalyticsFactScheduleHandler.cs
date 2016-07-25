using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsFactScheduleHandler : IAnalyticsFactScheduleHandler
	{
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IAnalyticsFactScheduleDateHandler _dateHandler;
		private readonly IAnalyticsFactScheduleTimeHandler _timeHandler;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;

		public AnalyticsFactScheduleHandler(
			IIntervalLengthFetcher intervalLengthFetcher,
			IAnalyticsFactScheduleDateHandler dateHandler,
			IAnalyticsFactScheduleTimeHandler timeHandler,
			IScheduleStorage scheduleStorage,
			IScenarioRepository scenarioRepository,
			IPersonRepository personRepository)
		{
			_intervalLengthFetcher = intervalLengthFetcher;
			_dateHandler = dateHandler;
			_timeHandler = timeHandler;
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
		}

		public List<IFactScheduleRow> AgentDaySchedule(ProjectionChangedEventScheduleDay scheduleDay, IAnalyticsFactSchedulePerson personPart, DateTime scheduleChangeTime, int shiftCategoryId, int scenarioId, Guid scenarioCode, Guid personCode)
		{
			if (scheduleDay.Shift == null)
				return new List<IFactScheduleRow>();

			var intervalLength = _intervalLengthFetcher.IntervalLength;
			var shiftStart = scheduleDay.Shift.StartDateTime;
			var intervalStart = shiftStart;
			var shiftEnd = scheduleDay.Shift.EndDateTime;
			var shiftLength = (int)(shiftEnd - shiftStart).TotalMinutes;
			var localStartDate = new DateOnly(scheduleDay.Date);
			var scheduleRows = new List<IFactScheduleRow>();

			var dateOnly = new DateOnly(scheduleDay.Date);

			var dictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_personRepository.Get(personCode),
					new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(dateOnly, dateOnly),
					_scenarioRepository.Get(scenarioCode)
					);
			IVisualLayerCollection projection = null;
			if (dictionary != null)
			{
				var scheduleDays = dictionary.SchedulesForDay(dateOnly);

				var firstOrDefault = scheduleDays.FirstOrDefault();
				if (firstOrDefault != null)
				{
					var projectionService = firstOrDefault.ProjectionService();
					projection = projectionService.CreateProjection();
				}
			}

			while (intervalStart < shiftEnd)
			{
                var minutesToAdd = intervalLength - (intervalStart.Minute % intervalLength);
				var periodToSearch = new DateTimePeriod(intervalStart, intervalStart.AddMinutes(minutesToAdd));
				var intervalLayers = scheduleDay.Shift.FilterLayers(periodToSearch);

				foreach (var layer in intervalLayers)
				{
					var datePart = _dateHandler.Handle(shiftStart, shiftEnd, localStartDate, layer, scheduleChangeTime, intervalLength);
					if (datePart == null)
						return null;
					var dateTimePeriod = new DateTimePeriod(layer.StartDateTime, layer.EndDateTime);
					if (projection != null)
					{
						layer.ContractTime = projection.ContractTime(dateTimePeriod);
						layer.WorkTime = projection.WorkTime(dateTimePeriod);
						layer.Overtime = projection.Overtime(dateTimePeriod);
						layer.PaidTime = projection.PaidTime(dateTimePeriod);
					}

					var factScheduleRow = new FactScheduleRow
					{
						PersonPart = personPart,
						DatePart = datePart,
						TimePart = _timeHandler.Handle(layer, shiftCategoryId, scenarioId, shiftLength)
					};
					scheduleRows.Add(factScheduleRow);
				}
               intervalStart = intervalStart.AddMinutes(minutesToAdd);
            }
            return scheduleRows;
		}
	}
}