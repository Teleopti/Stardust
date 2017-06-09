using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class ScheduleMinMaxTimeCalculator : IScheduleMinMaxTimeCalculator
	{
		private readonly IToggleManager _toggles;
		private readonly INow _now;
		private readonly ISiteOpenHourProvider _siteOpenHourProvider;
		private readonly IScheduledSkillOpenHourProvider _scheduledSkillOpenHourProvider; 

		public ScheduleMinMaxTimeCalculator(IToggleManager toggles, INow now, ISiteOpenHourProvider siteOpenHourProvider, IScheduledSkillOpenHourProvider scheduledSkillOpenHourProvider)
		{
			_toggles = toggles;
			_now = now;
			_siteOpenHourProvider = siteOpenHourProvider;
			_scheduledSkillOpenHourProvider = scheduledSkillOpenHourProvider;
		}

		public void AdjustScheduleMinMaxTime(WeekScheduleDomainData weekDomainData)
		{
			var minMaxTimeFixed = fixWeekScheduleMinMaxTime(weekDomainData);
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
			var minMaxTimeFixed = fixDayScheduleMinMaxTime(dayDomainData);

			if (minMaxTimeFixed)
			{
				dayDomainData.ScheduleDay.MinMaxTime = dayDomainData.MinMaxTime;
			}
		}

		private bool fixDayScheduleMinMaxTime(DayScheduleDomainData dayDomainData)
		{
			var openHourPeriod = _siteOpenHourProvider.GetSiteOpenHourPeriod(dayDomainData.Date);
			if (!openHourPeriod.HasValue)
			{
				openHourPeriod = _scheduledSkillOpenHourProvider.GetSkillOpenHourPeriod(dayDomainData.ScheduleDay.ScheduleDay);
			}
			if (!openHourPeriod.HasValue)
				return false;
			var newTimelinePeriod = getTimelinePeriod(dayDomainData, openHourPeriod.Value);
			if (dayDomainData.MinMaxTime == newTimelinePeriod)
			{
				return false;
			}

			dayDomainData.MinMaxTime = newTimelinePeriod;
			return true;
		}

		private bool fixWeekScheduleMinMaxTime(WeekScheduleDomainData scheduleDomainData)
		{ 
			var siteOpenHourPeriod = getSiteOpenHourPeriod(scheduleDomainData);
			
			var	skillOpenHourPeriod =
					_scheduledSkillOpenHourProvider.GetMergedSkillOpenHourPeriod(
						scheduleDomainData.Days.Select(a => a.ScheduleDay).ToList());

			if (!siteOpenHourPeriod.HasValue && !skillOpenHourPeriod.HasValue)
			{
				return false;
			}

			var openHourPeriod = mergeOpenHourPeriod(siteOpenHourPeriod, skillOpenHourPeriod);

			var newTimelinePeriod = getTimelinePeriod(scheduleDomainData, openHourPeriod);
			if (scheduleDomainData.MinMaxTime == newTimelinePeriod)
			{
				return false;
			}
			scheduleDomainData.MinMaxTime = newTimelinePeriod;
			return true;
		}

		private static TimePeriod mergeOpenHourPeriod(TimePeriod? siteOpenHourPeriod, TimePeriod? skillOpenHourPeriod)
		{
			TimePeriod openHourPeriod;
			if (!siteOpenHourPeriod.HasValue)
			{
				openHourPeriod = skillOpenHourPeriod.Value;
			}
			else if (!skillOpenHourPeriod.HasValue)
			{
				openHourPeriod = siteOpenHourPeriod.Value;
			}
			else
			{
				var minStartTime = new TimeSpan[] {siteOpenHourPeriod.Value.StartTime, skillOpenHourPeriod.Value.StartTime}.Min();
				var maxEndTime = new TimeSpan[] {siteOpenHourPeriod.Value.EndTime, skillOpenHourPeriod.Value.EndTime}.Max();
				openHourPeriod = new TimePeriod(minStartTime, maxEndTime);
			}
			return openHourPeriod;
		}

		private TimePeriod? getSiteOpenHourPeriod(WeekScheduleDomainData scheduleDomainData)
		{
			TimePeriod? siteOpenHourPeriod;
			if (_toggles.IsEnabled(Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880))
			{
				var weekPeriod = DateHelper.GetWeekPeriod(scheduleDomainData.Date, CultureInfo.CurrentCulture);
				if (weekPeriod.StartDate > _now.ServerDate_DontUse().AddDays(ScheduleStaffingPossibilityConsts.MaxAvailableDays))
				{
					return null;
				}
				siteOpenHourPeriod = _siteOpenHourProvider.GetMergedSiteOpenHourPeriod(weekPeriod);
			}
			else
			{
				siteOpenHourPeriod = _siteOpenHourProvider.GetSiteOpenHourPeriod(_now.ServerDate_DontUse());
			}
			return siteOpenHourPeriod;
		}

		private static TimePeriod getTimelinePeriod(BaseScheduleDomainData scheduleDomainData, TimePeriod openHourPeriod)
		{
			var scheduleMinMaxTime = scheduleDomainData.MinMaxTime;
			var minTime = scheduleMinMaxTime.StartTime;
			var maxTime = scheduleMinMaxTime.EndTime;

			var margin = TimeSpan.FromMinutes(ScheduleConsts.TimelineMarginInMinute);
			var early = openHourPeriod.StartTime.Ticks > TimeSpan.Zero.Add(margin).Ticks ? openHourPeriod.StartTime.Subtract(margin) : TimeSpan.Zero;
			var late = openHourPeriod.EndTime.Ticks < new TimeSpan(23, 59, 59).Subtract(margin).Ticks ? openHourPeriod.EndTime.Add(margin) : new TimeSpan(23, 59, 59);

			var adjustedOpenHourPeriod = new TimePeriod(early, late);
			if (adjustedOpenHourPeriod.StartTime < minTime)
			{
				minTime = adjustedOpenHourPeriod.StartTime;
			}
			if (adjustedOpenHourPeriod.EndTime > maxTime)
			{
				maxTime = adjustedOpenHourPeriod.EndTime;
			}

			return minTime == scheduleMinMaxTime.StartTime && maxTime == scheduleMinMaxTime.EndTime
				? scheduleMinMaxTime
				: new TimePeriod(minTime, maxTime);
		}
	}
}