using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class ScheduleWeekMinMaxTimeCalculator : IScheduleWeekMinMaxTimeCalculator
	{
		private readonly INow _now;
		private readonly ISiteOpenHourProvider _siteOpenHourProvider;
		private readonly IScheduledSkillOpenHourProvider _scheduledSkillOpenHourProvider; 

		public ScheduleWeekMinMaxTimeCalculator(INow now, ISiteOpenHourProvider siteOpenHourProvider, IScheduledSkillOpenHourProvider scheduledSkillOpenHourProvider)
		{
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
				var minStartTime = new[] {siteOpenHourPeriod.Value.StartTime, skillOpenHourPeriod.Value.StartTime}.Min();
				var maxEndTime = new[] {siteOpenHourPeriod.Value.EndTime, skillOpenHourPeriod.Value.EndTime}.Max();
				openHourPeriod = new TimePeriod(minStartTime, maxEndTime);
			}
			return openHourPeriod;
		}

		private TimePeriod? getSiteOpenHourPeriod(WeekScheduleDomainData scheduleDomainData)
		{
			TimePeriod? siteOpenHourPeriod;
			var weekPeriod = DateHelper.GetWeekPeriod(scheduleDomainData.Date, CultureInfo.CurrentCulture);
			if (weekPeriod.StartDate > _now.ServerDate_DontUse().AddDays(ScheduleStaffingPossibilityConsts.MaxAvailableDays))
			{
				return null;
			}
			siteOpenHourPeriod = _siteOpenHourProvider.GetMergedSiteOpenHourPeriod(weekPeriod);
			return siteOpenHourPeriod;
		}

		private static TimePeriod getTimelinePeriod(BaseScheduleDomainData scheduleDomainData, TimePeriod openHourPeriod)
		{
			var scheduleMinMaxTime = scheduleDomainData.MinMaxTime;
			var minTime = scheduleMinMaxTime.StartTime;
			var maxTime = scheduleMinMaxTime.EndTime;

			var margin = TimeSpan.FromMinutes(ScheduleConsts.TimelineMarginInMinute);
			var openHourPeriodStartTime = openHourPeriod.StartTime;
			if (openHourPeriod.EndTime.Days > 0)
				openHourPeriodStartTime = TimeSpan.Zero;
			var early = openHourPeriodStartTime.Ticks > TimeSpan.Zero.Add(margin).Ticks ? openHourPeriodStartTime.Subtract(margin) : TimeSpan.Zero;
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