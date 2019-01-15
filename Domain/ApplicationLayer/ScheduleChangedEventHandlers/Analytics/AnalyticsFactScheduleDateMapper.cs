using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsFactScheduleDateMapper
	{
		private readonly AnalyticsDateMapper _analyticsDateMapper;

		public AnalyticsFactScheduleDateMapper(AnalyticsDateMapper analyticsDateMapper)
		{
			_analyticsDateMapper = analyticsDateMapper;
		}

		public IAnalyticsFactScheduleDate Map(
			DateTime shiftStartDateUtc, 
			DateTime shiftEndDateUtc, 
			DateOnly shiftStartDateLocal, 
			ProjectionChangedEventLayer layer, 
			DateTime scheduleChangeTime,
			int minutesPerInterval)
		{
			if (!_analyticsDateMapper.MapDateId(shiftStartDateLocal, out var scheduleStartDateLocalId)) return null;
			if (!_analyticsDateMapper.MapDateId(new DateOnly(layer.StartDateTime), out var activityStartDateId)) return null;
			if (!_analyticsDateMapper.MapDateId(new DateOnly(layer.EndDateTime), out var activityEndDateId)) return null;
			if (!_analyticsDateMapper.MapDateId(new DateOnly(shiftStartDateUtc), out var shiftStartDateId)) return null;
			if (!_analyticsDateMapper.MapDateId(new DateOnly(shiftEndDateUtc), out var shiftEndDateId)) return null;

			return new AnalyticsFactScheduleDate
			{
				ScheduleDateId = activityStartDateId, 
				ScheduleStartDateLocalId = scheduleStartDateLocalId,
				ActivityStartTime = layer.StartDateTime,
				ActivityStartDateId = activityStartDateId,
				ActivityEndTime = layer.EndDateTime,
				ActivityEndDateId = activityEndDateId,
				ShiftStartTime = shiftStartDateUtc,
				ShiftStartDateId = shiftStartDateId,
				ShiftEndTime = shiftEndDateUtc,
				ShiftEndDateId = shiftEndDateId,
				IntervalId = getIdFromDateTime(layer.StartDateTime, minutesPerInterval),
				ShiftStartIntervalId = getIdFromDateTime(shiftStartDateUtc, minutesPerInterval),
				ShiftEndIntervalId = getIdFromDateTime(shiftEndDateUtc.AddSeconds(-1), minutesPerInterval),
				DatasourceUpdateDate = scheduleChangeTime
			};
		}

		private int getIdFromDateTime(DateTime date, int minutesPerInterval)
		{
			double minutesElapsedOfDay = date.TimeOfDay.TotalMinutes;
			int id = (int)minutesElapsedOfDay / minutesPerInterval;

			return id;
		}
	}
}