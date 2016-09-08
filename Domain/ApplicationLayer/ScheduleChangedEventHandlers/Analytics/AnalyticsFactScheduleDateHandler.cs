using System;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsFactScheduleDateHandler : IAnalyticsFactScheduleDateHandler
	{
		private readonly IAnalyticsDateRepository _analyticsDateRepository;

		public AnalyticsFactScheduleDateHandler(IAnalyticsDateRepository analyticsDateRepository)
		{
			_analyticsDateRepository = analyticsDateRepository;
		}

		public bool MapDateId(DateOnly date, out int dateId)
		{
			var datePair = _analyticsDateRepository.Date(date.Date) ?? new AnalyticsDate();
			var noDateIdFound = new AnalyticsDate();
			if (datePair.DateDate == noDateIdFound.DateDate)
			{
				dateId = -1;
				return false;
			}

			dateId = datePair.DateId;
			return true;
		}

		public IAnalyticsFactScheduleDate Handle(
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