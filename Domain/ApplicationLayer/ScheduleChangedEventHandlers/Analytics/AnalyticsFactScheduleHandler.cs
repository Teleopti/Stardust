using System;
using System.Collections.Generic;
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

		public AnalyticsFactScheduleHandler(
			IIntervalLengthFetcher intervalLengthFetcher,
			IAnalyticsFactScheduleDateHandler dateHandler,
			IAnalyticsFactScheduleTimeHandler timeHandler)
		{
			_intervalLengthFetcher = intervalLengthFetcher;
			_dateHandler = dateHandler;
			_timeHandler = timeHandler;
		}

		public List<IFactScheduleRow> AgentDaySchedule(
			ProjectionChangedEventScheduleDay scheduleDay,
			IAnalyticsFactSchedulePerson personPart,
			DateTime scheduleChangeTime,
			int shiftCategoryId,
			int scenarioId)
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

			while (intervalStart < shiftEnd)
			{
				var intervalLayers =
					scheduleDay.Shift.FilterLayers(new DateTimePeriod(intervalStart, intervalStart.AddMinutes(intervalLength)));
				foreach (var layer in intervalLayers)
				{
					var datePart = _dateHandler.Handle(shiftStart, shiftEnd, localStartDate, layer, scheduleChangeTime, intervalLength);
					if (datePart == null)
						return null;

					var factScheduleRow = new FactScheduleRow
					{
						PersonPart = personPart,
						DatePart = datePart,
						TimePart = _timeHandler.Handle(layer, shiftCategoryId, scenarioId, shiftLength)
					};
					scheduleRows.Add(factScheduleRow);
				}
				intervalStart = intervalStart.AddMinutes(intervalLength);
			}
			return scheduleRows;
		}
	}
}