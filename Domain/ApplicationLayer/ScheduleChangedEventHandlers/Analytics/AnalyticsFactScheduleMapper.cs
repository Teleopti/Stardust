using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactScheduleMapper
	{
		List<IFactScheduleRow> AgentDaySchedule(ProjectionChangedEventScheduleDay eventScheduleDay, IScheduleDay scheduleDay, IAnalyticsFactSchedulePerson personPart, DateTime scheduleChangeTime, int shiftCategoryId, int scenarioId, Guid scenarioCode, Guid personCode);
	}

	public class AnalyticsFactScheduleMapper : IAnalyticsFactScheduleMapper
	{
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IAnalyticsFactScheduleDateMapper _dateMapper;
		private readonly IAnalyticsFactScheduleTimeMapper _timeMapper;

		public AnalyticsFactScheduleMapper(
			IIntervalLengthFetcher intervalLengthFetcher,
			IAnalyticsFactScheduleDateMapper dateMapper,
			IAnalyticsFactScheduleTimeMapper timeMapper)
		{
			_intervalLengthFetcher = intervalLengthFetcher;
			_dateMapper = dateMapper;
			_timeMapper = timeMapper;
		}

		public List<IFactScheduleRow> AgentDaySchedule(ProjectionChangedEventScheduleDay eventScheduleDay, IScheduleDay scheduleDay, IAnalyticsFactSchedulePerson personPart, DateTime scheduleChangeTime, int shiftCategoryId, int scenarioId, Guid scenarioCode, Guid personCode)
		{
			if (eventScheduleDay.Shift == null)
				return new List<IFactScheduleRow>();

			var intervalLength = _intervalLengthFetcher.IntervalLength;
			var shiftStart = eventScheduleDay.Shift.StartDateTime;
			var intervalStart = shiftStart;
			var shiftEnd = eventScheduleDay.Shift.EndDateTime;
			var shiftLength = (int)(shiftEnd - shiftStart).TotalMinutes;
			var localStartDate = new DateOnly(eventScheduleDay.Date);
			var scheduleRows = new List<IFactScheduleRow>();

			var projectionService = scheduleDay.ProjectionService();
			var projection = projectionService.CreateProjection();

			var shiftLengthId = _timeMapper.MapShiftLengthId(shiftLength);

			while (intervalStart < shiftEnd)
			{
                var minutesToAdd = intervalLength - intervalStart.Minute % intervalLength;
				var periodToSearch = new DateTimePeriod(intervalStart, intervalStart.AddMinutes(minutesToAdd));
				var intervalLayers = eventScheduleDay.Shift.FilterLayers(periodToSearch);

				foreach (var layer in intervalLayers)
				{
					var datePart = _dateMapper.Map(shiftStart, shiftEnd, localStartDate, layer, scheduleChangeTime, intervalLength);
					if (datePart == null)
						return null;
					var dateTimePeriod = new DateTimePeriod(layer.StartDateTime, layer.EndDateTime);
					var plannedOvertime = TimeSpan.Zero;
					if (projection != null)
					{
						layer.ContractTime = projection.ContractTime(dateTimePeriod);
						layer.WorkTime = projection.WorkTime(dateTimePeriod);
						layer.Overtime = projection.Overtime(dateTimePeriod);
						layer.PaidTime = projection.PaidTime(dateTimePeriod);
						plannedOvertime = projection.PlannedOvertime(dateTimePeriod);
					}

					var factScheduleRow = new FactScheduleRow
					{
						PersonPart = personPart,
						DatePart = datePart,
						TimePart = _timeMapper.Handle(layer, shiftCategoryId, scenarioId, shiftLengthId, plannedOvertime)
					};
					scheduleRows.Add(factScheduleRow);
				}
               intervalStart = intervalStart.AddMinutes(minutesToAdd);
            }
            return scheduleRows;
		}
	}
}