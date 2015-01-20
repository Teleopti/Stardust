using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public interface IAnalyticsFactScheduleDateHandler
	{
		bool MapDateId(DateOnly date, out int dateId);

		IAnalyticsFactScheduleDate Handle(
			DateTime shiftStartDateUtc, 
			DateTime shiftEndDateUtc, 
			DateOnly shiftStartDateLocal,
			ProjectionChangedEventLayer layer, 
			DateTime scheduleChangeTime, 
			int minutesPerInterval);
	}
}