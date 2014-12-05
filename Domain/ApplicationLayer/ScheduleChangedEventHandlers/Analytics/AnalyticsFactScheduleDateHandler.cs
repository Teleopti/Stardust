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
			IAnalyticsScheduleRepository analyticsScheduleRepository,

		public AnalyticsFactScheduleDateHandler(IAnalyticsScheduleRepository analyticsScheduleRepository, INow now)
		{
			_analyticsScheduleRepository = analyticsScheduleRepository;
		}

		public AnalyticsFactScheduleDate Handle(
			DateTime shiftStartDateUtc, 
			DateTime shiftEndDateUtc, 
			DateOnly shiftStartDateLocal, 
			ProjectionChangedEventLayer layer, 
			DateTime scheduleChangeTime,
			int minutesPerInterval)

		{
			var dimDateList = _analyticsScheduleRepository.LoadDimDates();

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