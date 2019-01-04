using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class SiteOpenHourProvider : ISiteOpenHourProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;

		public SiteOpenHourProvider(ILoggedOnUser loggedOnUser)
		{
			_loggedOnUser = loggedOnUser;
		}

		public TimePeriod? GetSiteOpenHourPeriod(DateOnly date)
		{
			var siteOpenHour = _loggedOnUser.CurrentUser().SiteOpenHour(date);
			if (siteOpenHour == null || siteOpenHour.IsClosed)
			{
				return null;
			}
			return siteOpenHour.TimePeriod;
		}

		public TimePeriod? GetMergedSiteOpenHourPeriod(DateOnlyPeriod weekPeriod)
		{
			var timePeriods = new List<TimePeriod>();
			var person = _loggedOnUser.CurrentUser();

			foreach (var day in weekPeriod.DayCollection())
			{
				var siteOpenHourPeriod = getSiteOpenHourPeriodByDate(person, day);

				if (!siteOpenHourPeriod.HasValue) continue;

				if (day == weekPeriod.EndDate)
				{
					timePeriods.Add(new TimePeriod(siteOpenHourPeriod.Value.StartTime,
						isCrossDay(siteOpenHourPeriod.Value.EndTime)
							? getMaxEndTime()
							: siteOpenHourPeriod.Value.EndTime));
				}
				else
				{
					timePeriods.Add(siteOpenHourPeriod.Value);
				}
			}

			if (timePeriods.Count == 0)
			{
				return null;
			}

			var startTime = timePeriods.Min(a => a.StartTime);
			var endTime = timePeriods.Max(a => a.EndTime);
			if (!isCrossDay(endTime)) return new TimePeriod(startTime, endTime);

			var nextDayStartTime = endTime.Subtract(TimeSpan.FromDays(1));
			if (nextDayStartTime < startTime)
				startTime = nextDayStartTime;
			endTime = getMaxEndTime();

			return new TimePeriod(startTime, endTime);
		}

		private TimePeriod? getSiteOpenHourPeriodByDate(IPerson person, DateOnly date)
		{
			var siteOpenHour = person.SiteOpenHour(date);
			if (siteOpenHour == null || siteOpenHour.IsClosed)
			{
				return null;
			}
			return siteOpenHour.TimePeriod;
		}

		private static bool isCrossDay(TimeSpan timeSpan) => timeSpan.Days >= 1;

		private static TimeSpan getMaxEndTime() => TimeSpan.FromDays(1).Subtract(TimeSpan.FromSeconds(1));
	}
}