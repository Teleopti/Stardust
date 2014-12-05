using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsFactScheduleDateHandler : IAnalyticsFactScheduleDateHandler
	{
		private readonly IAnalyticsScheduleRepository _analyticsScheduleRepository;
		private readonly INow _now;
		private readonly int _minutesPerInterval;

		public AnalyticsFactScheduleDateHandler(
			IAnalyticsScheduleRepository analyticsScheduleRepository, 
			INow now, 
			int minutesPerInterval)

		{
			_analyticsScheduleRepository = analyticsScheduleRepository;
			_now = now;
			_minutesPerInterval = minutesPerInterval;
		}

		public AnalyticsFactScheduleDate Handle(
			DateTime shiftStartDateUtc, 
			DateTime shiftEndDateUtc, 
			DateOnly shiftStartDateLocal, 
			ProjectionChangedEventLayer layer, 
			DateTime scheduleChangeTime)

		{
			var dimDateList = _analyticsScheduleRepository.LoadDimDates(_now.UtcDateTime());

			var scheduleStartDateLocal = dimDateList.SingleOrDefault(x => x.Key == shiftStartDateLocal);
			var activityStartDate = dimDateList.SingleOrDefault(x => x.Key == new DateOnly(layer.StartDateTime));
			var activityEndDate = dimDateList.SingleOrDefault(x => x.Key == new DateOnly(layer.EndDateTime));
			var shiftStartDate = dimDateList.SingleOrDefault(x => x.Key == new DateOnly(shiftStartDateUtc));
			var shiftEndDate = dimDateList.SingleOrDefault(x => x.Key == new DateOnly(shiftEndDateUtc));

			var noDateIdFound = new KeyValuePair<DateOnly, int>();
			if (scheduleStartDateLocal.Key == noDateIdFound.Key
			    || activityStartDate.Key == noDateIdFound.Key
			    || activityEndDate.Key == noDateIdFound.Key
					|| shiftStartDate.Key == noDateIdFound.Key
					|| shiftEndDate.Key == noDateIdFound.Key)
			{
				return null;
			}

			return new AnalyticsFactScheduleDate
			{
				ScheduleDateId = activityStartDate.Value, 
				ScheduleStartDateLocalId = scheduleStartDateLocal.Value,
				ActivityStartTime = layer.StartDateTime,
				ActivityStartDateId = activityStartDate.Value,
				ActivityEndTime = layer.EndDateTime,
				ActivityEndDateId = activityEndDate.Value,
				ShiftStartTime = shiftStartDateUtc,
				ShiftStartDateId = shiftStartDate.Value,
				ShiftEndTime = shiftEndDateUtc,
				ShiftEndDateId = shiftEndDate.Value,
				IntervalId = getIdFromDateTime(layer.StartDateTime),
				ShiftStartIntervalId = getIdFromDateTime(shiftStartDateUtc),
				ShiftEndIntervalId = getIdFromDateTime(shiftEndDateUtc.AddSeconds(-1)),
				DatasourceUpdateDate = scheduleChangeTime
			};
		}

		private int getIdFromDateTime(DateTime date)
		{
			double minutesElapsedOfDay = date.TimeOfDay.TotalMinutes;
			int id = (int)minutesElapsedOfDay / _minutesPerInterval;

			return id;
		}
	}
}