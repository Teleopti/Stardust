using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours
{
	public class SiteOpenHoursSpecification : Specification<SiteOpenHoursCheckItem>, ISiteOpenHoursSpecification
	{
		public override bool IsSatisfiedBy(SiteOpenHoursCheckItem siteOpenHoursCheckItem)
		{
			var person = siteOpenHoursCheckItem.Person;
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var scheduleTimePeriods = getSchedulePeriods(siteOpenHoursCheckItem.Period, timeZone);
			return isSatisfiedSiteOpenHours(scheduleTimePeriods, person);
		}

		private static bool isSatisfiedSiteOpenHours(Dictionary<DateOnly, TimePeriod> personScheduleTimePeriods, IPerson person)
		{
			var isSatisfied = true;
			foreach (var personToScheduleTimePeriod in personScheduleTimePeriods)
			{
				var personSiteOpenHourPeriods = getPersonSiteOpenHourPeriod(person, personToScheduleTimePeriod.Key);
				if (personSiteOpenHourPeriods != null)
				{
					isSatisfied = isSatisfied && personSiteOpenHourPeriods.Any(p => p.Contains(personToScheduleTimePeriod.Value));
				}
			}
			return isSatisfied;
		}

		private Dictionary<DateOnly, TimePeriod> getSchedulePeriods(DateTimePeriod period, TimeZoneInfo timeZone)
		{
			var startTime = period.StartDateTimeLocal(timeZone);
			var endTime = period.EndDateTimeLocal(timeZone);
			var dateTimePeriodDictionary = new Dictionary<DateOnly, TimePeriod>();
			if (startTime.Day == endTime.Day)
			{
				dateTimePeriodDictionary.Add(new DateOnly(startTime),
					new TimePeriod(startTime.TimeOfDay, endTime.TimeOfDay));
			}
			else if (startTime.Day < endTime.Day && (endTime.Hour == 0 && endTime.Minute == 0))
			{
				dateTimePeriodDictionary.Add(new DateOnly(startTime),
					new TimePeriod(startTime.TimeOfDay, TimeSpan.FromDays(1).Subtract(TimeSpan.FromMinutes(1))));
			}
			else
			{
				dateTimePeriodDictionary.Add(new DateOnly(startTime),
					new TimePeriod(startTime.TimeOfDay, TimeSpan.FromDays(1).Subtract(TimeSpan.FromMinutes(1))));
				dateTimePeriodDictionary.Add(new DateOnly(endTime),
					new TimePeriod(TimeSpan.Zero, endTime.TimeOfDay));
			}

			return dateTimePeriodDictionary;
		}

		private static TimePeriod[] getPersonSiteOpenHourPeriod(IPerson person, DateOnly scheduleDate)
		{
			var siteOpenHour = person.SiteOpenHour(scheduleDate);
			if (siteOpenHour == null)
			{
				return null;
			}
			if (siteOpenHour.IsClosed)
			{
				return new[] {new TimePeriod()};
			}

			var timePeriods = new List<TimePeriod>();
			var timePeriod = siteOpenHour.TimePeriod;
			if (timePeriod.EndTime.Days > 0)
			{
				timePeriods.Add(new TimePeriod(timePeriod.StartTime, TimeSpan.FromDays(1).Subtract(TimeSpan.FromMinutes(1))));
				timePeriods.Add(new TimePeriod(TimeSpan.Zero, timePeriod.EndTime.Subtract(TimeSpan.FromDays(1))));
			}
			else
			{
				timePeriods.Add(timePeriod);
			}

			return timePeriods.ToArray();
		}
	}
}
