using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

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
				var personSiteOpenHourPeriod = getPersonSiteOpenHourPeriod(person, personToScheduleTimePeriod.Key);
				isSatisfied = isSatisfied && personSiteOpenHourPeriod.Contains(personToScheduleTimePeriod.Value);
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
			else
			{
				dateTimePeriodDictionary.Add(new DateOnly(startTime),
					new TimePeriod(startTime.TimeOfDay, endTime.TimeOfDay.Add(TimeSpan.FromDays(1))));
			}

			return dateTimePeriodDictionary;
		}

		private static TimePeriod getPersonSiteOpenHourPeriod(IPerson person, DateOnly scheduleDate)
		{
			var siteOpenHour = person.SiteOpenHour(scheduleDate);
			if (siteOpenHour == null)
			{
				return new TimePeriod(TimeSpan.Zero, TimeSpan.FromHours(24).Subtract(new TimeSpan(1)));
			}
			if (siteOpenHour.IsClosed)
			{
				return new TimePeriod();
			}
			return siteOpenHour.TimePeriod;
		}
	}
}
