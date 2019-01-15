using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsFactScheduleMapper
	{
		private readonly AnalyticsFactScheduleDateMapper _dateMapper;
		private readonly AnalyticsFactScheduleTimeMapper _timeMapper;

		public AnalyticsFactScheduleMapper(
			AnalyticsFactScheduleDateMapper dateMapper,
			AnalyticsFactScheduleTimeMapper timeMapper)
		{
			_dateMapper = dateMapper;
			_timeMapper = timeMapper;
		}

		public List<IFactScheduleRow> AgentDaySchedule(ProjectionChangedEventScheduleDay eventScheduleDay, IScheduleDay scheduleDay, IAnalyticsFactSchedulePerson personPart, int intervalLength, DateTime scheduleChangeTime, int shiftCategoryId, int scenarioId, IDictionary<Guid, AnalyticsActivity> activities, Guid personCode)
		{
			if (eventScheduleDay.Shift == null)
				return new List<IFactScheduleRow>();
			
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
						TimePart = _timeMapper.Handle(layer, activities, shiftCategoryId, scenarioId, shiftLengthId, plannedOvertime)
					};
					scheduleRows.Add(factScheduleRow);
				}
               intervalStart = intervalStart.AddMinutes(minutesToAdd);
            }
            return scheduleRows;
		}
	}
}