using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Restriction
{
	public class OvertimeAvailabilityPersonFilter
	{
		private readonly ITimeZoneGuard _timeZoneGuard;

		public OvertimeAvailabilityPersonFilter(ITimeZoneGuard timeZoneGuard)
		{
			_timeZoneGuard = timeZoneGuard;
		}

		public IEnumerable<IPerson> GetFilteredPerson(IEnumerable<IScheduleDay> scheduleDaysList, TimePeriod filterPeriod, bool allowIntersect)
		{
			foreach (var scheduleDay in scheduleDaysList)
			{
				var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
				var filterStartDateTimeLocal = new DateTime(date.Year, date.Month, date.Day).Add(filterPeriod.StartTime);
				var filterEndDateTimeLocal = new DateTime(date.Year, date.Month, date.Day).Add(filterPeriod.EndTime);
				var filterUtcPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(filterStartDateTimeLocal, filterEndDateTimeLocal, _timeZoneGuard.CurrentTimeZone());

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
							yield return overtimeAvailability.Person;
						}
					}
					else
					{
						if (availUtcPeriod.StartDateTime <= filterUtcPeriod.StartDateTime && availUtcPeriod.EndDateTime >= filterUtcPeriod.EndDateTime)
						{
							yield return overtimeAvailability.Person;
						}
					}
				}
			}
		}
	}
}