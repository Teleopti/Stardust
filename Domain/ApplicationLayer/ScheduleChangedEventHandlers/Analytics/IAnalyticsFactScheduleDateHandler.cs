using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactScheduleDateHandler
	{
		bool MapDateId(DateOnly date, out int dateId);

		AnalyticsFactScheduleDate Handle(
			DateTime shiftStartDateUtc, 
			DateTime shiftEndDateUtc, 
			DateOnly shiftStartDateLocal,
			ProjectionChangedEventLayer layer, 
			DateTime scheduleChangeTime, 
			int minutesPerInterval);
	}
}