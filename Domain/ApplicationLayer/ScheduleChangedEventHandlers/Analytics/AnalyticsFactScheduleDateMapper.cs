using System;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactScheduleDateMapper
	{
		bool MapDateId(DateOnly date, out int dateId);

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
		private readonly IAnalyticsDateRepository _analyticsDateRepository;

		public AnalyticsFactScheduleDateMapper(IAnalyticsDateRepository analyticsDateRepository)
		{
			_analyticsDateRepository = analyticsDateRepository;
		}

		public bool MapDateId(DateOnly date, out int dateId)
		{
			var noDateIdFound = new AnalyticsDate();
			var datePair = _analyticsDateRepository.Date(date.Date) ?? noDateIdFound;
			if (datePair.DateDate == noDateIdFound.DateDate)
			{
				dateId = -1;
				return false;
			}

			dateId = datePair.DateId;
			return true;
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

			if (!MapDateId(shiftStartDateLocal, out scheduleStartDateLocalId)) return null;
			if (!MapDateId(new DateOnly(layer.StartDateTime), out activityStartDateId)) return null;
			if (!MapDateId(new DateOnly(layer.EndDateTime), out activityEndDateId)) return null;
			if (!MapDateId(new DateOnly(shiftStartDateUtc), out shiftStartDateId)) return null;
			if (!MapDateId(new DateOnly(shiftEndDateUtc), out shiftEndDateId)) return null;

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