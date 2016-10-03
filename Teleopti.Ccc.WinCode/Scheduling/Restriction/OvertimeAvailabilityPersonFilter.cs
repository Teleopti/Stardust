using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
	public class OvertimeAvailabilityPersonFilter
	{
		public IList<IPerson> GetFilteredPerson(IList<IScheduleDay> scheduleDaysList, DateOnly date, TimeSpan filterStartTime, TimeSpan filterEndTime, TimeZoneInfo myTimeZone, bool allowIntersect)
		{
			var personList = new List<IPerson>();
			var filterStartDateTimeLocal = new DateTime(date.Year, date.Month, date.Day).Add(filterStartTime);
			var filterEndDateTimeLocal = new DateTime(date.Year, date.Month, date.Day).Add(filterEndTime);
			var filterUtcPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(filterStartDateTimeLocal, filterEndDateTimeLocal, myTimeZone);

			foreach (var scheduleDay in scheduleDaysList)
			{
				var overtimeAvailability = scheduleDay.PersistableScheduleDataCollection().OfType<IOvertimeAvailability>().FirstOrDefault();
				if (overtimeAvailability != null)
				{
					var startTime = overtimeAvailability.StartTime.GetValueOrDefault();
					var endTime = overtimeAvailability.EndTime.GetValueOrDefault();
					var availStartDateTimeLocal = new DateTime(date.Year, date.Month, date.Day).Add(startTime);
					var availEndDateTimeLocal = new DateTime(date.Year, date.Month, date.Day).Add(endTime);
					if (startTime > endTime)
					{
						availEndDateTimeLocal = availEndDateTimeLocal.AddDays(1);
					}
					var availUtcPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(availStartDateTimeLocal, availEndDateTimeLocal, scheduleDay.Person.PermissionInformation.DefaultTimeZone());

					if (allowIntersect)
					{
						if (availUtcPeriod.Intersect(filterUtcPeriod))
						{
							personList.Add(overtimeAvailability.Person);
						}
					}
					else
					{
						if (availUtcPeriod.StartDateTime <= filterUtcPeriod.StartDateTime && availUtcPeriod.EndDateTime >= filterUtcPeriod.EndDateTime)
						{
							personList.Add(overtimeAvailability.Person);
						}
					}
				}
			}
			return personList;
		}
	}
}