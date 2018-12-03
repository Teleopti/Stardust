using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactScheduleDateMapper
	{
		IAnalyticsFactScheduleDate Map(
			DateTime shiftStartDateUtc,
			DateTime shiftEndDateUtc,
			DateOnly shiftStartDateLocal,
			ProjectionChangedEventLayer layer,
			DateTime scheduleChangeTime,
			int minutesPerInterval);
	}

	public class AnalyticsFactScheduleDateMapper : IAnalyticsFactScheduleDateMapper
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
			int scheduleStartDateLocalId;
			int activityStartDateId;
			int activityEndDateId;
			int shiftStartDateId;
			int shiftEndDateId;

			if (!_analyticsDateMapper.MapDateId(shiftStartDateLocal, out scheduleStartDateLocalId)) return null;
			if (!_analyticsDateMapper.MapDateId(new DateOnly(layer.StartDateTime), out activityStartDateId)) return null;
			if (!_analyticsDateMapper.MapDateId(new DateOnly(layer.EndDateTime), out activityEndDateId)) return null;
			if (!_analyticsDateMapper.MapDateId(new DateOnly(shiftStartDateUtc), out shiftStartDateId)) return null;
			if (!_analyticsDateMapper.MapDateId(new DateOnly(shiftEndDateUtc), out shiftEndDateId)) return null;

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