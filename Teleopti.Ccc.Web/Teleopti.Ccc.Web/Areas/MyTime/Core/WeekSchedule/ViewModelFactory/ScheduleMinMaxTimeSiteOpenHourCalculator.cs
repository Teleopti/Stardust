using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class ScheduleMinMaxTimeSiteOpenHourCalculator : IScheduleMinMaxTimeSiteOpenHourCalculator
	{
		private readonly IToggleManager _toggles;
		private readonly INow _now;
		private readonly ILoggedOnUser _loggedOnUser;

		public ScheduleMinMaxTimeSiteOpenHourCalculator(IToggleManager toggles, INow now, ILoggedOnUser loggedOnUser)
		{
			_toggles = toggles;
			_now = now;
			_loggedOnUser = loggedOnUser;
		}

		public void AdjustScheduleMinMaxTime(WeekScheduleDomainData weekDomainData)
		{
			var minMaxTimeFixed = fixScheduleMinMaxTimeBySiteOpenHour(weekDomainData);
			if (minMaxTimeFixed && weekDomainData.Days != null)
			{
				foreach (var day in weekDomainData.Days)
				{
					day.MinMaxTime = weekDomainData.MinMaxTime;
				}
			}
		}

		public void AdjustScheduleMinMaxTime(DayScheduleDomainData dayDomainData)
		{
			var minMaxTimeFixed = fixScheduleMinMaxTimeBySiteOpenHour(dayDomainData);
			if (minMaxTimeFixed)
			{
				dayDomainData.ScheduleDay.MinMaxTime = dayDomainData.MinMaxTime;
			}
		}

		private bool fixScheduleMinMaxTimeBySiteOpenHour(BaseScheduleDomainData scheduleDomainData)
		{
			TimePeriod? siteOpenHourPeriod;

			if (scheduleDomainData is DayScheduleDomainData)
			{
				siteOpenHourPeriod = getSiteOpenHourPeriod(scheduleDomainData.Date);
			}
			else if (_toggles.IsEnabled(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880))
			{
				siteOpenHourPeriod = getWeekSiteOpenHourPeriod();
			}
			else
			{
				siteOpenHourPeriod = getSiteOpenHourPeriod(_now.LocalDateOnly());
			}

			if (!siteOpenHourPeriod.HasValue)
			{
				return false;
			}

			var newTimelinePeriod = getTimelinePeriod(scheduleDomainData, (TimePeriod)siteOpenHourPeriod);
			if (scheduleDomainData.MinMaxTime == newTimelinePeriod)
			{
				return false;
			}

			scheduleDomainData.MinMaxTime = newTimelinePeriod;
			return true;
		}

		private TimePeriod? getWeekSiteOpenHourPeriod()
		{
			var timePeriods = new List<TimePeriod>();
			var weekPeriod = DateHelper.GetWeekPeriod(_now.LocalDateOnly(), CultureInfo.CurrentCulture);
			foreach (var day in weekPeriod.DayCollection())
			{
				var siteOpenHourPeriod = getSiteOpenHourPeriod(day);

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

		private static TimeSpan getMaxEndTime() => TimeSpan.FromDays(1).Subtract(TimeSpan.FromMinutes(1));

		private static bool isCrossDay(TimeSpan timeSpan) => timeSpan.Days >= 1;

		private static TimePeriod getTimelinePeriod(BaseScheduleDomainData scheduleDomainData, TimePeriod siteOpenHourPeriod)
		{
			var scheduleMinMaxTime = scheduleDomainData.MinMaxTime;
			var minTime = scheduleMinMaxTime.StartTime;
			var maxTime = scheduleMinMaxTime.EndTime;
			if (siteOpenHourPeriod.StartTime < minTime)
			{
				minTime = siteOpenHourPeriod.StartTime;
			}
			if (siteOpenHourPeriod.EndTime > maxTime)
			{
				maxTime = siteOpenHourPeriod.EndTime;
			}

			return minTime == scheduleMinMaxTime.StartTime && maxTime == scheduleMinMaxTime.EndTime
				? scheduleMinMaxTime
				: new TimePeriod(minTime, maxTime);
		}

		private TimePeriod? getSiteOpenHourPeriod(DateOnly date)
		{
			var siteOpenHour = _loggedOnUser.CurrentUser().SiteOpenHour(date);
			if (siteOpenHour == null || siteOpenHour.IsClosed)
			{
				return null;
			}
			return siteOpenHour.TimePeriod;
		}
	}
}